/*
 * 5. Write a program which creates two threads and a shared collection:
 * the first one should add 10 elements into the collection and the second should print all elements
 * in the collection after each adding.
 * Use Thread, ThreadPool or Task classes for thread creation and any kind of synchronization constructions.
 */
using System;
using System.Collections.Generic;
using System.Threading;

namespace MultiThreading.Task5.Threads.SharedCollection
{
    class Program
    {
        const int ElementsNumber = 10;
        static readonly AutoResetEvent AddEvent = new(false);
        static readonly AutoResetEvent PrintEvent = new(false);

        static void Main()
        {
            var collection = new List<int>();

            var addingThread = new Thread(() => AddElements(collection));

            var printingThread = new Thread(() => PrintElements(collection));

            addingThread.Start();
            printingThread.Start();

            addingThread.Join();
            printingThread.Join();

            Console.WriteLine("Completed.");
        }
        private static void AddElements(List<int> collection)
        {
            for (int i = 0; i < ElementsNumber; i++)
            {
                collection.Add(i);
                Console.WriteLine($"Added element: {i}");
                PrintEvent.Set();
                AddEvent.WaitOne();
            }
        }

        private static void PrintElements(List<int> collection)
        {
            for (int i = 0; i < ElementsNumber; i++)
            {
                PrintEvent.WaitOne();
                Console.WriteLine($"Collection state: {string.Join(", ", collection)}");
                AddEvent.Set();
            }
        }
    }
}