using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Mvc;
using OpenXmlPowerTools;

namespace WebApplication2.controller;

[Route("api/upload/[controller]")]
[ApiController]
[RequestSizeLimit(1_000_000)]
[RequestFormLimits(MultipartBodyLengthLimit = 1_000_000)]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public UploadController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    // POST api/upload
    [HttpPost]
    public async Task<IActionResult> Post([FromForm] Upload uploadFile)
    {
        var file = uploadFile.File;

        if (file == null || file.Length == 0)
            return BadRequest("No file selected");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) ||
            (extension != ".pdf" && extension != ".docx" && extension != ".jpg" && extension != ".jpeg"))
            return BadRequest("Invalid file type");

        // Generate a new file name with a GUID
        var newFileName = Path.GetRandomFileName() + extension;

        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        var filePath = Path.Combine(folderPath, newFileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var fileUrl = Url.Content($"http://localhost:5178/uploads/{newFileName}");
        return Ok(new { url = fileUrl });
    }

    [HttpPost("multiupload")]
    public async Task<IActionResult> UploadFiles(IList<IFormFile> files)
    {
        // Define and setup the directory where your files will be saved.
        var root = Path.Combine(_environment.WebRootPath, "uploads");
        if (!Directory.Exists(root)) Directory.CreateDirectory(root);

        foreach (var file in files)
        {
            var fileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);

            var filePath = Path.Combine(root, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }

        return Ok(new { message = "Files uploaded successfully." });
    }
    
    [HttpPost("html")]
    public async Task<IActionResult> UploadFilesDocx(IList<IFormFile> files)
    {
        // Define and setup the directory where your files will be saved.
        var root = Path.Combine(_environment.WebRootPath, "uploads");
        if (!Directory.Exists(root)) Directory.CreateDirectory(root);

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var filePath = Path.Combine(root, file.FileName);

                // Save the file to the specified directory.
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Convert DOCX to HTML after saving the file.
                ConvertDocxToHtml(filePath);
            }
        }

        return Ok(new { message = "Files uploaded and converted successfully." });
    }

    private void ConvertDocxToHtml(string filePath)
    {
        byte[] byteArray = System.IO.File.ReadAllBytes(filePath);
        using (MemoryStream memoryStream = new MemoryStream())
        {
            memoryStream.Write(byteArray, 0, byteArray.Length);
            using (WordprocessingDocument doc = WordprocessingDocument.Open(memoryStream, true))
            {
                HtmlConverterSettings settings = new HtmlConverterSettings()
                {
                    PageTitle = "My Page Title"
                };
                XElement html = HtmlConverter.ConvertToHtml(doc, settings);

                // Add custom font CSS to the HTML
                string customFontCss = @"
                    <style>
                            @font-face {
                              font-family: 'khmer-mptc';
                              src: url(../fonts/KHMERMPTC.OTF) format('opentype');
                              font-size: normal;
                            }
                        body {
                            font-family: 'khmer-mptc';
                        }
                    </style>";
                html.AddFirst(XElement.Parse(customFontCss));

                // Save the HTML content to a file.
                var htmlFilePath = Path.ChangeExtension(filePath, ".html");
                System.IO.File.WriteAllText(htmlFilePath, html.ToStringNewLineOnAttributes());
            }
        }
    }

}