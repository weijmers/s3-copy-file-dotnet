using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace functions;

public class Test
{
    public Test() { }

    public async Task Handler(object input, ILambdaContext context)
    {
        Console.WriteLine("Testing testing ...");
        await Task.CompletedTask;
    }
}
