using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Quartz;
using Serilog;
using SocialAppWebApi.Data;
using SocialAppWebApi.Dto;
using SocialAppWebApi.Jobs;
using SocialAppWebApi.Services;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Custom logging
builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// OpenAPI and Swagger
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Posts API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In =  ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.AddSecurityRequirement((document) => new OpenApiSecurityRequirement()
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

// Controllers
builder.Services.AddControllers();

// Database and DTO mapper
builder.Services.AddDbContext<AppDatabase>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAutoMapper(config => config
    .AddProfile(typeof(MappingProfile)));

// Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var jwtTokenParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = jwtSettings["Issuer"],
    ValidAudience = jwtSettings["Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
};
builder.Services.AddSingleton(jwtTokenParameters);
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = jwtTokenParameters;
    });
builder.Services.AddAuthorization();

// Quartz
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey(nameof(UpdateCachedLikeCountsJob));
    q.AddJob<UpdateCachedLikeCountsJob>(opt => opt.WithIdentity(jobKey));
    q.AddTrigger(opt => opt
        .ForJob(jobKey)
        .WithIdentity($"{nameof(UpdateCachedLikeCountsJob)}.Trigger")
        .WithCronSchedule(builder.Configuration.GetRequiredSection("LikeCacheUpdateSchedule").Value!));
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Domain-specific services
builder.Services.AddTransient<PostsService>();
builder.Services.AddTransient<LikesService>();
builder.Services.AddTransient<UsersService>();
builder.Services.AddTransient<AuthService>();

var app = builder.Build();

app.UseSwagger();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseSwaggerUI(options => options.SwaggerEndpoint("v1/swagger.json", "Posts API V1"));

app.Run();