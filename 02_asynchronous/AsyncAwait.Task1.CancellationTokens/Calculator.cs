using System.Threading;
using System.Threading.Tasks;

namespace AsyncAwait.Task1.CancellationTokens;

internal static class Calculator
{
    public static Task<long> Calculate(int n, CancellationToken token)
    {
        return Task.Run(() =>
        {
            long sum = 0;

            for (var i = 0; i <= n; i++)
            {
                token.ThrowIfCancellationRequested();
                sum += i;
                Thread.Sleep(100);
            }

            return sum;
        }, token);
    }
}