using DataAccess.Repository;
using DataAccess.Entities;
using DataAccess.IRepository;
using Microsoft.EntityFrameworkCore;
using BussinessLogic.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Org.BouncyCastle.Crypto.Agreement.Srp;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AutoWrapper;
using BussinessLogic.DTO.Email;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Configurar la licencia de QuestPDF
QuestPDF.Settings.License = LicenseType.Community;
//agrego la inyeccion de dependencia de mercado pago, para poder usar el servicio que cree
builder.Services.Configure<MercadoPagoDevSettings>(builder.Configuration.GetSection("MercadoPagoDev"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<GoogleCloudStorage>(builder.Configuration.GetSection("GoogleCloudStorage"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mi API", Version = "v1" });

    // Configurar Swagger para usar Authorization
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese 'Bearer' [espacio] y luego su token en el campo de texto. Ejemplo: 'Bearer 12345abcdef'",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
});// builder.

// ADD Entity framework con mysql

builder.Services.AddDbContext<DbveterinariaContext>(options => options.UseMySQL(builder.Configuration.GetConnectionString("dbConnection")));

//agrego la inyeccion de dependencia de los repositorios y el UnitOfWork

//El AddScoped es para que se cree un nuevo contexto cada vez que se haga un request
//El addTransient es para que se cree un nuevo contexto cada vez que se llame a la clase
//La diferencia entre AddScoped y AddTransient es que el AddScoped crea un contexto por cada request y el AddTransient crea un contexto por cada vez que se lo llama
//El AddSingleton es para que se cree un contexto por una unica vez y se reutilice en todos los request


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();



builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy("politica", app =>
    {
        app.AllowAnyOrigin();
        app.AllowAnyHeader();
        app.AllowAnyMethod();
    });
});


//este tambien funciono

// var domain = $"https://{builder.Configuration["Auth0:Domain"]}/";
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
// .AddJwtBearer(options =>
// {
//     options.Authority = domain;
//     options.Audience = builder.Configuration["Auth0:Audience"];
//     options.TokenValidationParameters = new TokenValidationParameters
//     {
//         NameClaimType = ClaimTypes.NameIdentifier
//     };
// });



var domain = $"https://{builder.Configuration["Auth0:Domain"]}/";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.Authority = domain;
    options.Audience = builder.Configuration["Auth0:Audience"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = ClaimTypes.NameIdentifier,
        // Configuración para validar la firma del token
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Auth0:SecretKey"])),

        // Estas son configuraciones adicionales que puedes necesitar
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = domain,
        ValidAudience = builder.Configuration["Auth0:Audience"]
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c =>
                c.Type == "user_rol" && c.Value == "Administrador")));

    options.AddPolicy("Sucursal", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c =>
                c.Type == "user_rol" && c.Value == "Sucursal")));

    options.AddPolicy("Cliente", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c =>
                c.Type == "user_rol" && c.Value == "Cliente")));
});



var app = builder.Build();



using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DbveterinariaContext>();
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}

app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions
{
    // Aquí puedes personalizar las opciones como prefieras
    IsDebug = app.Environment.IsDevelopment()
});




//habilito los cors

app.UseCors("politica");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();


app.Run();

