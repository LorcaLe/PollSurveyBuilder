using FluentValidation;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using PollSurveyBuilder.API.Hubs;
using PollSurveyBuilder.Application.Validators;
using PollSurveyBuilder.Infrastructure;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ---- Persistence, Identity, Redis cache, domain services (see InfrastructureDependencyInjection) ----
builder.Services.AddInfrastructure(builder.Configuration);

// ---- FluentValidation ----
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePollValidator>();

// ---- Controllers + OpenAPI ----
// Enums (PollType, PollStatus) must serialize as strings ("SingleChoice", not 0) -
// the React frontend compares against string names, and this keeps both
// the REST responses and SignalR's JSON payloads consistent with each other.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

// ---- SignalR (live vote broadcasting) ----
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ---- JWT auth (creator dashboard / poll management; voting itself never needs a login) ----
// AddIdentity() (called inside AddInfrastructure above) registers its own cookie scheme
// and quietly claims DefaultChallengeScheme for itself. Without explicitly overriding all
// three scheme properties here, [Authorize] failures get handled by Identity's cookie
// handler (redirecting to /Account/Login, a page that doesn't exist in this API-only
// project) instead of returning a clean 401 from the JWT bearer handler.
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        };

        // SignalR sends the JWT as a query string param (browsers can't set
        // Authorization headers on WebSocket upgrade requests), so accept it there too.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/hubs/polls"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
});

// ---- CORS: the React SPA runs on a different origin, and voting relies on a
// cookie (voter_token), so credentials must be explicitly allowed. ----
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

if (allowedOrigins == null)
{
    var singleOrigin = builder.Configuration["Cors:AllowedOrigins"];
    allowedOrigins = !string.IsNullOrEmpty(singleOrigin) ? new[] { singleOrigin } : new[] { "http://localhost:5173" };
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ---- Rate limiting: voting is the endpoint most worth protecting from abuse
// (script spamming a poll to skew results), so it gets a tighter, dedicated policy. ----
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));

    options.AddFixedWindowLimiter("vote", opts =>
    {
        opts.PermitLimit = 10;
        opts.Window = TimeSpan.FromSeconds(10);
        opts.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("create-poll", opts =>
    {
        opts.PermitLimit = 5;
        opts.Window = TimeSpan.FromMinutes(1);
        opts.QueueLimit = 0;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Poll & Survey Builder API").WithTheme(ScalarTheme.Purple);
    });
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");

// Every visitor (voter or creator) gets a stable anonymous voter_token cookie the
// first time they hit the API. VotesController relies on this to enforce
// one-vote-per-browser without requiring an account.
app.Use(async (context, next) =>
{
    const string cookieName = "voter_token";
    var token = context.Request.Cookies[cookieName];
    if (string.IsNullOrEmpty(token))
    {
        token = Guid.NewGuid().ToString("N");
        context.Response.Cookies.Append(cookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !app.Environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddYears(1),
        });
    }
    // Stash it on HttpContext.Items so controllers can read it regardless of
    // whether it was just-created (not yet in Request.Cookies) or pre-existing.
    context.Items[cookieName] = token;
    await next();
});

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PollHub>("/hubs/polls");

app.Run();