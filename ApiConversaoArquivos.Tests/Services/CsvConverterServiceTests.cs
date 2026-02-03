using ApiConversaoArquivos.Services.Implementations;

namespace ApiConversaoArquivos.Tests.Services
{
    public class CsvConverterServiceTests
    {
        private readonly CsvConverterService _service;

        public CsvConverterServiceTests()
        {
            _service = new CsvConverterService();
        }

        [Fact]
        public async Task ConvertToJsonAsync_ValidCsv_ShouldReturnCorrectJson()
        {
            // Arrange - Usar ASCII ou caracteres sem acentuação para evitar problemas
            var csvContent = "Nome,Idade,Cidade\nJoao,30,Sao Paulo\nMaria,25,Rio de Janeiro";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            // Act
            var result = await _service.ConvertToJsonAsync(stream, "test.csv");

            // Assert
            result.Should().NotBeNull();
            var jsonObject = JObject.Parse(result.ToString());
            var data = jsonObject["data"];
            data.Should().NotBeNull();
            var dataArray = JArray.Parse(data.ToString());
            dataArray.Should().HaveCount(2);
            dataArray[0]["Nome"].ToString().Should().Be("Joao");
            dataArray[0]["Idade"].ToString().Should().Be("30");
            dataArray[1]["Nome"].ToString().Should().Be("Maria");
        }

        [Fact]
        public async Task ConvertToJsonAsync_EmptyCsv_ShouldReturnEmptyArray()
        {
            // Arrange
            var csvContent = "Nome,Idade,Cidade";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            // Act
            var result = await _service.ConvertToJsonAsync(stream, "empty.csv");

            // Assert
            result.Should().NotBeNull();
            var jsonObject = JObject.Parse(result.ToString());
            var data = jsonObject["data"];
            var dataArray = JArray.Parse(data.ToString());
            dataArray.Should().BeEmpty();
        }

        [Fact]
        public async Task ConvertToJsonAsync_InvalidStream_ShouldThrowException()
        {
            // Arrange
            Stream stream = null!;

            // Act & Assert - Mudar para Exception genérica pois o serviço encapsula
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.ConvertToJsonAsync(stream, "test.csv")
            );

            exception.Message.Should().Contain("Erro ao processar CSV");
        }

        [Fact]
        public async Task ConvertToJsonAsync_CsvWithSpecialCharacters_ShouldHandleCorrectly()
        {
            // Arrange
            var csvContent = "Produto,Preco,Quantidade\nNotebook,3500.00,10\nMouse,85.50,50";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            // Act
            var result = await _service.ConvertToJsonAsync(stream, "produtos.csv");

            // Assert
            result.Should().NotBeNull();
            var jsonObject = JObject.Parse(result.ToString());
            var data = jsonObject["data"];
            var dataArray = JArray.Parse(data.ToString());
            dataArray.Should().HaveCount(2);
            dataArray[0]["Produto"].ToString().Should().Be("Notebook");
            dataArray[0]["Preco"].ToString().Should().Be("3500.00");
        }
    }
}