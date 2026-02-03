using ApiConversaoArquivos.GraphQL.Types;
using ApiConversaoArquivos.Services.Implementations;
using HotChocolate.Types;

namespace ApiConversaoArquivos.GraphQL
{
    public class Query
    {
        public string GetApiVersion() => "1.3.0";

        public string GetApiStatus() => "healthy";

        public List<string> GetSupportedFormats() => new()
        {
            ".pdf", ".xlsx", ".xls", ".xlsm", ".csv",
            ".docx", ".xml", ".txt", ".log"
        };

        public async Task<string> GetHealthCheck()
        {
            return await Task.FromResult($"API Online - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        }
    }
}