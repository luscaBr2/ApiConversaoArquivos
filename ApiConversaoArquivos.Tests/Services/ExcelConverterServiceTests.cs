using ApiConversaoArquivos.Services.Implementations;

namespace ApiConversaoArquivos.Tests.Services
{
    public class ExcelConverterServiceTests
    {
        private readonly ExcelConverterService _service;

        public ExcelConverterServiceTests()
        {
            _service = new ExcelConverterService();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [Fact]
        public async Task ConvertToJsonAsync_InvalidStream_ShouldThrowException()
        {
            // Arrange - Stream vazio não é Excel válido
            var stream = new MemoryStream();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.ConvertToJsonAsync(stream, "test.xlsx")
            );

            exception.Message.Should().Contain("Erro ao processar arquivo Excel");
        }

        [Fact]
        public async Task ConvertToJsonAsync_NullStream_ShouldThrowException()
        {
            // Arrange
            Stream stream = null!;

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _service.ConvertToJsonAsync(stream, "test.xlsx")
            );
        }

        // Este teste requer um arquivo Excel real em TestFiles/
        [Fact(Skip = "Requer arquivo Excel real em TestFiles/test.xlsx")]
        public async Task ConvertToJsonAsync_RealExcelFile_ShouldReturnCorrectStructure()
        {
            // Arrange
            var filePath = Path.Combine("TestFiles", "test.xlsx");

            if (!File.Exists(filePath))
            {
                // Skip do teste se o arquivo não existir
                return;
            }

            using var fileStream = File.OpenRead(filePath);
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            // Act
            var result = await _service.ConvertToJsonAsync(memoryStream, "test.xlsx");

            // Assert
            result.Should().NotBeNull();
            var jsonObject = JObject.Parse(result.ToString());
            jsonObject["fileName"].ToString().Should().Be("test.xlsx");
            jsonObject["fileType"].ToString().Should().Be("Excel");
            jsonObject["sheets"].Should().NotBeNull();
        }
    }
}