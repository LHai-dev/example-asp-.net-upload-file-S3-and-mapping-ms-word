using System.Net;
using Amazon.S3;
using Amazon.S3.Model;

namespace WebApplication2.service.impl;

public class S3Service
{
    public static async Task<bool> DownloadObjectFromBucketAsync(
        IAmazonS3 client,
        string bucketName,
        string objectName,
        string filePath)
    {
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = objectName
        };

        using var response = await client.GetObjectAsync(request);

        try
        {
            await response.WriteResponseStreamToFileAsync($"{filePath}\\{objectName}", true, CancellationToken.None);
            return response.HttpStatusCode == HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Error saving {objectName}: {ex.Message}");
            return false;
        }
    }
}