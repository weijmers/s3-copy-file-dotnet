using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace s3_copy_file_dotnet;

internal class ExtensionHttpClient
{
    private static readonly HttpClient httpClient;
    private static string baseUrl = $"http://{Environment.GetEnvironmentVariable("AWS_LAMBDA_RUNTIME_API")}/2020-01-01/extension";

    public string ExtensionName { get; private set; }

    public ExtensionHttpClient(string? extensionName)
    {
        if (string.IsNullOrEmpty(extensionName)) throw new ArgumentException(nameof(extensionName));

        ExtensionName = extensionName;
    }

    static ExtensionHttpClient()
    {
        httpClient = new HttpClient
        {
            BaseAddress = new UriBuilder(baseUrl).Uri,
            Timeout = Timeout.InfiniteTimeSpan,
        };
    }

    public async Task<string?> Register()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "register")
        {
            Content = new StringContent(JsonSerializer.Serialize(new { events = new[] { "INVOKE", "SHUTDOWN" } }), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Lambda-Extension-Name", "NAME");
        request.Headers.Add("Content-Type", "application/json");

        var response = await httpClient.SendAsync(request);
        return string.Join(", ", response.Headers.GetValues("lambda-extension-identifier"));
    }

    public async Task<(string? type, string payload)> Next(string id)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"next/{id}");
        request.Headers.Add("Lambda-Extension-Identifier", id);
        request.Headers.Add("Content-Type", "application/json");

        var response = await httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseContent);
        return (doc.RootElement.GetProperty("eventType").GetString(), responseContent);
    }
}
