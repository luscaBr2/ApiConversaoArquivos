using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ApiConversaoArquivos.Services.Interfaces;
using ExcelDataReader;
using System.Text;
using System.Data;

namespace ApiConversaoArquivos.Services.Implementations
{
    /// <summary>
    /// Serviço responsável pela conversão de arquivos Excel (.xlsx, .xls, .xlsm) para JSON
    /// Utiliza ExcelDataReader para suportar todos os formatos Excel
    /// </summary>
    public class ExcelConverterService : IFileConverterService
    {
        public ExcelConverterService()
        {
            // Registra o CodePagesEncodingProvider para suportar encodings antigos do .xls
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// Converte Excel (.xlsx, .xls, .xlsm) em JSON
        /// </summary>
        public async Task<JToken> ConvertToJsonAsync(Stream fileStream, string fileName)
        {
            return await Task.Run(() =>
            {
                var sheets = new List<Dictionary<string, object>>();

                try
                {
                    using (var reader = ExcelReaderFactory.CreateReader(fileStream))
                    {
                        // Converte para DataSet
                        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration
                            {
                                UseHeaderRow = true // Primeira linha como cabeçalho
                            }
                        });

                        // Processa cada planilha
                        foreach (DataTable table in dataSet.Tables)
                        {
                            var sheetData = new List<Dictionary<string, object>>();

                            // Lê os dados
                            foreach (DataRow row in table.Rows)
                            {
                                var rowData = new Dictionary<string, object>();

                                foreach (DataColumn column in table.Columns)
                                {
                                    var value = row[column];

                                    // Converte DBNull para string vazia
                                    // Mantém o tipo original dos outros valores
                                    rowData[column.ColumnName] = value == DBNull.Value ? "" : value;
                                }

                                sheetData.Add(rowData);
                            }

                            sheets.Add(new Dictionary<string, object>
                            {
                                { "sheetName", table.TableName },
                                { "rowCount", table.Rows.Count },
                                { "data", sheetData }
                            });
                        }
                    }

                    var resultObject = new
                    {
                        fileName = fileName,
                        fileType = "Excel",
                        totalSheets = sheets.Count,
                        sheets = sheets
                    };

                    var json = JsonConvert.SerializeObject(resultObject);
                    return JToken.Parse(json);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao processar arquivo Excel: {ex.Message}", ex);
                }
            });
        }
    }
}