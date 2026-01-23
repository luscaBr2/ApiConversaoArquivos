namespace ApiConversaoArquivos.Models
{
    /// <summary>
    /// Modelo que representa a resposta da conversão
    /// Padroniza todas as respostas da API
    /// 
    /// Como usar: Este modelo será retornado por todos os endpoints da API.
    /// </summary>
    public class ConversionResponse
    {
        /// <summary>
        /// Indica se a conversão foi bem-sucedida
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensagem descritiva sobre o resultado da operação
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Dados convertidos em formato object (será serializado como JSON)
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Mensagem de erro, caso ocorra algum problema
        /// </summary>
        public string? Error { get; set; }

        public string? Json { get; set; }
    }
}