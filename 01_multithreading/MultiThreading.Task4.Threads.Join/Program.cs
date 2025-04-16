/*
 * 4.	Write a program which recursively creates 10 threads.
 * Each thread should be with the same body and receive a state with integer number, decrement it,
 * print and pass as a state into the newly created thread.
 * Use Thread class for this task and Join for waiting threads.
 * 
 * Implement all of the following options:
 * - a) Use Thread class for this task and Join for waiting threads.
 * - b) ThreadPool class for this task and Semaphore for waiting threads.
 */

using System;
using System.Threading;

namespace MultiThreading.Task4.Threads.Join
{
    class Program
    {
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

        static void Main()
        {
            CreateThreadsThroughJoin(10);
            CreateThreadsThroughSemaphore(10);

            Console.ReadLine();
        }

        private static void CreateThreadsThroughSemaphore(int leftToCreate)
        {
            if (leftToCreate <= 0)
                return;

            ThreadPool.QueueUserWorkItem(state =>
            {
                semaphore.Wait();
                LogLeft(leftToCreate);
                CreateThreadsThroughSemaphore(leftToCreate - 1);
                semaphore.Release();
            });
        }

        private static void CreateThreadsThroughJoin(int leftToCreate)
        {
            if (leftToCreate <= 0)
                return;

            Thread thread = new Thread(() =>
            {
                LogLeft(leftToCreate);
                CreateThreadsThroughJoin(leftToCreate - 1);
            });

            thread.Start();
            thread.Join();
        }

        private static void LogLeft(int leftToCreate)
        {
            Console.WriteLine($"Created new thread, left to create: {leftToCreate}");
        }
    }
}
