using Xunit.Abstractions;

namespace SharpMeta.Tests;

internal static class TestOutputHelperExtensions
{
    public static SharpResolverLogger ToSharpResolverLogger(this ITestOutputHelper outputHelper)
    {
        return new SharpResolverLogger
        {
            OnInfo = outputHelper.WriteLine,
            OnWarning = outputHelper.WriteLine,
            OnError = outputHelper.WriteLine
        };
    }
}
