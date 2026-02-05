using ApiConversaoArquivos.Services.Implementations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ApiConversaoArquivos.Endpoints
{
    public static class FileConverterEndpoints
    {
        public static void MapFileConverterEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/convert")
                .WithTags("Conversão de Arquivos");

            group.MapPost("/", async (
                IFormFile file,
                PdfConverterService pdfService,
                ExcelConverterService excelService,
                CsvConverterService csvService,
                DocxConverterService docxService,
                XmlConverterService xmlService,
                TxtConverterService txtService,
                LogConverterService logService)
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

                            case ".docx":
                                jsonResult = await docxService.ConvertToJsonAsync(stream, file.FileName);
                                break;

                            case ".xml":
                                jsonResult = await xmlService.ConvertToJsonAsync(stream, file.FileName);
                                break;

                            case ".txt":
                                jsonResult = await txtService.ConvertToJsonAsync(stream, file.FileName);
                                break;

                            case ".log":
                                jsonResult = await logService.ConvertToJsonAsync(stream, file.FileName);
                                break;

                            case ".pptx":
                                var pptxService = new PptxConverterService();
                                jsonResult = await pptxService.ConvertToJsonAsync(stream, file.FileName);
                                break;

                            default:
                                var formatErrorResponse = new
                                {
                                    success = false,
                                    message = "Formato de arquivo não suportado",
                                    error = $"A extensão '{fileExtension}' não é suportada. " +
                                            $"Tipos aceitos: PDF (.pdf), Excel (.xlsx, .xls, .xlsm), CSV (.csv), Word (.docx), PowerPoint (.pptx), XML (.xml), Text (.txt), Log (.log)",
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
                        var jsonObject = JObject.Parse(jsonResult.ToString());
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
                    if (fileExtension == ".xlsx" || fileExtension == ".xls" || fileExtension == ".xlsm")
                    {
                        var jsonObject = JObject.Parse(jsonResult.ToString());
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

                    // === DOCX ===
                    if (fileExtension == ".docx")
                    {
                        var jsonObject = JObject.Parse(jsonResult.ToString());
                        var paragraphsValue = jsonObject["paragraphs"];
                        var tablesValue = jsonObject["tables"];
                        var fullTextValue = jsonObject["fullText"];
                        var totalParagraphsValue = jsonObject["totalParagraphs"];
                        var totalTablesValue = jsonObject["totalTables"];

                        var paragraphsObject = paragraphsValue != null
                            ? JsonConvert.DeserializeObject(paragraphsValue.ToString(), settings)
                            : null;
                        var tablesObject = tablesValue != null
                            ? JsonConvert.DeserializeObject(tablesValue.ToString(), settings)
                            : null;

                        var docxResponse = new
                        {
                            success = true,
                            message = "Arquivo Word convertido com sucesso para JSON",
                            data = new
                            {
                                fileName = file.FileName,
                                fileType = "Word",
                                totalParagraphs = totalParagraphsValue?.Value<int>() ?? 0,
                                totalTables = totalTablesValue?.Value<int>() ?? 0,
                                paragraphs = paragraphsObject,
                                tables = tablesObject,
                                fullText = fullTextValue?.ToString() ?? ""
                            },
                            error = (string?)null
                        };

                        var docxResponseJson = JsonConvert.SerializeObject(docxResponse, settings);
                        return Results.Content(docxResponseJson, "application/json");
                    }

                    // === XML ===
                    if (fileExtension == ".xml")
                    {
                        var jsonObject = JObject.Parse(jsonResult.ToString());
                        var xmlDataValue = jsonObject["xmlData"];
                        var rootElementValue = jsonObject["rootElement"];
                        var rawXmlValue = jsonObject["rawXml"];

                        var xmlDataObject = xmlDataValue != null
                            ? JsonConvert.DeserializeObject(xmlDataValue.ToString(), settings)
                            : null;

                        var xmlResponse = new
                        {
                            success = true,
                            message = "Arquivo XML convertido com sucesso para JSON",
                            data = new
                            {
                                fileName = file.FileName,
                                fileType = "XML",
                                rootElement = rootElementValue?.ToString() ?? "unknown",
                                xmlData = xmlDataObject,
                                rawXml = rawXmlValue?.ToString() ?? ""
                            },
                            error = (string?)null
                        };

                        var xmlResponseJson = JsonConvert.SerializeObject(xmlResponse, settings);
                        return Results.Content(xmlResponseJson, "application/json");
                    }

                    // === TXT ===
                    if (fileExtension == ".txt")
                    {
                        var jsonObject = JObject.Parse(jsonResult.ToString());
                        var linesValue = jsonObject["lines"];
                        var fullTextValue = jsonObject["fullText"];
                        var totalLinesValue = jsonObject["totalLines"];

                        var linesObject = linesValue != null
                            ? JsonConvert.DeserializeObject(linesValue.ToString(), settings)
                            : null;

                        var txtResponse = new
                        {
                            success = true,
                            message = "Arquivo de texto convertido com sucesso para JSON",
                            data = new
                            {
                                fileName = file.FileName,
                                fileType = "Text",
                                totalLines = totalLinesValue?.Value<int>() ?? 0,
                                lines = linesObject,
                                fullText = fullTextValue?.ToString() ?? ""
                            },
                            error = (string?)null
                        };

                        var txtResponseJson = JsonConvert.SerializeObject(txtResponse, settings);
                        return Results.Content(txtResponseJson, "application/json");
                    }

                    // === LOG ===
                    if (fileExtension == ".log")
                    {
                        var jsonObject = JObject.Parse(jsonResult.ToString());
                        var entriesValue = jsonObject["entries"];
                        var fullTextValue = jsonObject["fullText"];
                        var totalLinesValue = jsonObject["totalLines"];
                        var errorCountValue = jsonObject["errorCount"];
                        var logLevelStatsValue = jsonObject["logLevelStats"];

                        var entriesObject = entriesValue != null
                            ? JsonConvert.DeserializeObject(entriesValue.ToString(), settings)
                            : null;
                        var logLevelStatsObject = logLevelStatsValue != null
                            ? JsonConvert.DeserializeObject(logLevelStatsValue.ToString(), settings)
                            : null;

                        var logResponse = new
                        {
                            success = true,
                            message = "Arquivo de log convertido com sucesso para JSON",
                            data = new
                            {
                                fileName = file.FileName,
                                fileType = "Log",
                                totalLines = totalLinesValue?.Value<int>() ?? 0,
                                errorCount = errorCountValue?.Value<int>() ?? 0,
                                logLevelStats = logLevelStatsObject,
                                entries = entriesObject,
                                fullText = fullTextValue?.ToString() ?? ""
                            },
                            error = (string?)null
                        };

                        var logResponseJson = JsonConvert.SerializeObject(logResponse, settings);
                        return Results.Content(logResponseJson, "application/json");
                    }

                    // === POWERPOINT ===
                    if (fileExtension == ".pptx")
                    {
                        var jsonObject = JObject.Parse(jsonResult.ToString());
                        var slidesValue = jsonObject["slides"];
                        var fullTextValue = jsonObject["fullText"];
                        var totalSlidesValue = jsonObject["totalSlides"];

                        var slidesObject = slidesValue != null
                            ? JsonConvert.DeserializeObject(slidesValue.ToString(), settings)
                            : null;

                        var pptxResponse = new
                        {
                            success = true,
                            message = "Arquivo PowerPoint convertido com sucesso para JSON",
                            data = new
                            {
                                fileName = file.FileName,
                                fileType = "PowerPoint",
                                totalSlides = totalSlidesValue?.Value<int>() ?? 0,
                                slides = slidesObject,
                                fullText = fullTextValue?.ToString() ?? ""
                            },
                            error = (string?)null
                        };

                        var pptxResponseJson = JsonConvert.SerializeObject(pptxResponse, settings);
                        return Results.Content(pptxResponseJson, "application/json");
                    }

                    // === FALLBACK ===
                    return Results.Problem(
                        detail: "Tipo de arquivo não processado",
                        statusCode: 500,
                        title: "Erro interno"
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
                "Endpoint unificado que aceita PDF, Excel, PowerPoint, CSV, Word, XML, TXT e LOG e converte para JSON. " +
                "O tipo é identificado automaticamente pela extensão."
            )
            .Accepts<IFormFile>("multipart/form-data")
            .Produces(200, contentType: "application/json")
            .Produces(400)
            .Produces(500)
            .DisableAntiforgery();
        }
    }
}