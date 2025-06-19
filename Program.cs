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
using StudentPerformance.Api; // ���������, ��� ��� ������������ ���� ����������, ���� ���, �������.

var builder = WebApplication.CreateBuilder(args);

// --- 1. Configure Services ---
// Add controllers (API controllers)
builder.Services.AddControllers();

// Register custom services for Dependency Injection
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


// Register IPasswordHasher for hashing passwords
builder.Services.AddScoped<IPasswordHasher<User>, SimplePasswordHasher>();

// Configure AutoMapper (MappingProfile to map entities to DTOs)
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// --- ��������� �����: Configure DbContext for PostgreSQL ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))); // ������������� UseNpgsql ������ UseSqlServer

// --- 2. JWT Authentication Setup ---
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
var jwtAudience = builder.Configuration["JwtSettings:Audience"];
// ��������� ��������� ������� ����� ������ �� ������������.
// �� ��������� 15 �����, ���� � appsettings.json �� �������.
// ����������: "ExpirationMinutes" �� appsettings.json
var accessTokenExpirationMinutes = builder.Configuration.GetValue<int>("JwtSettings:ExpirationMinutes", 60); // �������� ExpirationMinutes

if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("JWT Secret is not configured. Please add 'JwtSettings:Secret' to appsettings.json or user secrets.");
}
if (string.IsNullOrEmpty(jwtIssuer))
{
    throw new InvalidOperationException("JWT Issuer is not configured. Please add 'JwtSettings:Issuer' to appsettings.json or user secrets.");
}
if (string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT Audience is not configured. Please add 'JwtSettings:Audience' to appsettings.json or user secrets.");
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
            // ��������� ClockSkew ��� �������� ������������.
            // ClockSkew - ��� ���������� ���������� ������� ����� ��������, �������� �����,
            // � ��������, ��� �����������. ��� ��������� ������������ ��������� ����������������.
            // ��������� ClockSkew � TimeSpan.Zero - ������� �������� ��� ����������.
            // ��� ����� ������������ ����� �������� ������� ��� ���������� �� ExpirationMinutes
            ClockSkew = TimeSpan.FromMinutes(accessTokenExpirationMinutes) // ���������� �������� �� ������������
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
        // ������ ����������� ���������� �� ������������ (appsettings.json ��� ���������� �����)
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
            // ���� ����������� ��������� �� ������� � ������������,
            // ����� ������������ ����� ������� �������� ��� ���������� ��������������.
            // ��� ��������� ����������, ��������, ��� ����������� "AllowAnyOrigin" ��������,
            // �� � ���������� ��� �� �������������.
            policy.AllowAnyOrigin() // ���������: AllowAnyOrigin ����������� � AllowCredentials
                  .AllowAnyMethod()
                  .AllowAnyHeader();
            // ���� �� ����������� AllowAnyOrigin, �� �� ������ ������������ AllowCredentials.
            // ������� AllowCredentials ���� ���������� AllowAnyOrigin ��� ����������.
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
        // ���������� �������� ������������� ��� ������� ���������� (������������� ��� Heroku)
        context.Database.Migrate();
        DataSeeder.SeedData(context); // Make sure DataSeeder.SeedData handles existing data gracefully
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
// --- End Data Seeding Section ---

// --- 7. Configure the HTTP request pipeline (Middleware) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // � ���������� ������������� ������������ HTTPS
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors(AllowReactAppSpecificOrigins); // CORS must be before UseRouting, UseAuthentication, UseAuthorization

app.UseRouting();
app.UseAuthentication(); // JWT authentication
app.UseAuthorization(); // Role-based authorization

app.MapControllers(); // Maps controller routes

app.Run();
