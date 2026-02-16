using ERP_Proflipper_ProjectService.Repositories.Interface;
using ERP_Proflipper_ProjectService.Repositories.Ports;
using ERP_Proflipper_ProjectService.Services;
using ERP_Proflipper_ProjectService;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using static Google.Apis.Requests.BatchRequest;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;
using DotNetEnv;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions => {
    serverOptions.ListenAnyIP(Convert.ToInt32(Environment.GetEnvironmentVariable("PROJECT_SERVICE_PORT")));
    serverOptions.ConfigureHttpsDefaults(httpsOptions => {
        httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
    });
});

builder.Services.AddControllersWithViews();

builder.Services.AddCors();
builder.Services.AddHttpClient();
builder.Services.AddTransient<ProjectService>();

builder.Services.AddDbContext<ProjectsDB>(options =>
{
    options.UseNpgsql($"Host={Environment.GetEnvironmentVariable("DB_HOST")}; Port={Environment.GetEnvironmentVariable("DB_PORT")}; Database=ERP_USERS; Username={Environment.GetEnvironmentVariable("DB_USER")}; Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}; Encoding=UTF8; Pooling=true");
    //options.LogTo(message => { }, LogLevel.None);


    //if (!builder.Environment.IsDevelopment())
    //{
    //    options.EnableSensitiveDataLogging(false);
    //    options.LogTo(_ => { }, LogLevel.None);
    //}

    //options.EnableSensitiveDataLogging(false);
    ////options.
    ////options.EnableSensitiveDataLogging();
}, ServiceLifetime.Scoped);

builder.Services.AddHealthChecks().AddDbContextCheck<ProjectsDB>().AddManualHealthCheck().AddApplicationLifecycleHealthCheck();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ValidateLifetime = true,

            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });



builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OnlyForPM", policy => policy.RequireRole("ProjectManager"));
    options.AddPolicy("OnlyForBuilder", policy => policy.RequireRole("Builder"));
    options.AddPolicy("OnlyForFinancier", policy => policy.RequireRole("Financier"));
    options.AddPolicy("OnlyForLawyer", policy => policy.RequireRole("Lawyer"));
});

builder.Services.AddTransient<IProjectRepository, ProjectRepository>();




builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService(serviceName: "ProjectService", serviceVersion: "1.0.0"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://jaeger:4317");
            });
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProjectsDB>();
    await dbContext.Database.MigrateAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseCors(builder => builder
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowAnyOrigin());

// app.MapHealthChecks("/healthcheck");
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();  