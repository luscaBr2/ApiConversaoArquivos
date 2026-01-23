using Newtonsoft.Json.Linq;

namespace ApiConversaoArquivos.Services.Interfaces
{
    /// <summary>
    /// Interface que define o contrato para serviços de conversão de arquivos
    /// Usar interfaces permite facilitar testes e manter o código desacoplado
    /// 
    /// Como usar: Esta interface será implementada por cada serviço específico de conversão.
    /// </summary>
    public interface IFileConverterService
    {
        /// <summary>
        /// Converte um arquivo para formato JSON
        /// </summary>
        /// <param name="fileStream">Stream do arquivo a ser convertido</param>
        /// <param name="fileName">Nome do arquivo (usado para validação de extensão)</param>
        /// <returns>Dados convertidos em formato JToken</returns>
        Task<JToken> ConvertToJsonAsync(Stream fileStream, string fileName);
    }
}