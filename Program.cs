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
using StudentPerformance.Api; // Убедитесь, что эта директива есть

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

// --- КЛЮЧЕВОЕ ИЗМЕНЕНИЕ: Получение и парсинг строки подключения из отдельных переменных Render ---
string connectionString;

// Пытаемся получить отдельные компоненты строки подключения из переменных среды Render
var pgHost = Environment.GetEnvironmentVariable("PGHOST");
var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
var pgUser = Environment.GetEnvironmentVariable("PGUSER");
var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");
var pgPort = Environment.GetEnvironmentVariable("PGPORT"); // Это будет string

// Если все эти переменные установлены, строим строку подключения
if (!string.IsNullOrEmpty(pgHost) &&
    !string.IsNullOrEmpty(pgDatabase) &&
    !string.IsNullOrEmpty(pgUser) &&
    !string.IsNullOrEmpty(pgPassword) &&
    !string.IsNullOrEmpty(pgPort))
{
    connectionString = $"Host={pgHost};Port={pgPort};Username={pgUser};Password={pgPassword};Database={pgDatabase};SSL Mode=Require;Trust Server Certificate=true";
}
else
{
    // Если отдельные переменные Render не установлены, пытаемся использовать DATABASE_URL
    // (это для совместимости или локальной разработки, если вы не используете PG* переменные локально)
    var databaseUrl = builder.Configuration.GetValue<string>("DATABASE_URL");
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        var username = userInfo[0];
        var password = userInfo[1];
        var host = uri.Host;
        var port = uri.Port;
        var database = uri.Segments[1];

        connectionString = $"Host={host};Port={port};Username={username};Password={password};Database={database.TrimEnd('/')};SSL Mode=Require;Trust Server Certificate=true";
    }
    else
    {
        // В крайнем случае, для локальной разработки, используем DefaultConnection
        connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    }
}

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string cannot be constructed. Neither individual PG* environment variables nor DATABASE_URL nor DefaultConnection are properly set.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- 2. JWT Authentication Setup ---
// Эти части кода остались без изменений, они уже используют Environment.GetEnvironmentVariable()
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
        var allowedOrigins = builder.Configuration["CorsSettings:AllowedOrigins"]?
                                 .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(o => o.Trim())
                                 .ToArray();

        if (allowedOrigins != null && allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
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
