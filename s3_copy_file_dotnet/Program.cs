using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace s3_copy_file_dotnet;

class Program
{
    private static HttpClient? _client;
    private static string baseUrl = $"http://{Environment.GetEnvironmentVariable("AWS_LAMBDA_RUNTIME_API")}/2020-01-01/extension";

    private static HttpClient GetClient()
    {
        if (_client is null)
        {
            _client = new HttpClient();
            _client.BaseAddress = new UriBuilder(baseUrl).Uri;
            _client.Timeout = Timeout.InfiniteTimeSpan;
        }

        return _client;
    }

    static async Task Main(string[] args)
    {
        var extensionName = (1 == args.Length)
            ? args[0]
            : Assembly.GetEntryAssembly()?.GetName()?.Name;

        var client = new ExtensionHttpClient(extensionName);
        var extensionId = await client.Register() ?? throw new Exception("Register failure ...");

        // run your code ...

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
}