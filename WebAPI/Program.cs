using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Domain.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddOpenApi();

            // Подключение к БД SQLite
            builder.Services.AddDbContext<AppDB>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("Infrastructure")));

            // Регистрация репозиториев и сервисов 
            builder.Services.AddScoped<Domain.Interfaces.ISupportRequestRepository, Infrastructure.Repositories.SupportRequestRepository>();
            builder.Services.AddScoped<ISupportService, SupportService>();

            // Включение кэширования в оперативной памяти 
            builder.Services.AddMemoryCache();

            // Настройка JWT Аутентификации
            var key = Encoding.ASCII.GetBytes("SuperSecretSecureKey1234567890KeyForHelpdeskAPI!");
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
            });

            var app = builder.Build();

            // Автоматическое создание базы данных и таблиц при старте приложения
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<Infrastructure.Data.AppDB>();
                dbContext.Database.EnsureCreated();
            }

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            // !!! ВАЖНО:Сначала аутентификация, затем авторизация !!! 
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}