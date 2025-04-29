using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using project_model;
using project_server;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( c =>
{
    c.SwaggerDoc("v1", new()
    {
        //Lets swagger to keep the token
        Contact = new()
        {
            Email = "development@plixelated.mozmail.com",
            Name = "Plixel",
            Url = new("https://github.com/Plixelated")
        },
        Description = "APIs for Drake Equation",
        Title = "Drake Equation APIs",
        Version = "V1"
    });
    OpenApiSecurityScheme jwtSecurityScheme = new()
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Please enter *only* JWT token",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, [] }
    });
});

//Load ENV
DotNetEnv.Env.Load();
var _connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
if (string.IsNullOrEmpty(_connectionString))
{
    throw new Exception("Connection String Not Found in .env file");
}

DotNetEnv.Env.Load();
var _securityKey = Environment.GetEnvironmentVariable("JWTSettings_SecurityKey");
if (string.IsNullOrEmpty(_securityKey))
{
    throw new Exception("Security Key Not Found in .env file");
}


//Add ENV Files to Configuration
builder.Configuration.AddEnvironmentVariables();
//Connect to DB
builder.Services.AddDbContext<ModelContext>(options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    .EnableSensitiveDataLogging()
    .LogTo(Console.WriteLine, LogLevel.Information)
    ;
    });

//Establish Microsoft Identity DI and point it to DB
builder.Services.AddIdentity<ProjectUser, IdentityRole>()
    .AddEntityFrameworkStores<ModelContext>();

//Adds authentication
//Validates the JWT Token
builder.Services.AddAuthentication(options =>
{
    //What Is Validated
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    //How it's Validated
    options.TokenValidationParameters = new()
    {
        RequireAudience = true,
        RequireExpirationTime = true,
        ValidateAudience = true,
        ValidateIssuer = true,
        //Checks the password
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["JWTSettings:Issuer"],
        ValidAudience = builder.Configuration["JWTSettings:Audience"],
 
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWTSettings_SecurityKey"])
            ) ?? throw new InvalidOperationException()
    };

});

//Scoped Dependency Injection of JWT Handler
builder.Services.AddScoped<JWTHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(option => option.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
