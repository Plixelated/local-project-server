using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using project_model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Load ENV
DotNetEnv.Env.Load();
var _connectionString = Environment.GetEnvironmentVariable("ConnectionStrings_DefaultConnection");
if (string.IsNullOrEmpty(_connectionString))
{
    throw new Exception("Connection String Not Found in .env file");
}

//builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
//    {
//        {"ConnectionStrings:DefaultConnection", _connectionString}
//    });

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<ModelContext>(options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    .EnableSensitiveDataLogging()
    .LogTo(Console.WriteLine, LogLevel.Information)
    ;
    
    });

Console.WriteLine("Loaded connection string: " + builder.Configuration.GetConnectionString("DefaultConnection"));


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
