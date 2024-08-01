using DeckBuilder.Api.Configurations;
using DeckBuilder.Api.Exceptions;
using DeckBuilder.Api.Routes;
using DeckBuilder.Api.Services;
using Microsoft.Extensions.FileProviders;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var configuration = builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
    .AddEnvironmentVariables()
    .Build();

var jwtConfiguration = new JwtConfiguration(configuration);
var mongoConfiguration = new MongoConfiguration(configuration);

builder.Services.AddSingleton(jwtConfiguration);
builder.Services.AddSingleton(mongoConfiguration);
builder.Services.AddScoped<CardService>();
builder.Services.AddScoped<DeckService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ImportService>();
builder.Services.AddScoped<DeckCreationService>();
builder.Services.AddScoped<IMongoDatabase>(_ =>
{
    var settings = MongoClientSettings.FromUrl(new MongoUrl(mongoConfiguration.ConnectionUri));

    settings.ClusterConfigurator = clusterBuilder =>
    {
        clusterBuilder.Subscribe<CommandStartedEvent>(startedEvent => { Console.WriteLine(startedEvent.Command); });
    };

    var client = new MongoClient(settings);
    var database = client.GetDatabase(mongoConfiguration.DatabaseName);

    return database;
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policyBuilder =>
    {
        policyBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.RegisterSetEndpoints();
app.RegisterCardEndpoints();
app.RegisterDeckEndpoints();
app.RegisterUserEndpoints();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/new/harry-potter-tcg/images")),
    RequestPath = "/assets/images"
});
    
app.Run();