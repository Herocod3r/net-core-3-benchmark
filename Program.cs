using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace NetCoreSumFiles
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<FileSummerBenchmarker>();
        }
    }

    [MemoryDiagnoser]
    public class FileSummerBenchmarker
    {
        private readonly FileSummer fileSummer = new FileSummer();
        [Benchmark]
        public async Task FileSum()
        {
            await fileSummer.StartProcess();
        }
    }



    public class FileSummer
    {
        private long TotalSum;
        private static readonly string RootDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public  async Task StartProcess()
        {
            
            var tasks = new Task[1000];
            for (int i = 1; i < 1000; i+=10)
            {
                var currentRoot = $"{i.ToString().PadLeft(6,'0')}-{(i+9).ToString().PadLeft(6,'0')}";
                for (int j = i; j <= i+9; j++)
                {
                    tasks[j-1] = GenerateTask(Path.Combine(RootDir,"files",currentRoot,$"{j.ToString().PadLeft(6,'0')}.csv"));
                }
            }

            await Task.WhenAll(tasks);
            //Console.WriteLine(TotalSum);
        }

        private Task GenerateTask(string filename)=>Task.Run(() => ComputeFile(filename));

      
        private void ComputeFile(string fileName)
        {
            long totalSum = 0;
            var lastVal = -1;
            using (var reader = new StreamReader(fileName))
            {
                while (reader.Peek() >= 0)
                {
                    //var numbers = ReadNumbers(stream);
                    char c = (char) reader.Read();

                    if (!char.IsNumber(c))
                    {
                        totalSum += lastVal;
                        lastVal = -1;
                        continue;
                    }

                    var numVal = (int) char.GetNumericValue(c);
                    lastVal = lastVal < 0 ? numVal : Concatenate(lastVal, numVal);
                }
            }
            Interlocked.Add(ref TotalSum, totalSum);
        }
        public IEnumerable<int> ReadNumbers (TextReader reader)
        {
            var lastVal = -1;
            while (reader.Peek() >= 0)
            {
                char c = (char)reader.Read ();

                if (!char.IsNumber(c))
                {
                    yield return lastVal;
                    lastVal = -1;
                    continue;
                }

                var numVal = (int) char.GetNumericValue(c);
                lastVal = lastVal < 0 ? numVal : Concatenate(lastVal, numVal);
            }
        }
        
        
        private static int Concatenate(int x,int y)
        {
            var pow = 10;
            while(y>=pow) pow*=10;
            return x*pow+y;
        }
    }
}
