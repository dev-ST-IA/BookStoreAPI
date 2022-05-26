using BulkyBookBackEnd;
using BulkyBookBackEnd.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options=>options.JsonSerializerOptions.ReferenceHandler=ReferenceHandler.Preserve)
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(p => p.AddPolicy("corsapp", corsBuilder =>
{
    corsBuilder.WithOrigins(builder.Configuration["CorsAllowedHosts:FrontEndDevelopmentHost"], builder.Configuration["CorsAllowedHosts:FrontEndProductionHost"], builder.Configuration["CorsAllowedHosts:FrontEndDevelopmentAdminHost"]).AllowAnyMethod().AllowAnyHeader();
}));

builder.Services.AddSwaggerGen(options=>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Scheme ="Bearer",
            BearerFormat ="JWT",
            In=ParameterLocation.Header,
            Name="Authorization",
            Description = "Bearer Authentication with JWT Token",
            Type = SecuritySchemeType.Http
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id ="Bearer",
                        Type = ReferenceType.SecurityScheme
                    }

                },
                new List<string>()
            }
        });
    }
);

// Inject DbContext
builder.Services.AddDbContext<BookDbContext>(options => 
options
.UseSqlServer(builder.Configuration.GetConnectionString("Development")));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateActor = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience  = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    }
); ;
Jwt.Issuer = builder.Configuration["Jwt:Issuer"];
Jwt.Audience = builder.Configuration["Jwt:Audience"];
Jwt.Key = builder.Configuration["Jwt:Key"];
CloudinaryClass.CloudName = builder.Configuration["Cloudinary:CloudName"];
CloudinaryClass.ApiKey = builder.Configuration["Cloudinary:ApiKey"];
CloudinaryClass.ApiSecret = builder.Configuration["Cloudinary:ApiSecret"];

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("corsapp");
app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.Run();
