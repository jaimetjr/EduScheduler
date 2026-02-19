using Azure.Identity;
using EduScheduler.Api.Data;
using EduScheduler.Api.Jobs;
using EduScheduler.Api.Services;
using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
    };
});

builder.Services.AddAuthorization();

var graphConfig = builder.Configuration.GetSection("MicrosoftGraph");
var credential = new ClientSecretCredential(
    graphConfig["TenantId"],
    graphConfig["ClientId"],
    graphConfig["ClientSecret"]);

builder.Services.AddSingleton(new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" }));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IGraphSyncService, GraphSyncService>();
builder.Services.AddTransient<GraphSyncJob>();

builder.Services.AddHangfire(config => config.UseSQLiteStorage("hangfire.db"));
builder.Services.AddHangfireServer();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite default
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "EduScheduler API v1");
    });
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

var syncInterval = int.Parse(builder.Configuration["Sync:IntervalInMinutes"] ?? "30");
RecurringJob.AddOrUpdate<GraphSyncJob>(
    "graph-sync",
    job => job.ExecuteAsync(),
    $"*/{syncInterval} * * * *"); // Every N minutes

app.UseHttpsRedirection();
app.Run();