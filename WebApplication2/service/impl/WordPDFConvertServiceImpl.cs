using System.Diagnostics;

namespace WebApplication2.service.impl;

public class WordPdfConvertServiceImpl : WordPDFConvertService
{
    private static readonly SemaphoreSlim Semaphore = new(20);

    private readonly string _inputFilePath =
        "C:\\Users\\Hai\\RiderProjects\\WebApplication2\\WebApplication2\\wwwroot\\uploads\\title.docx";

    private readonly string _officeToPdfExePaths = "./config/OfficeToPDF_1.exe";

    // Base directory for the output PDFs
    private readonly string _outputBaseDirectory =
        "C:\\Users\\Hai\\RiderProjects\\WebApplication2\\WebApplication2\\wwwroot\\uploads\\";


    public async Task<IEnumerable<string>> GenerateWords(IEnumerable<string> inputFilePaths)
    {
        var outputFilePaths = new List<string>();
        var tasks = new List<Task>();

        foreach (var inputFilePath in inputFilePaths)
        {
            tasks.Add(Task.Run(async () =>
            {
                var outputFilePath = await GenerateWord(inputFilePath);
                lock (outputFilePaths)
                {
                    outputFilePaths.Add(outputFilePath);
                }
            }));

            if (tasks.Count >= 5) // Batch size of 5
            {
                await Task.WhenAll(tasks);
                tasks.Clear();
            }
        }

        if (tasks.Count > 0) await Task.WhenAll(tasks);

        return outputFilePaths;
    }


    public async Task<string> GenerateWord()
    {
        var pdfGuid = Guid.NewGuid();
        var outputPdfPath = Path.Combine(_outputBaseDirectory, $"{pdfGuid}.pdf");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _officeToPdfExePaths,
                Arguments = $"\"{_inputFilePath}\" \"{outputPdfPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        try
        {
            await Task.Run(() =>
            {
                process.Start();
                process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                    throw new InvalidOperationException(
                        $"External process failed with exit code {process.ExitCode}: {error}");
            });

            return outputPdfPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            throw;
        }
    }

    private async Task<string> GenerateWord(string inputFilePath)
    {
        var outputFileName = $"output_{Guid.NewGuid()}.pdf"; // Unique output file name
        var outputPdfPath = Path.Combine(_outputBaseDirectory, outputFileName);


        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _officeToPdfExePaths,
                Arguments = $"\"{inputFilePath}\" \"{outputPdfPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        await Semaphore.WaitAsync();
        try
        {
            await Task.Run(() =>
            {
                process.Start();
                process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                    throw new InvalidOperationException(
                        $"External process failed with exit code {process.ExitCode}: {error}");
            });

            return outputPdfPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            throw;
        }
        finally
        {
            Semaphore.Release();
        }
    }
}