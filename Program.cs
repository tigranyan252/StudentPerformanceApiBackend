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
using StudentPerformance.Api; // Убедитесь, что это пространство имен существует, если нет, удалите.

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

// --- ИЗМЕНЕНИЕ ЗДЕСЬ: Configure DbContext for PostgreSQL ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))); // Использование UseNpgsql вместо UseSqlServer

// --- 2. JWT Authentication Setup ---
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
var jwtAudience = builder.Configuration["JwtSettings:Audience"];
// Добавлена настройка времени жизни токена из конфигурации.
// По умолчанию 15 минут, если в appsettings.json не указано.
// Исправлено: "ExpirationMinutes" из appsettings.json
var accessTokenExpirationMinutes = builder.Configuration.GetValue<int>("JwtSettings:ExpirationMinutes", 60); // Получаем ExpirationMinutes

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
            // Увеличена ClockSkew для удобства тестирования.
            // ClockSkew - это допустимое отклонение времени между сервером, выдавшим токен,
            // и сервером, его проверяющим. Это позволяет обрабатывать небольшие рассинхронизации.
            // Установка ClockSkew в TimeSpan.Zero - хорошая практика для продакшена.
            // Для целей тестирования можно оставить большим или установить по ExpirationMinutes
            ClockSkew = TimeSpan.FromMinutes(accessTokenExpirationMinutes) // Используем значение из конфигурации
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
        // Чтение разрешенных источников из конфигурации (appsettings.json или переменных среды)
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
            // Если разрешенные источники не указаны в конфигурации,
            // можно использовать более строгую политику или логировать предупреждение.
            // Для локальной разработки, возможно, вам понадобится "AllowAnyOrigin" временно,
            // но в продакшене это не рекомендуется.
            policy.AllowAnyOrigin() // Осторожно: AllowAnyOrigin несовместим с AllowCredentials
                  .AllowAnyMethod()
                  .AllowAnyHeader();
            // Если вы используете AllowAnyOrigin, вы не можете использовать AllowCredentials.
            // Удалите AllowCredentials если оставляете AllowAnyOrigin для продакшена.
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
        // Применение миграций автоматически при запуске приложения (рекомендуется для Heroku)
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
    // В продакшене рекомендуется использовать HTTPS
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors(AllowReactAppSpecificOrigins); // CORS must be before UseRouting, UseAuthentication, UseAuthorization

app.UseRouting();
app.UseAuthentication(); // JWT authentication
app.UseAuthorization(); // Role-based authorization

app.MapControllers(); // Maps controller routes

app.Run();
