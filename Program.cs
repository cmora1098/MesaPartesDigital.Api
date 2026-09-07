using MesaPartesDigital.Api.Models;
using MesaPartesDigital.Api.Services;
using MesaPartesDigital.Data;
using MesaPartesDigital.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Text;


var builder = WebApplication.CreateBuilder(args);


// ============================================================
// 1. AUTENTICACIÓN JWT
// ============================================================

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "La configuración 'Jwt:Key' no está definida.");

var key = Encoding.UTF8.GetBytes(jwtKey);


builder.Services
    .AddAuthentication(options =>
    {
        // El API utiliza JWT como esquema de autenticación.
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                // ====================================================
                // FIRMA
                // ====================================================

                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(key),


                // ====================================================
                // ISSUER
                // ====================================================
                //
                // Actualmente no utilizamos Issuer.
                //

                ValidateIssuer = false,


                // ====================================================
                // AUDIENCE
                // ====================================================
                //
                // Actualmente no utilizamos Audience.
                //

                ValidateAudience = false,


                // ====================================================
                // EXPIRACIÓN
                // ====================================================

                ValidateLifetime = true,

                // Sin tolerancia adicional.
                //
                // Si el JWT vence a las 10:15:00,
                // a las 10:15:01 ya no es válido.
                //

                ClockSkew = TimeSpan.Zero
            };
    });


builder.Services.AddAuthorization();


// ============================================================
// 2. CONTROLADORES Y SWAGGER
// ============================================================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Mesa de Partes Digital API",
            Version = "v1"
        });
});


// ============================================================
// 3. BASE DE DATOS
// ============================================================

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString(
                "DefaultConnection")));


// ============================================================
// 4. SERVICIOS
// ============================================================

builder.Services.AddMemoryCache();

builder.Services.AddScoped<UbigeoService>();
builder.Services.AddScoped<ContribuyenteService>();
builder.Services.AddScoped<TipoPersonaService>();
builder.Services.AddScoped<TipoDocumentoService>();
builder.Services.AddScoped<TipoDocPerService>();
builder.Services.AddScoped<EstadoService>();
builder.Services.AddScoped<DocumentoService>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILoginService, LoginService>();

builder.Services.AddScoped<FileStorageService>();
builder.Services.AddScoped<TramiteService>();

builder.Services.AddScoped<IPersonaService, PersonaService>();
    
builder.Services.Configure<StorageSettings>(
    builder.Configuration.GetSection("Storage"));


// ============================================================
// 5. CORS
// ============================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("Front", policy =>
    {
        var origins =
            builder.Configuration
                .GetSection("Cors:Origins")
                .Get<string[]>() ?? [];

        policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// ============================================================
// BUILD
// ============================================================

var app = builder.Build();


// ============================================================
// 6. SWAGGER
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}


// ============================================================
// 7. HTTPS
// ============================================================

app.UseHttpsRedirection();


// ============================================================
// 8. CORS
// ============================================================

app.UseCors("Front");


// ============================================================
// 9. AUTENTICACIÓN Y AUTORIZACIÓN
// ============================================================

// IMPORTANTE:
// Authentication debe ejecutarse antes que Authorization.

app.UseAuthentication();

app.UseAuthorization();


// ============================================================
// 10. CONTROLADORES
// ============================================================

app.MapControllers();


// ============================================================
// 11. EJECUCIÓN
// ============================================================

app.Run();