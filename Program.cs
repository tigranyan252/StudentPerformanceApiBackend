using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using AutoMapper;
using StudentPerformance.Api.Data;
using StudentPerformance.Api.Services;
using StudentPerformance.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using StudentPerformance.Api.Data.Entities;
using StudentPerformance.Api.Utilities;
using System;
using Npgsql;
using StudentPerformance.Api; // Если это ваш корневой namespace, оставьте его

var builder = WebApplication.CreateBuilder(args);

// --- 1. Configure Services ---
builder.Services.AddControllers();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<ITeacherSubjectGroupAssignmentService, TeacherSubjectGroupAssignmentService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddScoped<IPasswordHasher<User>, SimplePasswordHasher>();

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// --- Получение строки подключения ---
string connectionString;

var pgHost = Environment.GetEnvironmentVariable("PGHOST");
var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
var pgUser = Environment.GetEnvironmentVariable("PGUSER");
var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");
var pgPort = Environment.GetEnvironmentVariable("PGPORT");

if (string.IsNullOrEmpty(pgHost) ||
    string.IsNullOrEmpty(pgDatabase) ||
    string.IsNullOrEmpty(pgUser) ||
    string.IsNullOrEmpty(pgPassword) ||
    string.IsNullOrEmpty(pgPort))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Fatal Error: Database connection details are incomplete. Please ensure PGHOST, PGDATABASE, PGUSER, PGPASSWORD, PGPORT environment variables are set on Render, or 'DefaultConnection' is configured locally.");
    }
}
else
{
    connectionString = $"Host={pgHost};Port={pgPort};Username={pgUser};Password={pgPassword};Database={pgDatabase};SSL Mode=Require;Trust Server Certificate=true";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- 2. JWT Authentication Setup ---
var jwtSecret = Environment.GetEnvironmentVariable("JwtSettings__Secret");
var jwtIssuer = Environment.GetEnvironmentVariable("JwtSettings__Issuer");
var jwtAudience = Environment.GetEnvironmentVariable("JwtSettings__Audience");
var accessTokenExpirationMinutes = builder.Configuration.GetValue<int>("JwtSettings:ExpirationMinutes", 60);

if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("JWT Secret is not configured in environment variables. Please set 'JwtSettings__Secret'.");
}
if (string.IsNullOrEmpty(jwtIssuer))
{
    throw new InvalidOperationException("JWT Issuer is not configured in environment variables. Please set 'JwtSettings__Issuer'.");
}
if (string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT Audience is not configured in environment variables. Please set 'JwtSettings__Audience'.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.FromMinutes(accessTokenExpirationMinutes)
        };
    });

// --- 3. Authorization Setup ---
builder.Services.AddAuthorization();

// --- 4. Swagger/OpenAPI Setup ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Student Performance API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// --- 5. CORS Setup ---
const string AllowReactAppSpecificOrigins = "_allowReactAppSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowReactAppSpecificOrigins, policy =>
    {
        var allowedOriginsFromEnv = Environment.GetEnvironmentVariable("CorsSettings__AllowedOrigins");
        string[] originsToUse;

        if (!string.IsNullOrEmpty(allowedOriginsFromEnv) && allowedOriginsFromEnv != "*")
        {
            // Используем список из переменной среды
            originsToUse = allowedOriginsFromEnv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(o => o.Trim())
                                                .ToArray();
        }
        else
        {
            // Fallback для локальной разработки или если переменная среды не задана/некорректна
            // Используем значения из appsettings.json
            var localAllowedOrigins = builder.Configuration["CorsSettings:AllowedOrigins"];
            if (!string.IsNullOrEmpty(localAllowedOrigins))
            {
                originsToUse = localAllowedOrigins.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                  .Select(o => o.Trim())
                                                  .ToArray();
            }
            else
            {
                // Крайний случай: если нет ни переменной среды, ни настройки в appsettings.json,
                // используем только localhost для разработки.
                originsToUse = new[] { "http://localhost:3000", "https://localhost:7242" };
            }
        }

        // ВСЕГДА используем AllowCredentials, поэтому * невозможен.
        // Мы уже убедились, что originsToUse не содержит *
        policy.WithOrigins(originsToUse)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});


var app = builder.Build();

// --- 6. Data Seeding Section ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        DataSeeder.SeedData(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
// --- End Data Seeding Section ---

// --- Включаем Swagger UI для всех сред ---
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(AllowReactAppSpecificOrigins);

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
