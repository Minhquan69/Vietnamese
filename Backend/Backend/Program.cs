using Backend.Data;
using Backend.Repository;
using Backend.Repository.impl;
using Backend.Services;
using Backend.Services.impl;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Backend.Common;

var builder = WebApplication.CreateBuilder(args);


var key = Encoding.UTF8.GetBytes(
    "vietnamese_learning_system_super_secret_key_2026_project"
);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});


// ADD CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));
builder.Services.AddScoped<JwtService>();

builder.Services.AddControllers();

builder.Services.AddScoped<UserRepository, UserRepositoryImpl>();
builder.Services.AddHttpClient<VideoRepositoryImpl>();
builder.Services.AddScoped<VideoRepository, VideoRepositoryImpl>();

builder.Services.AddScoped<UserService, UserServiceImpl>();
builder.Services.AddScoped<VideoService, VideoServiceImpl>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserContextUtil>();

var app = builder.Build();

app.UseCors("AllowAngular");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();