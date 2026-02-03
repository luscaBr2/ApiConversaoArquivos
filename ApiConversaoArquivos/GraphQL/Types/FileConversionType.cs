namespace ApiConversaoArquivos.GraphQL.Types
{
    public class FileConversionType
    {
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public object? Data { get; set; }
        public string? Error { get; set; }
    }

    public class ConversionResultType
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public FileConversionType? Conversion { get; set; }
    }
}