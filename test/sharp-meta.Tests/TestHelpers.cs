using SharpMeta;
using Xunit.Abstractions;

namespace Tests;

internal static class TestHelpers
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