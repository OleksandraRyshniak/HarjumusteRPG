using System.Reflection;
using System.Text;
using HarjumusteRPG.API.Data;
using HarjumusteRPG.API.Infrastructure;
using HarjumusteRPG.API.Repositories;
using HarjumusteRPG.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using HarjumusteRPG.API.Realtime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IHabitRepository, HabitRepository>();
builder.Services.AddScoped<IHabitCompletionRepository, HabitCompletionRepository>();
builder.Services.AddScoped<IHabitService, HabitService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<ICompletionService, CompletionService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<IDailyGiftService, DailyGiftService>();
builder.Services.AddScoped<IStreakService, StreakService>();
builder.Services.AddScoped<IFriendshipRepository, FriendshipRepository>();
builder.Services.AddScoped<IFriendService, FriendService>();
builder.Services.AddSingleton<IConnectionRegistry, ConnectionRegistry>();
builder.Services.AddScoped<ILeaderboardNotifier, LeaderboardNotifier>();
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && context.HttpContext.Request.Path.StartsWithSegments("/ws"))
                    context.Token = token;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// One error format (ProblemDetails) for all errors
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Clients", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Harjumuste RPG API",
        Version = "v1",
        Description = "Habit tracker with XP, levels, streaks and a friends leaderboard."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));

    options.AddSecurityDefinition("basicAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        Description = "Log in with your email and password."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("basicAuth", document)] = new List<string>()
    });

    // Swagger shows only the endpoints that the logged-in user may call
    options.DocumentFilter<RoleVisibilityDocumentFilter>();
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseHttpsRedirection();
app.UseCors("Clients");
app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(30) });
app.UseAuthentication();
app.UseAuthorization();

// Swagger must come after authentication: the document depends on who is asking
if (app.Environment.IsDevelopment())
{
    // the document differs per user, so it must not be cached
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/swagger") &&
            context.Request.Path.Value!.EndsWith(".json"))
            context.Response.Headers.CacheControl = "no-store";
        await next();
    });

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Harjumuste RPG API");
        options.EnablePersistAuthorization();
        options.HeadContent = SwaggerLoginPage.HeadContent;
    });
}

app.MapControllers();
app.MapLeaderboardWebSocket();

app.Run();