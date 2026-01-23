using ApiConversaoArquivos.Endpoints;
using ApiConversaoArquivos.Services.Implementations;
using Microsoft.OpenApi;

// Cria o builder da aplicação web
var builder = WebApplication.CreateBuilder(args);

// === CONFIGURAÇÃO DE SERVIÇOS ===

// AddEndpointsApiExplorer permite que a API exponha metadados dos endpoints
// Isso é essencial para que o Swagger/OpenAPI consiga descobrir os endpoints
builder.Services.AddEndpointsApiExplorer();

// AddSwaggerGen configura a geração da especificação OpenAPI
// Configuração básica sem filtros customizados para evitar conflitos de versão
builder.Services.AddSwaggerGen(options =>
{
    // SwaggerDoc define uma especificação OpenAPI com nome "v1"
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Conversão de Arquivos",
        Version = "v1",
        Description = "API para conversão de arquivos PDF, Excel, CSV e execução de queries SQL em formato JSON",
        Contact = new OpenApiContact
        {
            Name = "Suporte",
            Email = "suporte@example.com"
        }
    });
});

// === INJEÇÃO DE DEPENDÊNCIA DOS SERVIÇOS ===
// AddScoped cria uma nova instância do serviço para cada requisição HTTP
builder.Services.AddScoped<PdfConverterService>();
builder.Services.AddScoped<ExcelConverterService>();
builder.Services.AddScoped<CsvConverterService>();
builder.Services.AddScoped<SqlConverterService>();

// === CONFIGURAÇÃO DE CORS ===
// CORS permite que a API seja chamada de outras origens
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()      // Permite qualquer origem
              .AllowAnyMethod()      // Permite qualquer método HTTP
              .AllowAnyHeader();     // Permite qualquer cabeçalho
    });
});

// Constrói a aplicação
var app = builder.Build();

// === CONFIGURAÇÃO DO PIPELINE DE REQUISIÇÃO ===

// Habilita Swagger apenas em ambiente Development
if (app.Environment.IsDevelopment())
    app.UseSwagger(); // UseSwagger expõe a especificação OpenAPI em JSON

// UseSwaggerUI cria a interface visual interativa
app.UseSwaggerUI(options =>
{
    // Define onde está o documento JSON da API
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Conversão v1");

    // Define que o Swagger UI será servido na raiz do site
    options.RoutePrefix = string.Empty;

    // Configurações opcionais da UI
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    options.DisplayRequestDuration();
    options.EnableFilter();
    options.DocumentTitle = "API de Conversão - Documentação";
});
// }

// Redireciona HTTP para HTTPS
app.UseHttpsRedirection();

// Habilita CORS
app.UseCors("AllowAll");

// === MAPEAMENTO DOS ENDPOINTS ===
// Chama o método de extensão que registra todos os endpoints
app.MapFileConverterEndpoints();

// Endpoint de health check
// Útil para monitoramento e verificar se a API está funcionando
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0",
    environment = app.Environment.EnvironmentName
}))
.WithName("HealthCheck")
.WithDescription("Verifica o status de saúde da API")
.WithTags("Sistema")
.Produces(200);

// Inicia a aplicação
app.Run();