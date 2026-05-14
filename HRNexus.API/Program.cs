using System.Security.Claims;
using System.Text;
using HRNexus.API.Security;
using HRNexus.API.Middleware;
using HRNexus.Business.DependencyInjection;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Options;
using HRNexus.Business.Security;
using HRNexus.DataAccess.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IClientIpAddressProvider, ClientIpAddressProvider>();
builder.Services.AddScoped<ICurrentUserContext, HeaderCurrentUserContext>();
builder.Services.AddScoped<IAccessTokenService, JwtAccessTokenService>();
builder.Services.Configure<AuthSecurityOptions>(builder.Configuration.GetSection("AuthSecurity"));
builder.Services.Configure<FileStorageOptions>(options =>
{
    builder.Configuration.GetSection("FileStorage").Bind(options);

    var environmentRootPath = Environment.GetEnvironmentVariable("HRNEXUS_STORAGE_ROOT");
    var configuredRootPath = builder.Configuration["FileStorage:RootPath"];

    if (!string.IsNullOrWhiteSpace(environmentRootPath))
    {
        options.RootPath = environmentRootPath.Trim();
    }
    else if (!string.IsNullOrWhiteSpace(configuredRootPath))
    {
        options.RootPath = configuredRootPath.Trim();
    }
    else if (builder.Environment.IsDevelopment())
    {
        options.RootPath = @"C:\HRNexusStorage";
    }
    else
    {
        throw new InvalidOperationException("File storage root path must be configured via HRNEXUS_STORAGE_ROOT or FileStorage:RootPath.");
    }

    if (options.MaxFileSizeBytes <= 0)
    {
        throw new InvalidOperationException("FileStorage:MaxFileSizeBytes must be greater than zero.");
    }
});

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing.");

var jwtSigningKey = Environment.GetEnvironmentVariable("HRNEXUS_JWT_SIGNING_KEY");

if (string.IsNullOrWhiteSpace(jwtSigningKey))
{
    throw new InvalidOperationException("JWT signing key must be configured via HRNEXUS_JWT_SIGNING_KEY.");
}

if (Encoding.UTF8.GetByteCount(jwtSigningKey) < 32)
{
    throw new InvalidOperationException("HRNEXUS_JWT_SIGNING_KEY must be at least 32 bytes long.");
}

jwtOptions.SigningKey = jwtSigningKey;

builder.Services.Configure<JwtOptions>(options =>
{
    options.Issuer = jwtOptions.Issuer;
    options.Audience = jwtOptions.Audience;
    options.SigningKey = jwtOptions.SigningKey;
    options.AccessTokenExpirationMinutes = jwtOptions.AccessTokenExpirationMinutes;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                await LogAccessDeniedAsync(context.HttpContext, null, "Authentication challenge.", context.HttpContext.RequestAborted);
            },
            OnForbidden = async context =>
            {
                await LogAccessDeniedAsync(
                    context.HttpContext,
                    TryGetUserId(context.HttpContext.User),
                    "Authorization forbidden.",
                    context.HttpContext.RequestAborted);
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicyNames.AuthenticatedUser, policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy(AuthorizationPolicyNames.HrOrAdmin, policy =>
        policy.RequireRole("Admin", "HRManager", "HRClerk"));

    options.AddPolicy(AuthorizationPolicyNames.SecurityAdmin, policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy(AuthorizationPolicyNames.CanReviewLeave, policy =>
        policy.RequireRole("Admin", "HRManager", "DepartmentManager"));

    options.AddPolicy(AuthorizationPolicyNames.SelfOrHr, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
        {
            if (context.User.IsInRole("Admin")
                || context.User.IsInRole("HRManager")
                || context.User.IsInRole("HRClerk"))
            {
                return true;
            }

            var httpContext = context.Resource as HttpContext;
            var routeEmployeeId = httpContext?.Request.RouteValues["employeeId"]?.ToString();
            var claimEmployeeId = context.User.FindFirstValue("employee_id");

            return int.TryParse(routeEmployeeId, out var requestedEmployeeId)
                && int.TryParse(claimEmployeeId, out var currentEmployeeId)
                && requestedEmployeeId > 0
                && requestedEmployeeId == currentEmployeeId;
        });
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("AuthLogin", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("AuthToken", limiterOptions =>
    {
        limiterOptions.PermitLimit = 20;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HRNexus API",
        Version = "v1",
        Description = "HRNexus backend API with Stage 3 JWT authentication foundation."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT bearer token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCors", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var connectionString = builder.Configuration.GetConnectionString("HRNexusDatabase")
    ?? throw new InvalidOperationException("Connection string 'HRNexusDatabase' is not configured.");

builder.Services.AddDataAccess(connectionString);
builder.Services.AddBusinessServices();

var app = builder.Build();


app.UseMiddleware<ApiExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();


app.UseCors("DevelopmentCors");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static int? TryGetUserId(ClaimsPrincipal principal)
{
    var rawUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
    return int.TryParse(rawUserId, out var userId) && userId > 0 ? userId : null;
}

static async Task LogAccessDeniedAsync(HttpContext httpContext, int? userId, string details, CancellationToken cancellationToken)
{
    try
    {
        var activityLogService = httpContext.RequestServices.GetRequiredService<IUserActivityLogService>();
        await activityLogService.LogAsync(
            userId,
            SecurityActivityCodes.AccessDenied,
            false,
            details,
            httpContext.RequestServices.GetRequiredService<IClientIpAddressProvider>().GetClientIpAddress(),
            cancellationToken);
    }
    catch
    {
        // Authorization failure logging must never prevent the auth middleware from returning its response.
    }
}
