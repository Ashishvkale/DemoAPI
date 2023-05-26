using Aticas.CoreAPI.Services;
using Aticas.CoreAPI.Models;
using Aticas.ObjectModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using System.Text;
using Autodesk.Forge.Client;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
Settings settings = config.GetRequiredSection("Settings").Get<Settings>();

//builder.Services.AddControllers()
//    .AddJsonOptions(option => option.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);

//builder.Services.AddControllers()
//    .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);
//alternative return Json(obj, new JsonSerializerOptions { PropertyNamingPolicy = null });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
                          policy =>
                          {
                              policy.WithOrigins(new string[] {"http://localhost:4200", "http://localhost:5000"});
                              policy.AllowAnyHeader();
                              policy.AllowAnyMethod();
                              policy.AllowCredentials();
                          });
});


builder.Services.AddControllers().AddNewtonsoftJson(options => { options.SerializerSettings.ContractResolver = new DefaultContractResolver(); });
var ForgeClientID = "ZvhttNGLOVlEpA3egh5AXbKw5vAzWcNj";
var ForgeClientSecret = "EG98v6jaCcYZPKLb";
var ForgeCallbackURL = "http://localhost:5000/api/auth/callback";

builder.Services.AddSingleton<ForgeService>(new ForgeService(ForgeClientID, ForgeClientSecret, ForgeCallbackURL));
builder.Services.Configure<FormOptions>(options => { options.MultipartBodyLengthLimit = 250000000; });

//program.cs
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = null;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenManagerMiddleware>();
builder.Services.AddScoped<ISessionManager, SessionManager>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

builder.Services.AddMemoryCache();
builder.Services.AddSwaggerGen(options =>
{
    options.IncludeXmlComments(xmlPath);
    options.EnableAnnotations();
    options.SwaggerDoc("1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Aticas API Service",
        Description = "Welcome to the Aticas API Service",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(settings.JwtSettings.JwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

#region Api Versioning

// Add API Versioning to the Project
builder.Services.AddApiVersioning(config =>
{
    // Specify the default API Version as 1.0
    config.DefaultApiVersion = new ApiVersion(1, 0);
    // If the client hasn't specified the API version in the request, use the default API version number
    config.AssumeDefaultVersionWhenUnspecified = true;
    // Advertise the API versions supported for the particular endpoint
    config.ReportApiVersions = true;
});

#endregion Api Versioning

var app = builder.Build();

app.UseSwagger(options =>
{
    options.RouteTemplate = "api/{documentname}/descriptor.json";
});
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "Aticas API Service";
    options.InjectStylesheet("/css/docs.css");
    options.InjectJavascript("/js/docs.js");
    options.SwaggerEndpoint("/api/1/descriptor.json", "Version 1");
    options.RoutePrefix = string.Empty;
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.UseCors(MyAllowSpecificOrigins);
app.UseMiddleware<TokenManagerMiddleware>();

app.UseAuthorization();

app.MapControllers();


app.Run();













/*using Aticas.CoreAPI.Models;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using Autodesk.Forge.Client;

namespace Demo_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                                      policy =>
                                      {
                                          policy.WithOrigins("http://localhost:5000,http://localhost:4200")
                                                              .AllowAnyHeader()
                                                              .AllowAnyMethod()
                                                              .AllowAnyOrigin();
                                      });
            });
            builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
                .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver
                = new DefaultContractResolver());

            builder.Services.AddControllers();
            var ForgeClientID = "ZvhttNGLOVlEpA3egh5AXbKw5vAzWcNj";
            var ForgeClientSecret = "EG98v6jaCcYZPKLb";
            var ForgeCallbackURL = "http://localhost:5000/api/auth/callback";

            builder.Services.AddSingleton<ForgeService>(new ForgeService(ForgeClientID, ForgeClientSecret, ForgeCallbackURL));
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors(MyAllowSpecificOrigins);

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}*/