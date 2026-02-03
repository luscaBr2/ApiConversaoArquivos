namespace ApiConversaoArquivos.Models
{
    public class BatchConversionRequest
    {
        public List<IFormFile> Files { get; set; } = new();
        public bool CombineResults { get; set; } = false;
    }

    public class BatchConversionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalFiles { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<BatchFileResult> Results { get; set; } = new();
        public object? CombinedData { get; set; }
    }

    public class BatchFileResult
    {
        public string FileName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
        public string? Error { get; set; }
    }
}