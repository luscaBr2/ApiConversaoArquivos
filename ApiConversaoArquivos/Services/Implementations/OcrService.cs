using Tesseract;
using PdfiumViewer;
using System.Drawing;
using System.Drawing.Imaging;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using Path = System.IO.Path;

namespace ApiConversaoArquivos.Services.Implementations
{
    public class OcrService
    {
        private readonly string _tesseractPath;
        private readonly string _tessDataPath;

        public OcrService()
        {
            // Configurar caminhos do Tesseract
            // Windows
            _tesseractPath = @"C:\Program Files\Tesseract-OCR";
            _tessDataPath = Path.Combine(_tesseractPath, "tessdata");

            // Linux (descomente se estiver no Linux)
            // _tesseractPath = "/usr/bin/tesseract";
            // _tessDataPath = "/usr/share/tesseract-ocr/4.00/tessdata";
        }

        public async Task<string> ExtractTextFromImageAsync(Stream imageStream)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var engine = new TesseractEngine(_tessDataPath, "por+eng", EngineMode.Default);
                    using var img = Pix.LoadFromMemory(StreamToByteArray(imageStream));
                    using var page = engine.Process(img);

                    var text = page.GetText();
                    var confidence = page.GetMeanConfidence();

                    Console.WriteLine($"[OCR] Confiança: {confidence:P}");

                    return text;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro no OCR: {ex.Message}", ex);
                }
            });
        }

        public async Task<List<string>> ExtractTextFromPdfAsync(Stream pdfStream)
        {
            return await Task.Run(() =>
            {
                var extractedTexts = new List<string>();

                try
                {
                    using var document = PdfDocument.Load(pdfStream);

                    for (int pageIndex = 0; pageIndex < document.PageCount; pageIndex++)
                    {
                        // Renderizar página como imagem
                        using var image = document.Render(pageIndex, 300, 300, PdfRenderFlags.CorrectFromDpi);

                        // Converter para stream
                        using var imageStream = new MemoryStream();
                        image.Save(imageStream, ImageFormat.Png);
                        imageStream.Position = 0;

                        // Executar OCR
                        var pageText = ExtractTextFromImageAsync(imageStream).Result;
                        extractedTexts.Add(pageText);

                        Console.WriteLine($"[OCR] Página {pageIndex + 1}/{document.PageCount} processada");
                    }

                    return extractedTexts;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao processar PDF com OCR: {ex.Message}", ex);
                }
            });
        }

        public async Task<bool> IsPdfScannedAsync(Stream pdfStream)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Tentar extrair texto normal do PDF
                    var reader = new iTextSharp.text.pdf.PdfReader(pdfStream);
                    var strategy = new iTextSharp.text.pdf.parser.SimpleTextExtractionStrategy();
                    var text = iTextSharp.text.pdf.parser.PdfTextExtractor.GetTextFromPage(reader, 1, strategy);
                    reader.Close();

                    // Se não tem texto ou tem muito pouco, é escaneado
                    var isScanned = string.IsNullOrWhiteSpace(text) || text.Trim().Length < 50;

                    Console.WriteLine($"[OCR] PDF é escaneado: {isScanned}");

                    return isScanned;
                }
                catch
                {
                    return true; // Se der erro, assume que é escaneado
                }
            });
        }

        private byte[] StreamToByteArray(Stream stream)
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}