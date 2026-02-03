using ApiConversaoArquivos.Services.Implementations;

namespace ApiConversaoArquivos.Tests.Services
{
    public class XmlConverterServiceTests
    {
        private readonly XmlConverterService _service;

        public XmlConverterServiceTests()
        {
            _service = new XmlConverterService();
        }

        [Fact]
        public async Task ConvertToJsonAsync_ValidXml_ShouldReturnCorrectJson()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                                <configuration>
                                    <database>
                                        <host>localhost</host>
                                        <port>5432</port>
                                    </database>
                                </configuration>";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlContent));

            // Act
            var result = await _service.ConvertToJsonAsync(stream, "test.xml");

            // Assert
            result.Should().NotBeNull();
            var jsonObject = JObject.Parse(result.ToString());
            jsonObject["fileName"].ToString().Should().Be("test.xml");
            jsonObject["fileType"].ToString().Should().Be("XML");
            jsonObject["rootElement"].ToString().Should().Be("configuration");
            jsonObject["xmlData"].Should().NotBeNull();
            jsonObject["rawXml"].Should().NotBeNull();
        }

        [Fact]
        public async Task ConvertToJsonAsync_XmlWithAttributes_ShouldPreserveAttributes()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0""?>
                                <root>
                                    <item id=""1"" enabled=""true"">Test</item>
                                </root>";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlContent));

            // Act
            var result = await _service.ConvertToJsonAsync(stream, "test.xml");

            // Assert
            result.Should().NotBeNull();
            var jsonObject = JObject.Parse(result.ToString());
            jsonObject["rootElement"].ToString().Should().Be("root");
            var xmlData = JObject.Parse(jsonObject["xmlData"].ToString());
            xmlData.Should().NotBeNull();
        }

        [Fact]
        public async Task ConvertToJsonAsync_InvalidXml_ShouldThrowException()
        {
            // Arrange
            var xmlContent = "<invalid><unclosed>";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlContent));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.ConvertToJsonAsync(stream, "invalid.xml")
            );

            exception.Message.Should().Contain("Erro ao processar");
        }

        [Fact]
        public async Task ConvertToJsonAsync_EmptyXml_ShouldThrowException()
        {
            // Arrange
            var xmlContent = "";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlContent));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _service.ConvertToJsonAsync(stream, "empty.xml")
            );
        }

        [Fact]
        public async Task ConvertToJsonAsync_ComplexXml_ShouldParseCorrectly()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0""?>
                            <catalog>
                                <book id=""1"">
                                    <author>John Doe</author>
                                    <title>Sample Book</title>
                                    <price>29.99</price>
                                </book>
                                <book id=""2"">
                                    <author>Jane Smith</author>
                                    <title>Another Book</title>
                                    <price>39.99</price>
                                </book>
                        </catalog>";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlContent));

            // Act
            var result = await _service.ConvertToJsonAsync(stream, "catalog.xml");

            // Assert
            result.Should().NotBeNull();
            var jsonObject = JObject.Parse(result.ToString());
            jsonObject["rootElement"].ToString().Should().Be("catalog");
            jsonObject["fileType"].ToString().Should().Be("XML");
        }
    }
}