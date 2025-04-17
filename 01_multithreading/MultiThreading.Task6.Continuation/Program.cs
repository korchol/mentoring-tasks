/*
*  Create a Task and attach continuations to it according to the following criteria:
   a.    Continuation task should be executed regardless of the result of the parent task.
   b.    Continuation task should be executed when the parent task finished without success.
   c.    Continuation task should be executed when the parent task would be finished with fail and parent task thread should be reused for continuation
   d.    Continuation task should be executed outside of the thread pool when the parent task would be cancelled
   Demonstrate the work of the each case with console utility.
*/
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreading.Task6.Continuation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Task parentTask = RunParentTask();
            AttachContinuations(parentTask);

            try
            {
                await parentTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static async Task RunParentTask()
        {
            int seconds = TryGetTimer();

            Console.WriteLine($"Timer started for {seconds} seconds.");
            Console.WriteLine("Press SPACE to cancel the timer.");

            using var cts = new CancellationTokenSource();

            await RunTimerWithCancellation(seconds, cts);
        }

        private static async Task RunTimerWithCancellation(int seconds, CancellationTokenSource cts)
        {
            Task timerTask = StartTimer(seconds, cts);
            Task cancelTask = ListenForCancelKeyPress(cts);

            await Task.WhenAll(timerTask, cancelTask);
        }

        private static Task StartTimer(int seconds, CancellationTokenSource cts)
        {
            return Task.Run(async () =>
            {
                cts.Token.ThrowIfCancellationRequested();

                for (int i = 1; i <= seconds; i++)
                {
                    Console.WriteLine($"Time elapsed: {i} second(s).");
                    await Task.Delay(1000, cts.Token);
                }

                Console.WriteLine("Timer completed successfully.");
                cts.Cancel();
                return;
            });
        }

        private static Task ListenForCancelKeyPress(CancellationTokenSource cts)
        {
            return Task.Run(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Spacebar)
                    {
                        Console.WriteLine("Timer canceled by user.");
                        cts.Cancel();
                        return;
                    }
                }
            });
        }

        private static int TryGetTimer()
        {
            Console.WriteLine("Enter the number of seconds for the timer (positive integer):");
            string input = Console.ReadLine();

            if (!int.TryParse(input, out int seconds) || seconds <= 0)
            {
                throw new ArgumentException("Timer duration must be a positive integer.");
            }

            return seconds;
        }

        private static void AttachContinuations(Task parentTask)
        {
            parentTask.ContinueWith(ExecuteContinuationRegardless);

            parentTask.ContinueWith(ExecuteContinuationOnFailure, TaskContinuationOptions.OnlyOnFaulted);

            parentTask.ContinueWith(ExecuteContinuationReusingParentThread,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

            parentTask.ContinueWith(ExecuteContinuationOutsideThreadPool,
                TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.LongRunning);
        }

        private static void ExecuteContinuationRegardless(Task parentTask)
        {
            Console.WriteLine("Continuation executed regardless of parent task result.");
        }

        private static void ExecuteContinuationOnFailure(Task parentTask)
        {
            Console.WriteLine("Continuation executed because parent task failed.");
        }

        private static void ExecuteContinuationReusingParentThread(Task parentTask)
        {
            Console.WriteLine("Continuation executed reusing parent thread because parent task failed.");
        }

        private static void ExecuteContinuationOutsideThreadPool(Task parentTask)
        {
            Console.WriteLine("Continuation executed outside thread pool because parent task was canceled.");
        }
    }
}