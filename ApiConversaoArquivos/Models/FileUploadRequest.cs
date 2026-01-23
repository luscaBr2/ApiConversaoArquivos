using System.ComponentModel.DataAnnotations;

namespace ApiConversaoArquivos.Models
{
    /// <summary>
    /// Modelo que representa a requisição de upload de arquivo
    /// 
    /// Como usar: Este modelo é usado para receber arquivos de qualquer tipo suportado.
    /// O sistema identifica automaticamente o tipo e faz a conversão apropriada.
    /// </summary>
    public class FileUploadRequest
    {
        /// <summary>
        /// Arquivo enviado pelo cliente
        /// Pode ser: PDF, Excel (.xlsx, .xls), CSV ou arquivo SQL (.sql)
        /// [Required] garante que o arquivo seja obrigatório
        /// </summary>
        [Required(ErrorMessage = "O arquivo é obrigatório")]
        public IFormFile File { get; set; }

        /// <summary>
        /// String de conexão SQL (OPCIONAL - usado apenas para arquivos .sql)
        /// Exemplo: "Server=localhost;Database=MinhaDB;User Id=sa;Password=senha;TrustServerCertificate=True;"
        /// </summary>
        public string? ConnectionString { get; set; }
    }
}