using ApiConversaoArquivos.Services.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace ApiConversaoArquivos.Endpoints
{
    public static class FileConverterEndpoints
    {
        public static void MapFileConverterEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/convert")
                .WithTags("Conversão de Arquivos");

            // ENDPOINT ÚNICO - SEMPRE USA BATCH
            group.MapPost("/", async (
                [FromForm] List<IFormFile> files,
                [FromForm] bool combineResults,
                BatchConverterService batchService)
                =>
            {
                try
                {
                    // Validação
                    if (files == null || files.Count == 0)
                    {
                        return Results.BadRequest(new
                        {
                            success = false,
                            message = "Nenhum arquivo foi enviado",
                            error = "Files are required",
                            data = (object?)null
                        });
                    }

                    if (files.Count > 20)
                    {
                        return Results.BadRequest(new
                        {
                            success = false,
                            message = "Máximo de 20 arquivos por lote",
                            error = "Too many files. Maximum is 20 files per request.",
                            data = (object?)null
                        });
                    }

                    // Processar com batch (funciona com 1 ou mais arquivos)
                    var result = await batchService.ConvertBatchAsync(files, combineResults);

                    var response = new
                    {
                        success = result.Success,
                        message = result.Message,
                        data = new
                        {
                            totalFiles = result.TotalFiles,
                            successCount = result.SuccessCount,
                            failureCount = result.FailureCount,
                            results = result.Results,
                            combinedData = combineResults ? result.CombinedData : null
                        },
                        error = (string?)null
                    };

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERRO] {ex.Message}");
                    Console.WriteLine($"[STACK] {ex.StackTrace}");

                    return Results.Problem(
                        detail: ex.Message,
                        statusCode: 500,
                        title: "Erro ao processar arquivo(s)"
                    );
                }
            })
            .WithName("ConverterArquivos")
            .WithSummary("Converte um ou múltiplos arquivos para JSON")
            .WithDescription(
                "Endpoint unificado que aceita de 1 a 20 arquivos e os processa em paralelo. " +
                "Suporta: PDF, Excel (.xlsx, .xls, .xlsm), CSV, Word (.docx), XML, Text (.txt) e Log (.log). " +
                "Use combineResults=true para obter um JSON consolidado de todos os arquivos."
            )
            .Accepts<List<IFormFile>>("multipart/form-data")
            .Produces(200, contentType: "application/json")
            .Produces(400)
            .Produces(500)
            .DisableAntiforgery();
        }
    }
}