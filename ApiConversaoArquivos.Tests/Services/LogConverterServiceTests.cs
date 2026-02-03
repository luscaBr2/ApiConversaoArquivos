using ApiConversaoArquivos.Services.Implementations;

namespace ApiConversaoArquivos.Tests.Services
{
    public class LogConverterServiceTests
    {
        private readonly LogConverterService _service;

        public LogConverterServiceTests()
        {
            _service = new LogConverterService();
        }

        [Fact]
        public async Task ConvertToJsonAsync_ValidLog_ShouldDetectLogLevels()
        {
            // Arrange
            var logContent = @"[2024-01-24T10:30:00] INFO Application started
[2024-01-24T10:30:05] DEBUG Loading configuration
[2024-01-24T10:40:15] ERROR Failed to process request";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(logContent));

            // Act
            var result = await _service.ConvertToJsonAsync(stream, "test.log");

            // Assert
            result.Should().NotBeNull();
            var jsonObject = JObject.Parse(result.ToString());
            jsonObject["fileType"].ToString().Should().Be("Log");
            jsonObject["totalLines"].Value<int>().Should().Be(3);
            jsonObject["errorCount"].Value<int>().Should().Be(1);

            var stats = JObject.Parse(jsonObject["logLevelStats"].ToString());
            stats["INFO"].Value<int>().Should().Be(1);
            stats["DEBUG"].Value<int>().Should().Be(1);
            stats["ERROR"].Value<int>().Should().Be(1);
        }

        [Fact]
        public async Task ConvertToJsonAsync_LogWithTimestamp_ShouldExtractTimestamp()
        {
            // Arrange
            var logContent = "[2024-01-24T10:30:00] INFO Test message";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(logContent));

            // Act
            var result = await _service.ConvertToJsonAsync(stream, "test.log");

            // Assert
            var jsonObject = JObject.Parse(result.ToString());
            var entries = JArray.Parse(jsonObject["entries"].ToString());

            // Verificar que o timestamp existe (formato pode variar)
            entries[0]["timestamp"].Should().NotBeNull();
            entries[0]["timestamp"].ToString().Should().NotBeEmpty();
            // Verificar que contém elementos da data
            entries[0]["timestamp"].ToString().Should().Contain("2024");
            entries[0]["timestamp"].ToString().Should().Contain("01");
            entries[0]["timestamp"].ToString().Should().Contain("24");

            entries[0]["logLevel"].ToString().Should().Be("INFO");
        }

        [Fact]
        public async Task ConvertToJsonAsync_ErrorLog_ShouldMarkAsError()
        {
            // Arrange
            var logContent = "[2024-01-24T10:40:15] ERROR Connection failed\n[2024-01-24T10:40:16] FATAL System crash";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(logContent));

            // Act
            var result = await _service.ConvertToJsonAsync(stream, "test.log");

            // Assert
            var jsonObject = JObject.Parse(result.ToString());
            jsonObject["errorCount"].Value<int>().Should().Be(2);

            var entries = JArray.Parse(jsonObject["entries"].ToString());
            entries[0]["isError"].Value<bool>().Should().BeTrue();
            entries[1]["isError"].Value<bool>().Should().BeTrue();
        }

        [Fact]
        public async Task ConvertToJsonAsync_MixedLogLevels_ShouldCountCorrectly()
        {
            // Arrange
            var logContent = @"INFO Starting application
                                DEBUG Configuration loaded
                                WARN Cache miss
                                ERROR Database connection failed
                                INFO Request completed";

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(logContent));

            // Act
            var result = await _service.ConvertToJsonAsync(stream, "test.log");

            // Assert
            var jsonObject = JObject.Parse(result.ToString());
            jsonObject["totalLines"].Value<int>().Should().Be(5);
            jsonObject["errorCount"].Value<int>().Should().Be(1);

            var stats = JObject.Parse(jsonObject["logLevelStats"].ToString());
            stats["INFO"].Value<int>().Should().Be(2);
            stats["DEBUG"].Value<int>().Should().Be(1);
            stats["WARN"].Value<int>().Should().Be(1);
            stats["ERROR"].Value<int>().Should().Be(1);
        }
    }
}