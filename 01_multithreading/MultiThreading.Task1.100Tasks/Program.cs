/*
 * 1.	Write a program, which creates an array of 100 Tasks, runs them and waits all of them are not finished.
 * Each Task should iterate from 1 to 1000 and print into the console the following string:
 * “Task #0 – {iteration number}”.
 */
using System;
using System.Threading.Tasks;

namespace MultiThreading.Task1._100Tasks
{
    class Program
    {
        const int TaskAmount = 100;
        const int MaxIterationsCount = 1000;

        public static void Main()
        {
            HundredTasks();
            Console.ReadLine();
        }

        private static void HundredTasks()
        {
            var tasks = new Task[TaskAmount];
            for (int t = 0; t < TaskAmount; t++)
            {
                int taskNumber = t;
                tasks[taskNumber] = Task.Run(() => ExecuteTask(taskNumber));
            }

            Task.WaitAll(tasks);
            Console.WriteLine("Done");
        }

        private static void ExecuteTask(int taskNumber)
        {
            for (int iterationNumber = 0; iterationNumber < MaxIterationsCount; iterationNumber++)
            {
                Output(taskNumber, iterationNumber);
            }
        }

        private static void Output(int taskNumber, int i)
        {
            Console.WriteLine($"Task #{taskNumber + 1} – {i + 1}");
        }
    }
}
