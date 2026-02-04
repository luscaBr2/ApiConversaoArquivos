using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ApiConversaoArquivos.Services.Interfaces;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using System.Text;
using A = DocumentFormat.OpenXml.Drawing;

namespace ApiConversaoArquivos.Services.Implementations
{
    public class PptxConverterService : IFileConverterService
    {
        public async Task<JToken> ConvertToJsonAsync(Stream fileStream, string fileName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var slides = new List<Dictionary<string, object>>();
                    var fullText = new StringBuilder();

                    using (var presentationDocument = PresentationDocument.Open(fileStream, false))
                    {
                        var presentationPart = presentationDocument.PresentationPart;

                        if (presentationPart == null)
                        {
                            throw new Exception("Apresentação inválida ou vazia");
                        }

                        var presentation = presentationPart.Presentation;
                        var slideIdList = presentation.SlideIdList;

                        if (slideIdList == null)
                        {
                            throw new Exception("Apresentação sem slides");
                        }

                        int slideNumber = 1;
                        foreach (var slideId in slideIdList.ChildElements.OfType<SlideId>())
                        {
                            var slidePart = (SlidePart)presentationPart.GetPartById(slideId.RelationshipId!);
                            var slide = slidePart.Slide;

                            var slideData = new Dictionary<string, object>
                            {
                                { "slideNumber", slideNumber },
                                { "title", GetSlideTitle(slide) },
                                { "content", GetSlideText(slide) },
                                { "notes", GetSlideNotes(slidePart) },
                                { "hasContent", HasContent(slide) }
                            };

                            slides.Add(slideData);

                            // Adicionar ao texto completo
                            fullText.AppendLine($"=== Slide {slideNumber} ===");
                            fullText.AppendLine(slideData["title"]?.ToString());
                            fullText.AppendLine(slideData["content"]?.ToString());
                            fullText.AppendLine();

                            slideNumber++;
                        }
                    }

                    var resultObject = new
                    {
                        fileName = fileName,
                        fileType = "PowerPoint",
                        totalSlides = slides.Count,
                        slides = slides,
                        fullText = fullText.ToString()
                    };

                    var json = JsonConvert.SerializeObject(resultObject);
                    return JToken.Parse(json);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao processar PowerPoint: {ex.Message}", ex);
                }
            });
        }

        private string GetSlideTitle(Slide slide)
        {
            var title = slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>()
                .FirstOrDefault()?.Text ?? string.Empty;

            return title;
        }

        private string GetSlideText(Slide slide)
        {
            var textBuilder = new StringBuilder();

            var textElements = slide.Descendants<A.Text>();

            foreach (var textElement in textElements)
            {
                if (!string.IsNullOrWhiteSpace(textElement.Text))
                {
                    textBuilder.AppendLine(textElement.Text);
                }
            }

            return textBuilder.ToString().Trim();
        }

        private string GetSlideNotes(SlidePart slidePart)
        {
            var notesPart = slidePart.NotesSlidePart;

            if (notesPart == null)
            {
                return string.Empty;
            }

            var notesTextBuilder = new StringBuilder();
            var textElements = notesPart.NotesSlide.Descendants<A.Text>();

            foreach (var textElement in textElements)
            {
                if (!string.IsNullOrWhiteSpace(textElement.Text))
                {
                    notesTextBuilder.AppendLine(textElement.Text);
                }
            }

            return notesTextBuilder.ToString().Trim();
        }

        private bool HasContent(Slide slide)
        {
            return slide.Descendants<A.Text>().Any(t => !string.IsNullOrWhiteSpace(t.Text));
        }
    }
}