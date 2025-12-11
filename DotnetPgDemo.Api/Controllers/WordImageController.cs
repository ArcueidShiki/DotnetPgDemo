using Microsoft.AspNetCore.Mvc;
using SkiaSharp;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace DotnetPgDemo.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class WordImageController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public WordImageController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        /// <summary>
        /// Converts an image to base64, then to PNG, and inserts it into a Word document.
        /// </summary>
        /// <param name="imagePath">Optional path to the image file. Defaults to test.jpeg in project root.</param>
        /// <param name="title">Title to insert in the Word document.</param>
        /// <param name="width">Optional width for the output image.</param>
        /// <param name="height">Optional height for the output image.</param>
        /// <returns>The generated Word document.</returns>
        [HttpPost("process")]
        public async Task<IActionResult> ProcessImage(
            [FromQuery] string? imagePath = null,
            [FromQuery] string title = "Sample Title",
            [FromQuery] int? width = null,
            [FromQuery] int? height = null)
        {
            try
            {
                // Step 1: Get the image path (default to test.jpeg)
                var projectRoot = _environment.ContentRootPath;
                var sourceImagePath = string.IsNullOrEmpty(imagePath) 
                    ? Path.Combine(projectRoot, "test.jpeg") 
                    : imagePath;

                if (!System.IO.File.Exists(sourceImagePath))
                {
                    return NotFound($"Image file not found: {sourceImagePath}");
                }

                // Step 1: Convert image to base64 URL and save to txt file
                var imageBytes = await System.IO.File.ReadAllBytesAsync(sourceImagePath);
                var mimeType = GetMimeType(sourceImagePath);
                var base64String = Convert.ToBase64String(imageBytes);
                var base64Url = $"data:{mimeType};base64,{base64String}";

                // Save base64 URL to txt file
                var base64FilePath = Path.Combine(projectRoot, "image_base64.txt");
                await System.IO.File.WriteAllTextAsync(base64FilePath, base64Url);

                // Step 2: Convert base64 back to PNG with optional resize
                var pngBytes = ConvertBase64ToPng(base64String, width, height, out int finalWidth, out int finalHeight);

                // Step 3: Create Word document from template and insert image
                var templatePath = Path.Combine(projectRoot, "template.docx");
                var outputPath = Path.Combine(projectRoot, "output.docx");

                // Ensure template exists
                if (!System.IO.File.Exists(templatePath))
                {
                    CreateTemplate(templatePath);
                }

                // Process the template
                System.IO.File.Copy(templatePath, outputPath, true);
                ProcessWordTemplate(outputPath, pngBytes, title, finalWidth, finalHeight);

                // Return the generated document
                var outputBytes = await System.IO.File.ReadAllBytesAsync(outputPath);
                return File(outputBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "output.docx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing image: {ex.Message}");
            }
        }

        private static string GetMimeType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        private static byte[] ConvertBase64ToPng(string base64String, int? targetWidth, int? targetHeight, out int finalWidth, out int finalHeight)
        {
            var imageBytes = Convert.FromBase64String(base64String);
            
            using var inputStream = new MemoryStream(imageBytes);
            using var originalBitmap = SKBitmap.Decode(inputStream);

            // Determine final dimensions
            finalWidth = targetWidth ?? originalBitmap.Width;
            finalHeight = targetHeight ?? originalBitmap.Height;

            // Resize if needed
            SKBitmap outputBitmap;
            if (finalWidth != originalBitmap.Width || finalHeight != originalBitmap.Height)
            {
                outputBitmap = originalBitmap.Resize(new SKImageInfo(finalWidth, finalHeight), SKSamplingOptions.Default);
            }
            else
            {
                outputBitmap = originalBitmap;
            }

            // Convert to PNG
            using var image = SKImage.FromBitmap(outputBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            
            if (outputBitmap != originalBitmap)
            {
                outputBitmap.Dispose();
            }

            return data.ToArray();
        }

        private static void CreateTemplate(string templatePath)
        {
            using var document = WordprocessingDocument.Create(templatePath, WordprocessingDocumentType.Document);
            
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            // Add Title placeholder
            var titleParagraph = body.AppendChild(new Paragraph());
            var titleRun = titleParagraph.AppendChild(new Run());
            titleRun.AppendChild(new Text("Title: {{title}}"));

            // Add empty paragraph for spacing
            body.AppendChild(new Paragraph());

            // Add Image placeholder
            var imageParagraph = body.AppendChild(new Paragraph());
            var imageRun = imageParagraph.AppendChild(new Run());
            imageRun.AppendChild(new Text("Image: {{image}}"));

            mainPart.Document.Save();
        }

        private static void ProcessWordTemplate(string documentPath, byte[] imageBytes, string title, int imageWidth, int imageHeight)
        {
            using var document = WordprocessingDocument.Open(documentPath, true);
            var mainPart = document.MainDocumentPart;
            if (mainPart == null) return;

            var body = mainPart.Document?.Body;
            if (body == null) return;

            // Find and replace {{title}}
            foreach (var text in body.Descendants<Text>())
            {
                if (text.Text.Contains("{{title}}"))
                {
                    text.Text = text.Text.Replace("{{title}}", title);
                }
            }

            // Find and replace {{image}} with actual image
            foreach (var paragraph in body.Descendants<Paragraph>().ToList())
            {
                var fullText = string.Concat(paragraph.Descendants<Text>().Select(t => t.Text));
                
                if (fullText.Contains("{{image}}"))
                {
                    // Add image to document
                    var imagePart = mainPart.AddImagePart(ImagePartType.Png);
                    using (var stream = new MemoryStream(imageBytes))
                    {
                        imagePart.FeedData(stream);
                    }

                    var relationshipId = mainPart.GetIdOfPart(imagePart);

                    // Create image element
                    var imageElement = CreateImageElement(relationshipId, imageWidth, imageHeight);

                    // Clear the paragraph and add the image
                    paragraph.RemoveAllChildren<Run>();
                    
                    // Add "Image: " text
                    var textRun = new Run(new Text("Image: "));
                    paragraph.AppendChild(textRun);
                    
                    // Add the image
                    var imageRun = new Run(imageElement);
                    paragraph.AppendChild(imageRun);
                }
            }

            mainPart.Document?.Save();
        }

        private static Drawing CreateImageElement(string relationshipId, int pixelWidth, int pixelHeight)
        {
            // Convert pixels to EMUs (English Metric Units)
            // 1 inch = 914400 EMUs, 1 inch = 96 pixels (at 96 DPI)
            const long emusPerPixel = 914400 / 96;
            long widthEmus = pixelWidth * emusPerPixel;
            long heightEmus = pixelHeight * emusPerPixel;

            var element = new Drawing(
                new DW.Inline(
                    new DW.Extent() { Cx = widthEmus, Cy = heightEmus },
                    new DW.EffectExtent()
                    {
                        LeftEdge = 0L,
                        TopEdge = 0L,
                        RightEdge = 0L,
                        BottomEdge = 0L
                    },
                    new DW.DocProperties()
                    {
                        Id = 1U,
                        Name = "Picture 1"
                    },
                    new DW.NonVisualGraphicFrameDrawingProperties(
                        new A.GraphicFrameLocks() { NoChangeAspect = true }),
                    new A.Graphic(
                        new A.GraphicData(
                            new PIC.Picture(
                                new PIC.NonVisualPictureProperties(
                                    new PIC.NonVisualDrawingProperties()
                                    {
                                        Id = 0U,
                                        Name = "image.png"
                                    },
                                    new PIC.NonVisualPictureDrawingProperties()),
                                new PIC.BlipFill(
                                    new A.Blip()
                                    {
                                        Embed = relationshipId,
                                        CompressionState = A.BlipCompressionValues.Print
                                    },
                                    new A.Stretch(
                                        new A.FillRectangle())),
                                new PIC.ShapeProperties(
                                    new A.Transform2D(
                                        new A.Offset() { X = 0L, Y = 0L },
                                        new A.Extents() { Cx = widthEmus, Cy = heightEmus }),
                                    new A.PresetGeometry(
                                        new A.AdjustValueList()
                                    )
                                    { Preset = A.ShapeTypeValues.Rectangle }))
                        )
                        { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                )
                {
                    DistanceFromTop = 0U,
                    DistanceFromBottom = 0U,
                    DistanceFromLeft = 0U,
                    DistanceFromRight = 0U
                });

            return element;
        }
    }
}
