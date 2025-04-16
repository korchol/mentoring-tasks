/*
 * 2.	Write a program, which creates a chain of four Tasks.
 * First Task – creates an array of 10 random integers.
 * Second Task – multiplies this array with another random integer.
 * Third Task – sorts this array by ascending.
 * Fourth Task – calculates the average value. All this tasks should print the values to console.
 */
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MultiThreading.Task2.Chaining
{
    class Program
    {
        private static readonly Random random = new Random();

        public static void Main(string[] args)
        {
            var finalResult = Task.Run(() => CreateArray())
                .ContinueWith(antecedent => MultiplyArray(antecedent.Result))
                .ContinueWith(antecedent => SortArray(antecedent.Result))
                .ContinueWith(antecedent => CalculateAverage(antecedent.Result))
                .Result;

            Console.WriteLine($"All tasks completed with final result of: {finalResult}");
        }

        private static int[] CreateArray()
        {
            var array = Enumerable.Range(0, 10)
                                  .Select(_ => random.Next(1, 101))
                                  .ToArray();
            Console.WriteLine($"Created Array: {string.Join(", ", array)}");

            return array;
        }

        private static int[] MultiplyArray(int[] array)
        {
            var multiplier = random.Next(2, 10);
            Console.WriteLine($"Multiplier: {multiplier}");

            var multipliedArray = array.Select(x => x * multiplier).ToArray();
            Console.WriteLine($"Multiplied Array: {string.Join(", ", multipliedArray)}");

            return multipliedArray;
        }

        private static int[] SortArray(int[] array)
        {
            var sortedArray = array.OrderBy(x => x).ToArray();
            Console.WriteLine($"Sorted Array: {string.Join(", ", sortedArray)}");

            return sortedArray;
        }

        private static double CalculateAverage(int[] array)
        {
            var average = array.Average();
            Console.WriteLine($"Average: {average:F2}");

            return average;
        }
    }
}