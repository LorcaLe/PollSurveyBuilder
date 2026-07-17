using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PollSurveyBuilder.Application.IServices;
using PollSurveyBuilder.Domain.Entities.Identity;
using PollSurveyBuilder.Infrastructure.Jobs;
using PollSurveyBuilder.Infrastructure.Persistence;
using PollSurveyBuilder.Infrastructure.Services;

namespace PollSurveyBuilder.Infrastructure
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            // "DatabaseProvider": "SqlServer" | "Postgres" in appsettings.json - the brief
            // allows either, so the team can point this at whichever engine their PaaS offers.
            var provider = config["DatabaseProvider"] ?? "SqlServer";
            var connectionString = config.GetConnectionString("Default");

            services.AddDbContext<AppDbContext>(options =>
            {
                if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
                    options.UseNpgsql(connectionString);
                else
                    options.UseSqlServer(connectionString);
            });

            services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // Redis-backed distributed cache for poll results (falls back to whatever
            // "Redis" connection string points at - see docker-compose.yml for local dev).
            var redisConnection = config.GetConnectionString("Redis");
            if (!string.IsNullOrWhiteSpace(redisConnection))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = "pollsurvey:";
                });
            }
            else
            {
                // Dev fallback so the app still runs without Redis installed locally.
                services.AddDistributedMemoryCache();
            }

            services.AddScoped<IPollService, PollService>();
            services.AddScoped<IVoteService, VoteService>();
            services.AddScoped<IQRCodeService, QRCodeService>();
            services.AddScoped<ITokenService, TokenService>();

            services.AddHostedService<PollExpiryJob>();

            return services;
        }
    }
}
