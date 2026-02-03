using ApiConversaoArquivos.GraphQL.Types;
using ApiConversaoArquivos.Services.Implementations;
using Newtonsoft.Json;
using Path = System.IO.Path;

namespace ApiConversaoArquivos.GraphQL
{
    public class Mutation
    {
        public async Task<ConversionResultType> ConvertFile(
            IFile file,
            [Service] PdfConverterService pdfService,
            [Service] CsvConverterService csvService,
            [Service] DocxConverterService docxService)
        {
            try
            {
                var fileName = file.Name;
                var extension = Path.GetExtension(fileName).ToLower();

                using var stream = file.OpenReadStream();
                object? result = null;

                switch (extension)
                {
                    case ".pdf":
                        result = await pdfService.ConvertToJsonAsync(stream, fileName);
                        break;
                    case ".csv":
                        result = await csvService.ConvertToJsonAsync(stream, fileName);
                        break;
                    case ".docx":
                        result = await docxService.ConvertToJsonAsync(stream, fileName);
                        break;
                    default:
                        return new ConversionResultType
                        {
                            Success = false,
                            Message = $"Formato não suportado: {extension}"
                        };
                }

                return new ConversionResultType
                {
                    Success = true,
                    Message = "Arquivo convertido com sucesso",
                    Conversion = new FileConversionType
                    {
                        FileName = fileName,
                        FileType = extension.TrimStart('.').ToUpper(),
                        Data = result
                    }
                };
            }
            catch (Exception ex)
            {
                return new ConversionResultType
                {
                    Success = false,
                    Message = $"Erro: {ex.Message}"
                };
            }
        }
    }
}