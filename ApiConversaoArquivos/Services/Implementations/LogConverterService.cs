using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ApiConversaoArquivos.Services.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace ApiConversaoArquivos.Services.Implementations
{
    public class LogConverterService : IFileConverterService
    {
        public async Task<JToken> ConvertToJsonAsync(Stream fileStream, string fileName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var logEntries = new List<Dictionary<string, object>>();
                    var fullText = new StringBuilder();
                    int lineNumber = 0;

                    // Regex para detectar padrões comuns de log
                    var timestampPattern = @"^\[?(\d{4}-\d{2}-\d{2}[T\s]\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})?)\]?";
                    var logLevelPattern = @"\b(DEBUG|INFO|WARN|WARNING|ERROR|FATAL|TRACE|CRITICAL)\b";

                    using (var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                    {
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            lineNumber++;

                            var entry = new Dictionary<string, object>
                            {
                                { "lineNumber", lineNumber },
                                { "content", line },
                                { "length", line.Length }
                            };

                            // Tentar extrair timestamp
                            var timestampMatch = Regex.Match(line, timestampPattern);
                            if (timestampMatch.Success)
                            {
                                entry["timestamp"] = timestampMatch.Groups[1].Value;
                            }

                            // Tentar extrair nível de log
                            var logLevelMatch = Regex.Match(line, logLevelPattern, RegexOptions.IgnoreCase);
                            if (logLevelMatch.Success)
                            {
                                entry["logLevel"] = logLevelMatch.Groups[1].Value.ToUpper();
                            }

                            // Detectar se é linha de erro/exceção
                            entry["isError"] = line.Contains("ERROR", StringComparison.OrdinalIgnoreCase) ||
                                              line.Contains("EXCEPTION", StringComparison.OrdinalIgnoreCase) ||
                                              line.Contains("FATAL", StringComparison.OrdinalIgnoreCase);

                            entry["isEmpty"] = string.IsNullOrWhiteSpace(line);

                            logEntries.Add(entry);
                            fullText.AppendLine(line);
                        }
                    }

                    // Estatísticas
                    var errorCount = logEntries.Count(e => e.ContainsKey("isError") && (bool)e["isError"]);
                    var logLevels = logEntries
                        .Where(e => e.ContainsKey("logLevel"))
                        .GroupBy(e => e["logLevel"])
                        .ToDictionary(g => g.Key.ToString()!, g => g.Count());

                    var resultObject = new
                    {
                        fileName = fileName,
                        fileType = "Log",
                        totalLines = logEntries.Count,
                        errorCount = errorCount,
                        logLevelStats = logLevels,
                        encoding = "UTF-8",
                        entries = logEntries,
                        fullText = fullText.ToString()
                    };

                    var json = JsonConvert.SerializeObject(resultObject);
                    return JToken.Parse(json);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao processar arquivo de log: {ex.Message}", ex);
                }
            });
        }
    }
}