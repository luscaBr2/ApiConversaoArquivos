using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ApiConversaoArquivos.Services.Interfaces;
using System.Text;

namespace ApiConversaoArquivos.Services.Implementations
{
    public class TxtConverterService : IFileConverterService
    {
        public async Task<JToken> ConvertToJsonAsync(Stream fileStream, string fileName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var lines = new List<Dictionary<string, object>>();
                    var fullText = new StringBuilder();
                    int lineNumber = 0;

                    using (var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                    {
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            lineNumber++;

                            lines.Add(new Dictionary<string, object>
                            {
                                { "lineNumber", lineNumber },
                                { "content", line },
                                { "length", line.Length },
                                { "isEmpty", string.IsNullOrWhiteSpace(line) }
                            });

                            fullText.AppendLine(line);
                        }
                    }

                    var resultObject = new
                    {
                        fileName = fileName,
                        fileType = "Text",
                        totalLines = lines.Count,
                        encoding = "UTF-8",
                        lines = lines,
                        fullText = fullText.ToString()
                    };

                    var json = JsonConvert.SerializeObject(resultObject);
                    return JToken.Parse(json);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao processar arquivo de texto: {ex.Message}", ex);
                }
            });
        }
    }
}