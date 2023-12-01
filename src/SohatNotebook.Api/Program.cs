using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SohatNotebook.Authentication.Configuration;
using SohatNotebook.DataService.Data;
using SohatNotebook.DataService.IConfiguration;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Update the JWT config from the settings.
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));

// Add DbContext for the API
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adding dependency injection for UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Enabling Api Versioning
builder.Services.AddApiVersioning(opt =>
{
    // Provides to the client the different Api versions that we have. 
    opt.ReportApiVersions = true;

    // This will allow the api to automatically provide a default version.
    opt.AssumeDefaultVersionWhenUnspecified = true;

    opt.DefaultApiVersion = ApiVersion.Default;
});

// Getting the secret from the config.
var key = Encoding.ASCII.GetBytes(builder.Configuration["JwtConfig:Secret"]!);

var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuer = false, // ToDo Update
    ValidateAudience = false,  // ToDo Update
    RequireExpirationTime = false, // ToDo Update
    ValidateLifetime = true
};

// Injecting into our DI container
builder.Services.AddSingleton(tokenValidationParameters);

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(jwt =>
    {
        jwt.SaveToken = true;
        jwt.TokenValidationParameters = tokenValidationParameters;
    });

builder.Services.AddDefaultIdentity<IdentityUser>(options 
    => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
