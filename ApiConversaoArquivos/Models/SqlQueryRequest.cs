using System.ComponentModel.DataAnnotations;

namespace ApiConversaoArquivos.Models
{
    /// <summary>
    /// Modelo específico para requisições SQL
    /// </summary>
    public class SqlQueryRequest
    {
        /// <summary>
        /// String de conexão com o banco de dados SQL Server
        /// [Required] torna este campo obrigatório
        /// </summary>
        [Required(ErrorMessage = "A string de conexão é obrigatória")]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Query SQL a ser executada
        /// [Required] torna este campo obrigatório
        /// </summary>
        [Required(ErrorMessage = "A query SQL é obrigatória")]
        public string SqlQuery { get; set; }
    }
}