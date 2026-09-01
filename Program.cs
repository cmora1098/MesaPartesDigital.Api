using MesaPartesDigital.Api.Models;
using MesaPartesDigital.Api.Services;
using MesaPartesDigital.Data;
using MesaPartesDigital.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración unificada de Cookies y JWT[cite: 6]
var jwtKey = builder.Configuration["Jwt:Key"] ?? "5f8a9e2d4b1c7f6a9e3d8b2c4f1a7d9e";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = ".MesaPartesDigital.Auth";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero // Elimina el tiempo de gracia extra al expirar
    };
});

// 2. Controladores y Swagger[cite: 6]
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Mesa de Partes Digital API", Version = "v1" });
});

// 3. Base de Datos[cite: 6]
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. Inyección de Dependencias (Servicios)[cite: 6]
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
builder.Services.Configure<StorageSettings>(
    builder.Configuration.GetSection("Storage"));

// 5. CORS[cite: 6]
builder.Services.AddCors(options =>
{
    options.AddPolicy("Front", policy =>
        policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

// 6. Pipeline de la Aplicación[cite: 6]
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Front");

// 7. Autenticación y Autorización[cite: 6]
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();