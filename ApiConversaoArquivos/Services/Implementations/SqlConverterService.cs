using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ApiConversaoArquivos.Services.Interfaces;

namespace ApiConversaoArquivos.Services.Implementations
{
    /// <summary>
    /// Serviço responsável pela execução de queries SQL e conversão para JSON
    /// </summary>
    public class SqlConverterService : IFileConverterService
    {
        /// <summary>
        /// Converte arquivo .sql em JSON executando as queries contidas nele
        /// </summary>
        public Task<JToken> ConvertToJsonAsync(Stream fileStream, string fileName)
        {
            throw new InvalidOperationException(
                "Para converter arquivos SQL, use o método ExecuteQueryFromFileAsync com a connection string"
            );
        }

        /// <summary>
        /// Lê um arquivo .sql e executa as queries nele contidas
        /// </summary>
        public async Task<JToken> ExecuteQueryFromFileAsync(Stream fileStream, string fileName, string connectionString)
        {
            try
            {
                // Lê todo o conteúdo do arquivo .sql
                using var reader = new StreamReader(fileStream);
                var sqlQuery = await reader.ReadToEndAsync();

                // Valida se o arquivo não está vazio
                if (string.IsNullOrWhiteSpace(sqlQuery))
                {
                    throw new Exception("O arquivo SQL está vazio");
                }

                // Executa a query lida do arquivo
                return await ExecuteQueryAsync(connectionString, sqlQuery, fileName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao processar arquivo SQL: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Executa uma query SQL e converte o resultado em JSON
        /// </summary>
        public async Task<JToken> ExecuteQueryAsync(string connectionString, string query, string fileName = "SQL Query")
        {
            var records = new List<Dictionary<string, object>>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.CommandTimeout = 30;

                        using (var dbReader = await command.ExecuteReaderAsync())
                        {
                            while (await dbReader.ReadAsync())
                            {
                                var row = new Dictionary<string, object>();

                                for (int i = 0; i < dbReader.FieldCount; i++)
                                {
                                    var columnName = dbReader.GetName(i);
                                    var columnValue = dbReader.GetValue(i);

                                    row[columnName] = columnValue == DBNull.Value ? null : columnValue;
                                }

                                records.Add(row);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Erro SQL: {ex.Message}", ex);
            }

            var resultObject = new
            {
                fileName = fileName,
                queryType = "SQL",
                totalRecords = records.Count,
                data = records
            };

            return JObject.FromObject(resultObject);
        }
    }
}