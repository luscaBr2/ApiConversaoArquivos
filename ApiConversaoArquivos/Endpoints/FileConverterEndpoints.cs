using ApiConversaoArquivos.Services.Implementations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ApiConversaoArquivos.Endpoints
{
    /// <summary>
    /// Classe estática que define os endpoints da API
    /// 
    /// Como usar: Esta classe define um único endpoint unificado que aceita qualquer tipo de arquivo.
    /// O sistema identifica automaticamente o tipo e faz a conversão apropriada.
    /// </summary>
    public static class FileConverterEndpoints
    {
        /// <summary>
        /// Método de extensão que mapeia todos os endpoints de conversão
        /// </summary>
        public static void MapFileConverterEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/convert")
                .WithTags("Conversão de Arquivos");

            group.MapPost("/", async (
                IFormFile file,
                [FromForm] string? connectionString,
                PdfConverterService pdfService,
                ExcelConverterService excelService,
                CsvConverterService csvService,
                SqlConverterService sqlService)
                =>
            {
                try
                {
                    // === VALIDAÇÃO INICIAL ===
                    if (file == null || file.Length == 0)
                    {
                        var errorResponse = new
                        {
                            success = false,
                            message = "Nenhum arquivo foi enviado",
                            error = "File is required",
                            data = (object?)null
                        };
                        return Results.BadRequest(errorResponse);
                    }

                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    JToken jsonResult;

                    using (var stream = file.OpenReadStream())
                    {
                        // === IDENTIFICAÇÃO AUTOMÁTICA DO TIPO DE ARQUIVO ===
                        switch (fileExtension)
                        {
                            case ".pdf":
                                jsonResult = await pdfService.ConvertToJsonAsync(stream, file.FileName);
                                break;

                            case ".xlsx":
                            case ".xls":
                            case ".xlsm":
                                jsonResult = await excelService.ConvertToJsonAsync(stream, file.FileName);
                                break;

                            case ".csv":
                                jsonResult = await csvService.ConvertToJsonAsync(stream, file.FileName);
                                break;

                            case ".sql":
                                if (string.IsNullOrWhiteSpace(connectionString))
                                {
                                    var sqlErrorResponse = new
                                    {
                                        success = false,
                                        message = "Para arquivos .sql, é necessário fornecer a connectionString",
                                        error = "ConnectionString is required for .sql files",
                                        data = (object?)null
                                    };
                                    return Results.BadRequest(sqlErrorResponse);
                                }

                                jsonResult = await sqlService.ExecuteQueryFromFileAsync(
                                    stream,
                                    file.FileName,
                                    connectionString
                                );
                                break;

                            default:
                                var formatErrorResponse = new
                                {
                                    success = false,
                                    message = "Formato de arquivo não suportado",
                                    error = $"A extensão '{fileExtension}' não é suportada. " +
                                           $"Tipos aceitos: PDF (.pdf), Excel (.xlsx, .xls), CSV (.csv), SQL (.sql)",
                                    data = (object?)null
                                };
                                return Results.BadRequest(formatErrorResponse);
                        }
                    }

                    // === PROCESSAMENTO ESPECÍFICO POR TIPO DE ARQUIVO ===

                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        Formatting = Formatting.Indented
                    };

                    // === CSV ===
                    if (fileExtension == ".csv")
                    {
                        var jsonObject = JObject.Parse(jsonResult.ToString());
                        var dataValue = jsonObject["data"];
                        var dataObject = dataValue != null
                            ? JsonConvert.DeserializeObject(dataValue.ToString(), settings)
                            : null;

                        var responseObject = new
                        {
                            success = true,
                            message = "Arquivo CSV convertido com sucesso para JSON",
                            data = dataObject,
                            error = (string?)null
                        };

                        var responseJson = JsonConvert.SerializeObject(responseObject, settings);
                        return Results.Content(responseJson, "application/json");
                    }

                    // === PDF ===
                    if (fileExtension == ".pdf")
                    {
                        // Log para debug
                        Console.WriteLine($"[PDF] jsonResult: {jsonResult?.ToString()}");

                        var jsonObject = JObject.Parse(jsonResult.ToString());

                        // Para PDF, retornamos o objeto completo com páginas
                        var pagesValue = jsonObject["pages"];
                        var fullTextValue = jsonObject["fullText"];
                        var totalPagesValue = jsonObject["totalPages"];

                        var pagesObject = pagesValue != null
                            ? JsonConvert.DeserializeObject(pagesValue.ToString(), settings)
                            : null;

                        var pdfResponse = new
                        {
                            success = true,
                            message = "Arquivo PDF convertido com sucesso para JSON",
                            data = new
                            {
                                fileName = file.FileName,
                                fileType = "PDF",
                                totalPages = totalPagesValue?.Value<int>() ?? 0,
                                pages = pagesObject,
                                fullText = fullTextValue?.ToString() ?? ""
                            },
                            error = (string?)null
                        };

                        var pdfResponseJson = JsonConvert.SerializeObject(pdfResponse, settings);
                        return Results.Content(pdfResponseJson, "application/json");
                    }

                    // === EXCEL ===
                    if (fileExtension == ".xlsx" || fileExtension == ".xls")
                    {
                        var jsonObject = JObject.Parse(jsonResult.ToString());

                        // Para Excel, retornamos o objeto completo com sheets
                        var sheetsValue = jsonObject["sheets"];
                        var totalSheetsValue = jsonObject["totalSheets"];

                        var sheetsObject = sheetsValue != null
                            ? JsonConvert.DeserializeObject(sheetsValue.ToString(), settings)
                            : null;

                        var excelResponse = new
                        {
                            success = true,
                            message = "Arquivo Excel convertido com sucesso para JSON",
                            data = new
                            {
                                fileName = file.FileName,
                                fileType = "Excel",
                                totalSheets = totalSheetsValue?.Value<int>() ?? 0,
                                sheets = sheetsObject
                            },
                            error = (string?)null
                        };

                        var excelResponseJson = JsonConvert.SerializeObject(excelResponse, settings);
                        return Results.Content(excelResponseJson, "application/json");
                    }

                    // === SQL ===
                    if (fileExtension == ".sql")
                    {
                        var jsonObject = JObject.Parse(jsonResult.ToString());
                        var dataValue = jsonObject["data"];
                        var totalRecordsValue = jsonObject["totalRecords"];

                        var dataObject = dataValue != null
                            ? JsonConvert.DeserializeObject(dataValue.ToString(), settings)
                            : null;

                        var sqlResponse = new
                        {
                            success = true,
                            message = "Arquivo SQL executado com sucesso",
                            data = new
                            {
                                fileName = file.FileName,
                                queryType = "SQL",
                                totalRecords = totalRecordsValue?.Value<int>() ?? 0,
                                records = dataObject
                            },
                            error = (string?)null
                        };

                        var sqlResponseJson = JsonConvert.SerializeObject(sqlResponse, settings);
                        return Results.Content(sqlResponseJson, "application/json");
                    }

                    // === FALLBACK (não deveria chegar aqui) ===
                    return Results.Problem(
                        detail: "Tipo de arquivo não processado",
                        statusCode: 500,
                        title: "Erro interno"
                    );
                }
                catch (SqlException ex)
                {
                    return Results.Problem(
                        detail: $"Erro SQL: {ex.Message}",
                        statusCode: 500,
                        title: "Erro ao executar query SQL"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERRO] {ex.Message}");
                    Console.WriteLine($"[STACK] {ex.StackTrace}");

                    return Results.Problem(
                        detail: ex.Message,
                        statusCode: 500,
                        title: "Erro ao processar arquivo"
                    );
                }
            })
            .WithName("ConverterArquivo")
            .WithSummary("Converte qualquer arquivo suportado para JSON")
            .WithDescription(
                "Endpoint unificado que aceita PDF, Excel, CSV ou SQL e converte para JSON. " +
                "O tipo é identificado automaticamente pela extensão. " +
                "Para arquivos .sql, forneça também o campo 'connectionString'."
            )
            .Accepts<IFormFile>("multipart/form-data")
            .Produces(200, contentType: "application/json")
            .Produces(400)
            .Produces(500)
            .DisableAntiforgery();
        }
    }
}