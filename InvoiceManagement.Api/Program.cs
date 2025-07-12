using InvoiceManagement.Api.Data;
using InvoiceManagement.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ********************************************************************************
// NUEVO: Configuración de CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins"; // Define un nombre para tu política CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      builder =>
                      {
                          // Permite cualquier origen, método y encabezado (para desarrollo)
                          // En producción, aquí especificarías los orígenes exactos de tu frontend (ej. WithOrigins("http://localhost:5173", "https://tudominio.com"))
                          builder.AllowAnyOrigin()
                                 .AllowAnyMethod()
                                 .AllowAnyHeader();
                      });
});
// ********************************************************************************

// Configuración de Controladores y Serialización JSON
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    });

// Configuración de Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "InvoiceManagement.Api", Version = "v1" });
});

// Registro de servicios
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<InvoiceImportService>();

var app = builder.Build();

// Configuración del Pipeline de Peticiones HTTP

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// ********************************************************************************
// NUEVO: Usa la política CORS en el pipeline
app.UseCors(MyAllowSpecificOrigins);
// ********************************************************************************
app.UseAuthorization();
app.MapControllers();

app.Run();