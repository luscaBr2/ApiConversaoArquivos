using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ApiConversaoArquivos.Services.Interfaces;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ApiConversaoArquivos.Services.Implementations
{
    public class PdfConverterService : IFileConverterService
    {
        private readonly OcrService _ocrService;

        public PdfConverterService()
        {
            _ocrService = new OcrService();
        }

        public async Task<JToken> ConvertToJsonAsync(Stream fileStream, string fileName)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    var pages = new List<Dictionary<string, object>>();
                    var fullText = new System.Text.StringBuilder();

                    // Criar cópia do stream para verificação
                    using var memoryStream = new MemoryStream();
                    await fileStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    // Verificar se é PDF escaneado
                    var isScanned = await _ocrService.IsPdfScannedAsync(memoryStream);
                    memoryStream.Position = 0;

                    if (isScanned)
                    {
                        Console.WriteLine("[PDF] Detectado PDF escaneado - usando OCR");

                        // Processar com OCR
                        var ocrTexts = await _ocrService.ExtractTextFromPdfAsync(memoryStream);

                        for (int i = 0; i < ocrTexts.Count; i++)
                        {
                            var pageText = ocrTexts[i];

                            pages.Add(new Dictionary<string, object>
                            {
                                { "pageNumber", i + 1 },
                                { "content", pageText },
                                { "hasContent", !string.IsNullOrWhiteSpace(pageText) },
                                { "extractedWithOCR", true }
                            });

                            fullText.AppendLine(pageText);
                        }
                    }
                    else
                    {
                        Console.WriteLine("[PDF] Detectado PDF normal - extração padrão");

                        // Processar normalmente
                        var reader = new PdfReader(memoryStream);

                        for (int pageNumber = 1; pageNumber <= reader.NumberOfPages; pageNumber++)
                        {
                            var strategy = new SimpleTextExtractionStrategy();
                            var pageText = PdfTextExtractor.GetTextFromPage(reader, pageNumber, strategy);

                            pages.Add(new Dictionary<string, object>
                            {
                                { "pageNumber", pageNumber },
                                { "content", pageText },
                                { "hasContent", !string.IsNullOrWhiteSpace(pageText) },
                                { "extractedWithOCR", false }
                            });

                            fullText.AppendLine(pageText);
                        }

                        reader.Close();
                    }

                    var resultObject = new
                    {
                        fileName = fileName,
                        fileType = "PDF",
                        totalPages = pages.Count,
                        isScanned = isScanned,
                        pages = pages,
                        fullText = fullText.ToString()
                    };

                    var json = JsonConvert.SerializeObject(resultObject);
                    return JToken.Parse(json);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao processar PDF: {ex.Message}", ex);
                }
            });
        }
    }
}