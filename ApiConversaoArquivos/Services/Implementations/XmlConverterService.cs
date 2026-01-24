using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ApiConversaoArquivos.Services.Interfaces;
using System.Xml.Linq;
using System.Text;

namespace ApiConversaoArquivos.Services.Implementations
{
    public class XmlConverterService : IFileConverterService
    {
        public async Task<JToken> ConvertToJsonAsync(Stream fileStream, string fileName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var reader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        var xmlContent = reader.ReadToEnd();
                        var xDocument = XDocument.Parse(xmlContent);

                        // Converter XML para JSON preservando a estrutura
                        var jsonFromXml = JsonConvert.SerializeXNode(xDocument, Formatting.Indented, omitRootObject: false);
                        var parsedJson = JObject.Parse(jsonFromXml);

                        var resultObject = new
                        {
                            fileName = fileName,
                            fileType = "XML",
                            rootElement = xDocument.Root?.Name.LocalName ?? "unknown",
                            hasNamespace = xDocument.Root?.Name.Namespace != null && !string.IsNullOrEmpty(xDocument.Root.Name.Namespace.NamespaceName),
                            xmlData = parsedJson,
                            rawXml = xmlContent
                        };

                        var json = JsonConvert.SerializeObject(resultObject);
                        return JToken.Parse(json);
                    }
                }
                catch (System.Xml.XmlException ex)
                {
                    throw new Exception($"Erro ao processar XML: XML malformado ou inválido - {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao processar arquivo XML: {ex.Message}", ex);
                }
            });
        }
    }
}