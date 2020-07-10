using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FileMetadataPerf
{
    class Program
    {
        private static string _test = "";
        private static string _runName = "";
        private static Random _random = new Random(1);

        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: [metadata | data] runName path [numberOfFiles numberOfDirectories | MBytes]");
                return;
            }

            _test = args[0];

            _runName = args[1];

            string path = args[2];

            int seq = 1;

            Console.WriteLine("\r\nFileMetadataPerformance [{0}]", _test + " " + _runName);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (_test == "metadata")
            {
                int numberOfFiles = 1;
                try { numberOfFiles = int.Parse(args[3]); }
                catch { }

                int numberOfDirectories = 1;
                try { numberOfDirectories = int.Parse(args[4]); }
                catch { }

                TimeCreateFiles(seq++, path, numberOfFiles);

                TimeFileExists(seq++, path, numberOfFiles, true);

                TimeFileExists(seq++, path, numberOfFiles, false);

                TimeDeleteFiles(seq++, path, numberOfFiles);

                TimeCreateDirectories(seq++, path, numberOfDirectories);

                TimeDirectoryExists(seq++, path, numberOfDirectories, true);

                TimeDirectoryExists(seq++, path, numberOfDirectories, false);

                TimeDeleteDirectories(seq++, path, numberOfDirectories);
            } else
            {
                int mbs = int.Parse(args[3]);
                
                TimeFileWrite(seq++, path, mbs);
                TimeFileRead(seq++, path, mbs);
            }
        }

        static void Log(int seq, string type, string path, int number, long milliseconds)
        {
            if (type.Contains("_End"))
            {
                string perf = "";
                if (_test == "metadata" && milliseconds != -1 && number != 0)
                    perf = ((decimal)milliseconds / number).ToString();
                else if (_test == "data" && milliseconds != -1)
                    perf = (1000 * number / (decimal)milliseconds).ToString();

                Console.WriteLine("{0}|{1}|{2}|{3}|{4}|{5}|{6}", _runName, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), seq.ToString().PadLeft(2, '0') + "_" + type, path, number, milliseconds != -1 ? milliseconds.ToString() : "", perf);
            }
        }

        static void TimeCreateFiles(int seq, string path, int numberOfFiles)
        {
            Log(seq, "TimeCreateFiles_Start", path, numberOfFiles, -1);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i=0; i<numberOfFiles; i++)
            {
                var streamWriter = File.CreateText(Path.Combine(path,"file" + i.ToString().PadLeft(6,'0')));
                streamWriter.Close();
            }
            stopwatch.Stop();
            Log(seq, "TimeCreateFiles_End", path, numberOfFiles, stopwatch.ElapsedMilliseconds);
        }

        static void TimeFileExists(int seq, string path, int numberOfFiles, bool existing)
        {
            Log(seq, "TimeFileExists" + existing + "_Start", path, numberOfFiles, -1);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < numberOfFiles; i++)
            {
                if (existing)
                    File.Exists(Path.Combine(path, "file" + i.ToString().PadLeft(6, '0')));
                else
                    File.Exists(Path.Combine(path, "file-notexisting" + i.ToString().PadLeft(6, '0')));
            }
            stopwatch.Stop();
            Log(seq, "TimeFileExists" + existing + "_End", path, numberOfFiles, stopwatch.ElapsedMilliseconds);
        }

        static void TimeDeleteFiles(int seq, string path, int numberOfFiles)
        {
            Log(seq, "TimeDeleteFiles_Start", path, numberOfFiles, -1);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < numberOfFiles; i++)
            {
                File.Delete(Path.Combine(path, "file" + i.ToString().PadLeft(6, '0')));
            }
            stopwatch.Stop();
            Log(seq, "TimeDeleteFiles_End", path, numberOfFiles, stopwatch.ElapsedMilliseconds);
        }

        static void TimeCreateDirectories(int seq, string path, int numberOfDirectories)
        {
            Log(seq, "TimeCreateDirectories_Start", path, numberOfDirectories, -1);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < numberOfDirectories; i++)
            {
                Directory.CreateDirectory(Path.Combine(path, "dir" + i.ToString().PadLeft(6, '0')));
            }
            stopwatch.Stop();
            Log(seq, "TimeCreateDirectories_End", path, numberOfDirectories, stopwatch.ElapsedMilliseconds);
        }

        static void TimeDirectoryExists(int seq, string path, int numberOfDirectories, bool existing)
        {
            Log(seq, "TimeDirectoryExists" + existing + "_Start", path, numberOfDirectories, -1);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < numberOfDirectories; i++)
            {
                if (existing)
                    Directory.Exists(Path.Combine(path, "dir" + i.ToString().PadLeft(6, '0')));
                else
                    Directory.Exists(Path.Combine(path, "dir-notexisting" + i.ToString().PadLeft(6, '0')));
            }
            stopwatch.Stop();
            Log(seq, "TimeDirectoryExists" + existing + "_End", path, numberOfDirectories, stopwatch.ElapsedMilliseconds);
        }

        static void TimeDeleteDirectories(int seq, string path, int numberOfDirectories)
        {
            Log(seq, "TimeDeleteDirectories_Start", path, numberOfDirectories, -1);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < numberOfDirectories; i++)
            {
                Directory.Delete(Path.Combine(path, "dir" + i.ToString().PadLeft(6, '0')));
            }
            stopwatch.Stop();
            Log(seq, "TimeDeleteDirectories_End", path, numberOfDirectories, stopwatch.ElapsedMilliseconds);
        }

        static void TimeFileWrite(int seq, string path, int mbs)
        {
            string randomString = RandomString(1024);
            Log(seq, "TimeFileWrite_Start", path, mbs, -1);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var streamWriter = File.CreateText(Path.Combine(path, "file" + mbs + ".dat"));
            for (int i = 0; i < mbs * 1024; i++)
            {
                streamWriter.WriteLine(randomString);
            }
            streamWriter.Close();
            stopwatch.Stop();
            Log(seq, "TimeFileWrite_End", path, mbs, stopwatch.ElapsedMilliseconds);
        }

        static void TimeFileRead(int seq, string path, int mbs)
        {
            int readBytes = 0;
            Log(seq, "TimeFileRead_Start", path, mbs, -1);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var streamReader = File.OpenText(Path.Combine(path, "file" + mbs + ".dat"));
            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                readBytes += line.Length;
            }
            streamReader.Close();
            stopwatch.Stop();
            Log(seq, "TimeFileRead_End", path, mbs, stopwatch.ElapsedMilliseconds);
        }

        static string RandomString(int length)
        {
            const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
            var builder = new StringBuilder();

            for (var i = 0; i < length; i++)
            {
                var c = pool[_random.Next(0, pool.Length)];
                builder.Append(c);
            }

            return builder.ToString();
        }
    }
}
