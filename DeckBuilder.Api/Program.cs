using System.Text;
using DeckBuilder.Api.Configurations;
using DeckBuilder.Api.Exceptions;
using DeckBuilder.Api.Routes;
using DeckBuilder.Api.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Deck Builder API",
        Description = "Deck builder API implementation",
        TermsOfService = new Uri("http://www.deckbuilder.com"),
        Contact = new OpenApiContact
        {
            Name = "Nicolas Fernandes",
            Email = "nicolaspfernandes@outlook.com",
            Url = new Uri("http://www.napfsolutions.com")
        },
        License = new OpenApiLicense
        {
            Name = "Free License",
            Url = new Uri("http://www.napfsolutions.com")
        }
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JSON Web Token based security",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var configuration = builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
    .AddEnvironmentVariables()
    .Build();

var jwtConfiguration = new JwtConfiguration(configuration);
var mongoConfiguration = new MongoConfiguration(configuration);

builder.Services.AddSingleton(jwtConfiguration);
builder.Services.AddSingleton(mongoConfiguration);
builder.Services.AddSingleton<IMemoryCache>(_ => new MemoryCache(new MemoryCacheOptions()));
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
builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        var secretKeyBytes = Convert.FromBase64String(jwtConfiguration.Secret);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfiguration.Issuer,
            ValidAudience = jwtConfiguration.Audience,
            ValidateIssuer = !string.IsNullOrWhiteSpace(jwtConfiguration.Issuer),
            ValidateAudience = !string.IsNullOrWhiteSpace(jwtConfiguration.Audience),
            IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes)
        };
    });

builder.Services.AddCors(corsOptions =>
{
    corsOptions.AddPolicy("CorsPolicy", policyBuilder =>
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
app.UseAuthentication();
app.UseAuthorization();

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