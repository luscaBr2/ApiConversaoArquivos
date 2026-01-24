using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ApiConversaoArquivos.Services.Interfaces;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace ApiConversaoArquivos.Services.Implementations
{
    public class DocxConverterService : IFileConverterService
    {
        public async Task<JToken> ConvertToJsonAsync(Stream fileStream, string fileName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var paragraphs = new List<Dictionary<string, object>>();
                    var tables = new List<Dictionary<string, object>>();
                    var fullText = new StringBuilder();
                    int paragraphCount = 0;
                    int tableCount = 0;

                    using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(fileStream, false))
                    {
                        var body = wordDoc.MainDocumentPart?.Document?.Body;

                        if (body == null)
                        {
                            throw new Exception("Documento Word vazio ou inválido");
                        }

                        foreach (var element in body.Elements())
                        {
                            if (element is Paragraph paragraph)
                            {
                                var text = GetParagraphText(paragraph);

                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    var paragraphStyle = GetParagraphStyle(paragraph);

                                    paragraphs.Add(new Dictionary<string, object>
                                    {
                                        { "index", paragraphCount++ },
                                        { "text", text },
                                        { "style", paragraphStyle },
                                        { "isHeading", IsHeading(paragraphStyle) },
                                        { "isBold", IsBold(paragraph) },
                                        { "isItalic", IsItalic(paragraph) }
                                    });

                                    fullText.AppendLine(text);
                                }
                            }
                            else if (element is Table table)
                            {
                                var tableData = ProcessTable(table);
                                tableData["index"] = tableCount++;
                                tables.Add(tableData);
                            }
                        }
                    }

                    var resultObject = new
                    {
                        fileName = fileName,
                        fileType = "Word",
                        totalParagraphs = paragraphs.Count,
                        totalTables = tables.Count,
                        paragraphs = paragraphs,
                        tables = tables,
                        fullText = fullText.ToString().Trim()
                    };

                    var json = JsonConvert.SerializeObject(resultObject);
                    return JToken.Parse(json);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao processar arquivo Word: {ex.Message}", ex);
                }
            });
        }

        private string GetParagraphText(Paragraph paragraph)
        {
            var text = new StringBuilder();

            foreach (var run in paragraph.Elements<Run>())
            {
                foreach (var textElement in run.Elements<Text>())
                {
                    text.Append(textElement.Text);
                }
            }

            return text.ToString();
        }

        private string GetParagraphStyle(Paragraph paragraph)
        {
            var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
            return styleId ?? "Normal";
        }

        private bool IsHeading(string style)
        {
            return style.ToLower().Contains("heading") ||
                   style.ToLower().Contains("title") ||
                   style.ToLower().StartsWith("h");
        }

        private bool IsBold(Paragraph paragraph)
        {
            foreach (var run in paragraph.Elements<Run>())
            {
                var bold = run.RunProperties?.Bold;
                if (bold != null && bold.Val?.Value != false)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsItalic(Paragraph paragraph)
        {
            foreach (var run in paragraph.Elements<Run>())
            {
                var italic = run.RunProperties?.Italic;
                if (italic != null && italic.Val?.Value != false)
                {
                    return true;
                }
            }
            return false;
        }

        private Dictionary<string, object> ProcessTable(Table table)
        {
            var rows = new List<List<string>>();
            var headers = new List<string>();
            bool isFirstRow = true;

            foreach (var row in table.Elements<TableRow>())
            {
                var cells = new List<string>();

                foreach (var cell in row.Elements<TableCell>())
                {
                    var cellText = new StringBuilder();

                    foreach (var paragraph in cell.Elements<Paragraph>())
                    {
                        var text = GetParagraphText(paragraph);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            cellText.Append(text);
                        }
                    }

                    cells.Add(cellText.ToString());
                }

                if (isFirstRow)
                {
                    headers = cells;
                    isFirstRow = false;
                }
                else
                {
                    rows.Add(cells);
                }
            }

            return new Dictionary<string, object>
            {
                { "headers", headers },
                { "rowCount", rows.Count },
                { "columnCount", headers.Count },
                { "rows", rows }
            };
        }
    }
}