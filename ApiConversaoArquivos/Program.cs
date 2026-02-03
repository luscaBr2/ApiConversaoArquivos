using ApiConversaoArquivos.Endpoints;
using ApiConversaoArquivos.GraphQL;
using ApiConversaoArquivos.Services.Implementations;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// === CONFIGURAÇÃO DE SERVIÇOS ===

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100 MB

    // Configurar Kestrel para responder em HTTP e HTTPS
    options.ListenAnyIP(5000);  // HTTP
    options.ListenAnyIP(5001, listenOptions =>  // HTTPS
    {
        listenOptions.UseHttps();  // O Let's Encrypt configura o HTTPS automaticamente
    });
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
            Name = "Lucas Santos",
            Email = "lucas.ifsp387@gmail.com",
            Url = new Uri("https://www.linkedin.com/in/lucas-santos387/")
        }
    });
});

if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("http://localhost:5214", "http://127.0.0.1:5214");
}

// === INJEÇÃO DE DEPENDÊNCIA ===
builder.Services.AddScoped<PdfConverterService>();
builder.Services.AddScoped<ExcelConverterService>();
builder.Services.AddScoped<CsvConverterService>();
builder.Services.AddScoped<DocxConverterService>();
builder.Services.AddScoped<XmlConverterService>();
builder.Services.AddScoped<TxtConverterService>();
builder.Services.AddScoped<LogConverterService>();

builder.Services.AddScoped<BatchConverterService>();

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

// Uso do GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);

var app = builder.Build();

// Forçar redirecionamento de HTTP para HTTPS
app.UseHttpsRedirection();  // Redireciona todas as requisições HTTP para HTTPS

// === PIPELINE (ORDEM IMPORTA!) ===

// 1. CORS primeiro
app.UseCors("AllowAll");

// 2. Routing
app.UseRouting();

// 3. Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1.3");
    options.RoutePrefix = string.Empty;
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    options.DisplayRequestDuration();
    options.EnableFilter();
});

// 4. Endpoints
app.UseEndpoints(endpoints =>
{
    // GraphQL DEVE vir ANTES dos outros endpoints
    endpoints.MapGraphQL("/graphql");
});

// 5. Outros endpoints
app.MapFileConverterEndpoints();
app.MapBatchConverterEndpoint();

// Health check
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.3.0",
    features = new[] { "OCR", "Batch", "GraphQL" },
    endpoints = new
    {
        swagger = "/",
        graphql = "/graphql",
        convert = "/api/convert",
        batch = "/api/convert/batch",
        health = "/health"
    }
}))
.WithTags("Sistema");

app.Run();