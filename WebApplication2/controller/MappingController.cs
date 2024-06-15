using Microsoft.AspNetCore.Mvc;
using MiniSoftware;

namespace WebApplication2.controller;

[ApiController]
[Route("/api/v1/mapping")]
public class MappingController : Controller
{
    [HttpPost]
    public async Task<IActionResult> GenerateWord([FromBody] TemplateData templateData)
    {
        try
        {
            var outputPathDocx =
                "C:\\Users\\Hai\\RiderProjects\\WebApplication2\\WebApplication2\\wwwroot\\uploads\\title.docx";
            var inputDocx =
                "C:\\Users\\Hai\\RiderProjects\\WebApplication2\\WebApplication2\\wwwroot\\uploads\\Leave_Request_Template.docx";

            await Task.Run(() => MiniWord.SaveAsByTemplate(outputPathDocx, inputDocx, templateData.Values));

            Console.WriteLine("Document... Converted!");
            Console.WriteLine("Word to PDF conversion successful!");

            return Ok(); // Return a meaningful success response
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Unauthorized access: {ex.Message}");
            return Unauthorized(); // Return a meaningful error response
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return StatusCode(500); // Return a meaningful error response
        }
    }
}