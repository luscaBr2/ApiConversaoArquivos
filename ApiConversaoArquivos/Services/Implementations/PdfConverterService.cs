using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ApiConversaoArquivos.Services.Interfaces;
using System.Text;

namespace ApiConversaoArquivos.Services.Implementations
{
    /// <summary>
    /// Serviço responsável pela conversão de arquivos PDF para JSON
    /// Utiliza iTextSharp para extração de texto
    /// </summary>
    public class PdfConverterService : IFileConverterService
    {
        /// <summary>
        /// Converte PDF em JSON extraindo o texto de cada página
        /// </summary>
        public async Task<JToken> ConvertToJsonAsync(Stream fileStream, string fileName)
        {
            return await Task.Run(() =>
            {
                var pages = new List<Dictionary<string, object>>();
                string fullText = "";

                try
                {
                    // Cria um MemoryStream a partir do fileStream
                    byte[] fileBytes;
                    using (var ms = new MemoryStream())
                    {
                        fileStream.CopyTo(ms);
                        fileBytes = ms.ToArray();
                    }

                    // Valida se o arquivo tem conteúdo
                    if (fileBytes == null || fileBytes.Length == 0)
                    {
                        throw new Exception("O arquivo PDF está vazio ou corrompido");
                    }

                    // Cria o PdfReader a partir dos bytes
                    using (var pdfReader = new PdfReader(fileBytes))
                    {
                        int numberOfPages = pdfReader.NumberOfPages;

                        // Itera sobre todas as páginas
                        for (int i = 1; i <= numberOfPages; i++)
                        {
                            try
                            {
                                // Tenta extrair o texto usando LocationTextExtractionStrategy (mais precisa)
                                var strategy = new LocationTextExtractionStrategy();
                                string pageText = PdfTextExtractor.GetTextFromPage(pdfReader, i, strategy);

                                // Se não conseguiu com LocationTextExtractionStrategy, tenta com SimpleTextExtractionStrategy
                                if (string.IsNullOrWhiteSpace(pageText))
                                {
                                    var simpleStrategy = new SimpleTextExtractionStrategy();
                                    pageText = PdfTextExtractor.GetTextFromPage(pdfReader, i, simpleStrategy);
                                }

                                // Remove espaços extras e linhas vazias
                                pageText = CleanText(pageText);

                                fullText += pageText + "\n\n";

                                pages.Add(new Dictionary<string, object>
                                {
                                    { "pageNumber", i },
                                    { "content", pageText },
                                    { "hasContent", !string.IsNullOrWhiteSpace(pageText) }
                                });
                            }
                            catch (Exception ex)
                            {
                                // Se houver erro em uma página específica, adiciona erro mas continua
                                pages.Add(new Dictionary<string, object>
                                {
                                    { "pageNumber", i },
                                    { "content", "" },
                                    { "hasContent", false },
                                    { "error", $"Erro ao processar página: {ex.Message}" }
                                });
                            }
                        }

                        // Se nenhuma página teve conteúdo, tenta extrair metadados
                        if (string.IsNullOrWhiteSpace(fullText))
                        {
                            var metadata = ExtractMetadata(pdfReader);

                            var resultWithMetadata = new
                            {
                                fileName = fileName,
                                fileType = "PDF",
                                totalPages = pages.Count,
                                pages = pages,
                                fullText = fullText.Trim(),
                                metadata = metadata,
                                warning = "Não foi possível extrair texto do PDF. O arquivo pode conter apenas imagens ou estar protegido."
                            };

                            var jsonString = JsonConvert.SerializeObject(resultWithMetadata);
                            return JToken.Parse(jsonString);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao processar PDF: {ex.Message}", ex);
                }

                var resultObject = new
                {
                    fileName = fileName,
                    fileType = "PDF",
                    totalPages = pages.Count,
                    pages = pages,
                    fullText = fullText.Trim()
                };

                var json = JsonConvert.SerializeObject(resultObject);
                return JToken.Parse(json);
            });
        }

        /// <summary>
        /// Limpa o texto extraído removendo espaços extras
        /// </summary>
        private string CleanText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Remove múltiplas linhas vazias
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            var cleanedLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    cleanedLines.Add(trimmed);
                }
            }

            return string.Join("\n", cleanedLines);
        }

        /// <summary>
        /// Extrai metadados do PDF
        /// </summary>
        private Dictionary<string, string> ExtractMetadata(PdfReader reader)
        {
            var metadata = new Dictionary<string, string>();

            try
            {
                var info = reader.Info;
                if (info != null)
                {
                    foreach (var key in info.Keys)
                    {
                        if (info[key] != null)
                        {
                            metadata[key] = info[key].ToString();
                        }
                    }
                }

                // Adiciona informações básicas
                metadata["NumberOfPages"] = reader.NumberOfPages.ToString();
                metadata["FileSize"] = reader.FileLength.ToString();
                metadata["PdfVersion"] = reader.PdfVersion.ToString();
            }
            catch
            {
                // Se falhar ao extrair metadados, apenas ignora
            }

            return metadata;
        }
    }
}