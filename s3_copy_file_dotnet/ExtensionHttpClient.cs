using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace s3_copy_file_dotnet;

internal class ExtensionHttpClient
{
    private static HttpClient httpClient;
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
            Timeout = Timeout.InfiniteTimeSpan,
        };
    }

    public async Task<string?> Register()
    {
        var content = new StringContent(JsonSerializer.Serialize(new { events = new[] { "INVOKE", "SHUTDOWN" } }), Encoding.UTF8, "application/json");
        content.Headers.Add("Lambda-Extension-Name", ExtensionName);

        var response = await httpClient.PostAsync($"{baseUrl}/register", content);

        return response.Headers.GetValues("Lambda-Extension-Identifier").FirstOrDefault();
    }

    public async Task<(string? type, string payload)> Next(string id)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/event/next");
        request.Headers.Add("Lambda-Extension-Identifier", id);

        var response = await httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseContent);
        return (doc.RootElement.GetProperty("eventType").GetString(), responseContent);
    }
}
