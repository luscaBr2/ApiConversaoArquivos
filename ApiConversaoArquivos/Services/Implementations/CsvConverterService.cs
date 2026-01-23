using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json.Linq;
using System.Globalization;
using ApiConversaoArquivos.Services.Interfaces;

namespace ApiConversaoArquivos.Services.Implementations
{
    /// <summary>
    /// Serviço responsável pela conversão de arquivos CSV para JSON
    /// </summary>
    public class CsvConverterService : IFileConverterService
    {
        /// <summary>
        /// Converte CSV em JSON lendo linha por linha
        /// </summary>
        public async Task<JToken> ConvertToJsonAsync(Stream fileStream, string fileName)
        {
            return await Task.Run(() =>
            {
                var records = new List<Dictionary<string, string>>();

                try
                {
                    // Detecta encoding automaticamente (suporta caracteres acentuados)
                    using (var reader = new StreamReader(fileStream, System.Text.Encoding.GetEncoding("ISO-8859-1"), detectEncodingFromByteOrderMarks: true))
                    {
                        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            HasHeaderRecord = true,
                            TrimOptions = TrimOptions.Trim,
                            Delimiter = ",",
                            IgnoreBlankLines = true,
                            BadDataFound = null
                        };

                        using (var csv = new CsvReader(reader, config))
                        {
                            // Lê o cabeçalho
                            csv.Read();
                            csv.ReadHeader();

                            var headers = csv.HeaderRecord;

                            if (headers == null || headers.Length == 0)
                            {
                                throw new Exception("O arquivo CSV não contém cabeçalhos válidos");
                            }

                            // Lê cada linha de dados
                            while (csv.Read())
                            {
                                var row = new Dictionary<string, string>();

                                foreach (var header in headers)
                                {
                                    var fieldValue = csv.GetField(header);
                                    row[header] = fieldValue ?? string.Empty;
                                }

                                records.Add(row);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao processar CSV: {ex.Message}", ex);
                }

                // Cria objeto de resultado - ESTE é o objeto que será retornado
                var resultObject = new
                {
                    fileName = fileName,
                    fileType = "CSV",
                    totalRecords = records.Count,
                    data = records
                };

                // Retorna como JObject para manter compatibilidade
                return JObject.FromObject(resultObject);
            });
        }
    }
}