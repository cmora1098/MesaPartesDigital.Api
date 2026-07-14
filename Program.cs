using MesaPartesDigital.Data;
using MesaPartesDigital.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mesa de Partes Digital API",
        Version = "v1",
        Description = "API para la gestión de la Mesa de Partes Digital."
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("Front", policy =>
        policy
            .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Mesa de Partes Digital API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Mesa de Partes Digital API";
    });
}

app.UseHttpsRedirection();
app.UseCors("Front");
app.MapControllers();

app.Run();
