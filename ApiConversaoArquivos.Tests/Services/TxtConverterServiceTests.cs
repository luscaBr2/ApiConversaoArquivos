using ApiConversaoArquivos.Services.Implementations;

namespace ApiConversaoArquivos.Tests.Services
{
    public class TxtConverterServiceTests
    {
        private readonly TxtConverterService _service;

        public TxtConverterServiceTests()
        {
            _service = new TxtConverterService();
        }

        [Fact]
        public async Task ConvertToJsonAsync_ValidText_ShouldReturnCorrectJson()
        {
            // Arrange
            var textContent = "Linha 1\nLinha 2\nLinha 3";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(textContent));

            // Act
            var result = await _service.ConvertToJsonAsync(stream, "test.txt");

            // Assert
            result.Should().NotBeNull();
            var jsonObject = JObject.Parse(result.ToString());
            jsonObject["fileName"].ToString().Should().Be("test.txt");
            jsonObject["fileType"].ToString().Should().Be("Text");
            jsonObject["totalLines"].Value<int>().Should().Be(3);

            var lines = JArray.Parse(jsonObject["lines"].ToString());
            lines.Should().HaveCount(3);
            lines[0]["lineNumber"].Value<int>().Should().Be(1);
            lines[0]["content"].ToString().Should().Be("Linha 1");
            lines[0]["isEmpty"].Value<bool>().Should().BeFalse();
        }

        [Fact]
        public async Task ConvertToJsonAsync_EmptyLines_ShouldDetectEmptyLines()
        {
            // Arrange
            var textContent = "Linha 1\n\nLinha 3";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(textContent));

            // Act
            var result = await _service.ConvertToJsonAsync(stream, "test.txt");

            // Assert
            result.Should().NotBeNull();
            var jsonObject = JObject.Parse(result.ToString());
            var lines = JArray.Parse(jsonObject["lines"].ToString());
            lines.Should().HaveCount(3);
            lines[1]["isEmpty"].Value<bool>().Should().BeTrue();
            lines[1]["content"].ToString().Should().BeEmpty();
        }

        [Fact]
        public async Task ConvertToJsonAsync_EmptyFile_ShouldReturnEmptyLines()
        {
            // Arrange
            var textContent = "";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(textContent));

            // Act
            var result = await _service.ConvertToJsonAsync(stream, "empty.txt");

            // Assert
            result.Should().NotBeNull();
            var jsonObject = JObject.Parse(result.ToString());
            jsonObject["totalLines"].Value<int>().Should().Be(0);
        }
    }
}