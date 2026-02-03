using ApiConversaoArquivos.Endpoints;
using ApiConversaoArquivos.GraphQL;
using ApiConversaoArquivos.Services.Implementations;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// === KESTREL ===
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100 MB
});

if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("http://localhost:5214");
}
else
{
    builder.WebHost.UseUrls("http://0.0.0.0:5000");
}

// === SWAGGER ===
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Conversão de Arquivos",
        Version = "v2.0",
        Description = "API unificada para conversão de arquivos com processamento em lote, OCR e GraphQL",
        Contact = new OpenApiContact
        {
            Name = "Suporte",
            Email = "suporte@example.com"
        }
    });
});

// === SERVIÇOS ===
builder.Services.AddScoped<PdfConverterService>();
builder.Services.AddScoped<ExcelConverterService>();
builder.Services.AddScoped<CsvConverterService>();
builder.Services.AddScoped<DocxConverterService>();
builder.Services.AddScoped<XmlConverterService>();
builder.Services.AddScoped<TxtConverterService>();
builder.Services.AddScoped<LogConverterService>();
builder.Services.AddScoped<OcrService>();
builder.Services.AddScoped<BatchConverterService>();

// === CORS ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// === GRAPHQL ===
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();

var app = builder.Build();

// === PIPELINE ===
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v2.0");
    c.RoutePrefix = string.Empty;
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    c.DisplayRequestDuration();
    c.EnableFilter();
});

// GraphQL
app.MapGraphQL("/graphql");

// REST Endpoint (unificado com batch)
app.MapFileConverterEndpoints();

// Health check
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "2.0.0",
    features = new[] { "Batch Processing", "OCR", "GraphQL" },
    endpoints = new
    {
        swagger = "/",
        graphql = "/graphql",
        convert = "/api/convert",
        health = "/health"
    }
}))
.WithName("HealthCheck")
.WithDescription("Verifica o status de saúde da API")
.WithTags("Sistema");

app.Run();