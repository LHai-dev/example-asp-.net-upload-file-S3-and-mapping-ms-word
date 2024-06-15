using Microsoft.AspNetCore.Mvc;
using WebApplication2.service;

namespace WebApplication2.controller;

[ApiController]
[Route("/api/v1/PDF-Convert/[controller]")]
public class WordPdfConvertController : ControllerBase
{
    private readonly WordPDFConvertService _convertService;

    public WordPdfConvertController(WordPDFConvertService convertService)
    {
        _convertService = convertService;
    }

    [HttpPost]
    public async Task<IActionResult> GenerateWordBatching(IEnumerable<string> filePaths)
    {
        try
        {
            var outputFilePaths = await _convertService.GenerateWords(filePaths);
            return Ok(outputFilePaths);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpPost("/GenerateWord")]
    public async Task<IActionResult> GenerateWord()
    {
        try
        {
            var outputFilePaths = await _convertService.GenerateWord();
            return Ok(outputFilePaths);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
    
    
}