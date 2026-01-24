using ApiConversaoArquivos.Endpoints;
using ApiConversaoArquivos.Services.Implementations;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// === CONFIGURAÇÃO DE SERVIÇOS ===
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100 MB
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Conversão de Arquivos",
        Version = "v1.2",
        Description = "API para conversão de arquivos PDF, Excel, CSV, Word, XML, TXT e LOG em formato JSON",
        Contact = new OpenApiContact
        {
            Name = "Suporte",
            Email = "suporte@example.com"
        }
    });
});

// === INJEÇÃO DE DEPENDÊNCIA ===
builder.Services.AddScoped<PdfConverterService>();
builder.Services.AddScoped<ExcelConverterService>();
builder.Services.AddScoped<CsvConverterService>();
builder.Services.AddScoped<DocxConverterService>();
builder.Services.AddScoped<XmlConverterService>();
builder.Services.AddScoped<TxtConverterService>();
builder.Services.AddScoped<LogConverterService>();

// === CONFIGURAÇÃO DE CORS ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// === CONFIGURAÇÃO DO PIPELINE ===
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Conversão v1.2");
    options.RoutePrefix = string.Empty;
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    options.DisplayRequestDuration();
    options.EnableFilter();
    options.DocumentTitle = "API de Conversão - Documentação";
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// === MAPEAMENTO DOS ENDPOINTS ===
app.MapFileConverterEndpoints();

// Health check
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.2.0",
    environment = app.Environment.EnvironmentName
}))
.WithName("HealthCheck")
.WithDescription("Verifica o status de saúde da API")
.WithTags("Sistema")
.Produces(200);

app.Run();