using System.Text;
using AspNetCoreRateLimit;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using TaskManager.Middlewares;
using TaskManager.Models;
using TaskManager.Services;
using TaskManager.Validators;

public partial class Program
{
  public static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    builder.Services.AddInMemoryRateLimiting();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
      options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
      {
        Title = "Task Manager API",
        Version = "v1",
        Description = "API for managing tasks with authentication and authorization.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
          Name = "Vinícius Carvalho",
          Email = "vinicius.cs01@gmail.com",
          Url = new Uri("https://github.com/Viniciuscs01")
        }
      });

      options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
      {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your JWT token."
      });

      options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
      {
        {
          new Microsoft.OpenApi.Models.OpenApiSecurityScheme
          {
              Reference = new Microsoft.OpenApi.Models.OpenApiReference
              {
                  Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                  Id = "Bearer"
              }
          },
          new string[] {}
        }
      });

      options.OperationFilter<TaskManager.Configurations.SwaggerExamplesFilter>();
      options.OperationFilter<TaskManager.Configurations.RestrictMediaTypeFilter>();
    });

    builder.Services.AddSingleton<LoginAttemptService>();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]));

    builder.Configuration.AddUserSecrets<Program>();

    var secretKey = builder.Configuration["JWT_SECRET"];
    var key = Encoding.ASCII.GetBytes(secretKey);

    builder.Services.AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
      };
    });

    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

    builder.Services.AddResponseCaching();

    builder.Services.Configure<RequestLocalizationOptions>(options =>
    {
      var supportedCultures = new[] { "en", "pt" };
      options.SetDefaultCulture("en");
      options.AddSupportedCultures(supportedCultures);
      options.AddSupportedUICultures(supportedCultures);
    });
    builder.Logging.AddConsole();
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();

    builder.Host.UseSerilog();

    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
      options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    });

    builder.Services.AddControllers();
    builder.Services.AddScoped<IAuditLogService, AuditLogService>();
    builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<TaskValidator>();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
      app.UseSwagger();
      app.UseSwaggerUI();
    }

    app.UseResponseCaching();
    app.UseHttpsRedirection();

    app.UseHsts();
    app.UseXContentTypeOptions();
    app.UseReferrerPolicy(opt => opt.NoReferrer());
    app.UseXXssProtection(opt => opt.EnabledWithBlockMode());
    app.UseXfo(opt => opt.Deny());


    app.UseMiddleware<AuditLoggingMiddleware>();
    app.UseMiddleware<ErrorHandlingMiddleware>();

    var localizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>().Value;
    app.UseRequestLocalization(localizationOptions);
    app.UseIpRateLimiting();
    app.MapControllers();

    app.Run();
  }
}
