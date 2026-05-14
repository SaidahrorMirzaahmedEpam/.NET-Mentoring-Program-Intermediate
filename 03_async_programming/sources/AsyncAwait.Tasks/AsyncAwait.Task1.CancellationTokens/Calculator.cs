using System.Threading;

namespace AsyncAwait.Task1.CancellationTokens;

internal static class Calculator
{
    // todo: change this method to support cancellation token
    public static long Calculate(int n , CancellationToken token)
    {

        long sum = 0;

        for (var i = 0; i < n; i++)
        {
            token.ThrowIfCancellationRequested();

            sum = sum + (i + 1);
            Thread.Sleep(50);
        }

        return sum;
    }
}
