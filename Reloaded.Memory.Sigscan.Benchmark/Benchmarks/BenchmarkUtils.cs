using System;
using System.Collections.Generic;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks
{
    public static class BenchmarkUtils
    {
        // ReSharper disable once InconsistentNaming
        public static double BytesToMB(long bytes)
        {
            return bytes / 1000.0  // to kb
                         / 1000.0; // to mb
        }

        /// <summary>
        /// Creates a random array of specified size.
        /// </summary>
        /// <param name="size">Size of the array.</param>
        public static byte[] CreateRandomArray(int size)
        {
            var random = new Random(DateTime.Now.Millisecond * DateTime.Now.Second);
            var items  = new byte[size];
            random.NextBytes(items);
            return items;
        }

        /// <summary>
        /// Creates random patterns for a given input.
        /// </summary>
        /// <param name="numPatterns">Number of patterns to generate.</param>
        /// <param name="patternLength">Length of each pattern.</param>
        /// <param name="totalBytes">Number of total bytes that will be scanned.</param>
        /// <param name="source">Where to make patterns from.</param>
        /// <returns>Random patterns.</returns>
        public static List<string> CreateRandomPatterns(byte[] source, int numPatterns, int patternLength, out long totalBytes)
        {
            totalBytes = 0;
            var patterns = new List<string>();
            var random   = new Random(DateTime.Now.Millisecond * DateTime.Now.Second);

            // Pick out some random offsets
            for (int x = 0; x < numPatterns; x++)
            {
                var offset = random.Next(patternLength, source.Length - patternLength);
                totalBytes += offset;
                Console.WriteLine($"Offset: {offset}");
                patterns.Add(GeneratePattern(offset));
            }

            return patterns;

            string GeneratePattern(int offset)
            {
                var span = source.AsSpan(offset, patternLength);
                return BitConverter.ToString(span.ToArray()).Replace('-', ' ');
            }
        }
    }
}
