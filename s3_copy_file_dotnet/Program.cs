using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

namespace s3_copy_file_dotnet;

class Program
{
    static async Task Main(string[] args)
    {
        var extensionName = args[0];
        var client = new ExtensionHttpClient(extensionName);
        var extensionId = await client.Register() ?? throw new Exception("Register failure ...");

        await CopyFile();

        while (true)
        {
            var (type, payload) = await client.Next(extensionId);
            switch (type)
            {
                case "SHUTDOWN":
                    await HandleShutdown(payload);
                    break;
                case "INVOKE":
                    await HandleInvoke(payload);
                    break;
                default:
                    throw new Exception("woop woop");
            }
        }
    }

    private static async Task HandleInvoke(string payload)
    {
        await Task.CompletedTask;
    }

    private static async Task HandleShutdown(string payload)
    {
        Console.WriteLine("SHUTDOWN");
        Console.WriteLine(payload);
        await Task.CompletedTask;

        Environment.Exit(0);
    }

    private static async Task CopyFile()
    {
        var client = new AmazonS3Client();
        var request = new GetObjectRequest
        {
            BucketName = Environment.GetEnvironmentVariable("S3_BUCKET"),
            Key = Environment.GetEnvironmentVariable("S3_KEY"),
        };
        var filePath = Path.Combine("/tmp", Environment.GetEnvironmentVariable("S3_KEY"));

        using var response = await client.GetObjectAsync(request);
        using var fileStream = File.Create(filePath);
        await response.ResponseStream.CopyToAsync(fileStream);
    }
}