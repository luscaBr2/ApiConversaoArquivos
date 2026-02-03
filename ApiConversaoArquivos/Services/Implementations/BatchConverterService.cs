using ApiConversaoArquivos.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using Path = System.IO.Path;

namespace ApiConversaoArquivos.Services.Implementations
{
    public class BatchConverterService
    {
        private readonly PdfConverterService _pdfService;
        private readonly ExcelConverterService _excelService;
        private readonly CsvConverterService _csvService;
        private readonly DocxConverterService _docxService;
        private readonly XmlConverterService _xmlService;
        private readonly TxtConverterService _txtService;
        private readonly LogConverterService _logService;

        public BatchConverterService(
            PdfConverterService pdfService,
            ExcelConverterService excelService,
            CsvConverterService csvService,
            DocxConverterService docxService,
            XmlConverterService xmlService,
            TxtConverterService txtService,
            LogConverterService logService)
        {
            _pdfService = pdfService;
            _excelService = excelService;
            _csvService = csvService;
            _docxService = docxService;
            _xmlService = xmlService;
            _txtService = txtService;
            _logService = logService;
        }

        public async Task<BatchConversionResponse> ConvertBatchAsync(List<IFormFile> files, bool combineResults = false)
        {
            var response = new BatchConversionResponse
            {
                TotalFiles = files.Count,
                Results = new List<BatchFileResult>()
            };

            var results = new ConcurrentBag<BatchFileResult>();

            // Processar arquivos em paralelo
            await Parallel.ForEachAsync(files, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (file, ct) =>
            {
                var result = await ProcessSingleFileAsync(file);
                results.Add(result);
            });

            response.Results = results.OrderBy(r => r.FileName).ToList();
            response.SuccessCount = results.Count(r => r.Success);
            response.FailureCount = results.Count(r => !r.Success);
            response.Success = response.FailureCount == 0;
            response.Message = response.Success
                ? $"Todos os {response.TotalFiles} arquivos convertidos com sucesso"
                : $"{response.SuccessCount}/{response.TotalFiles} arquivos convertidos com sucesso";

            // Combinar resultados se solicitado
            if (combineResults && response.SuccessCount > 0)
            {
                response.CombinedData = CombineResults(response.Results.Where(r => r.Success).ToList());
            }

            return response;
        }

        private async Task<BatchFileResult> ProcessSingleFileAsync(IFormFile file)
        {
            try
            {
                var extension = Path.GetExtension(file.FileName).ToLower();
                JToken result;

                using var stream = file.OpenReadStream();

                switch (extension)
                {
                    case ".pdf":
                        result = await _pdfService.ConvertToJsonAsync(stream, file.FileName);
                        break;
                    case ".xlsx":
                    case ".xls":
                    case ".xlsm":
                        result = await _excelService.ConvertToJsonAsync(stream, file.FileName);
                        break;
                    case ".csv":
                        result = await _csvService.ConvertToJsonAsync(stream, file.FileName);
                        break;
                    case ".docx":
                        result = await _docxService.ConvertToJsonAsync(stream, file.FileName);
                        break;
                    case ".xml":
                        result = await _xmlService.ConvertToJsonAsync(stream, file.FileName);
                        break;
                    case ".txt":
                        result = await _txtService.ConvertToJsonAsync(stream, file.FileName);
                        break;
                    case ".log":
                        result = await _logService.ConvertToJsonAsync(stream, file.FileName);
                        break;
                    default:
                        return new BatchFileResult
                        {
                            FileName = file.FileName,
                            Success = false,
                            Error = $"Formato não suportado: {extension}"
                        };
                }

                return new BatchFileResult
                {
                    FileName = file.FileName,
                    Success = true,
                    Message = "Convertido com sucesso",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new BatchFileResult
                {
                    FileName = file.FileName,
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private object CombineResults(List<BatchFileResult> successResults)
        {
            var combined = new
            {
                totalFiles = successResults.Count,
                files = successResults.Select(r => new
                {
                    fileName = r.FileName,
                    data = r.Data
                }).ToList()
            };

            return combined;
        }
    }
}