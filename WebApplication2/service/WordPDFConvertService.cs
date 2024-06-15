namespace WebApplication2.service;

public interface WordPDFConvertService
{
    Task<IEnumerable<string>> GenerateWords(IEnumerable<string> inputFilePaths);

    Task<string> GenerateWord();
}