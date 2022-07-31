using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

#region Пояснения по коду
/* 
программа реализующая анализ файлов, класс FileReader
основные вычисления происходят в функции WorkWithDirectory(), которая принимает на вход директории
в финальном варианте данная функция должна выполняться асинхронно - это нужно для того, чтобы программа могла
выводить статус работы анализа во время проведения самого анализа
функция WorkWithDirectory() принимает помимо директории лист. Это нужно было для того чтобы не передавать много 
переменных в функцию
*/
#endregion
namespace analuzepath
{
    class Program
    {
        class FileReader
        {
            public string Path_name { get; set; }
            public int Processed_files { get; set; }
            public int JS_detects { get; set; }
            public int Rm_Rf_detects { get; set; }

            public int Rundll32_detects { get; set; }
            public int Errors { get; set; }
            public string Execution_time { get; set; }

            public FileReader(string dirName)
            {
                List<int> tmp = new List<int>() { 0, 0, 0, 0, 0 };
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                WorkWithDirectory(dirName, tmp);
                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                Processed_files = tmp[3];
                Errors = tmp[4];
                JS_detects = tmp[2];
                Rm_Rf_detects = tmp[1];
                Rundll32_detects = tmp[0];
                Execution_time = elapsedTime;
                Path_name = dirName;
            }
            public void PrintInfo()
            {
                Console.WriteLine("Directory: {0}", Path_name);
                Console.WriteLine("Processed files: {0}", Processed_files);
                Console.WriteLine("JS detects: {0}", JS_detects);
                Console.WriteLine("Rm_Rf_detects: {0}", Rm_Rf_detects);
                Console.WriteLine("Rundll32_detects: {0}", Rundll32_detects);
                Console.WriteLine("Execution_time: {0}", Execution_time);
                Console.WriteLine("Errors: {0}", Errors);
                Console.WriteLine("=========================");
            }
        }
        static void WorkWithDirectory(string dirName, List<int> tmp)
        {
            if (Directory.Exists(dirName))
            {
                string[] dirs = Directory.GetDirectories(dirName);
                if (dirs.Length > 0)
                {
                    foreach (string s in dirs)
                    {
                        WorkWithDirectory(s, tmp); //Рекурсия нужна для просмотра подкаталогов в каталоге
                    }
                }
                string[] files = Directory.GetFiles(dirName);                     
                Parallel.ForEach(files, s =>
                {
                    tmp[3]++;
                    try
                    {
                        using (StreamReader reader = new StreamReader(s))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                if (line.Contains("Rundll32 sus.dll SusEntry"))
                                {
                                    tmp[0]++;
                                    continue;
                                }
                                if (line.Contains("rm -rf " + dirName))
                                {
                                    tmp[1]++;
                                    continue;
                                }
                                if (line.Contains("<script>evil_script()</script>") && Path.GetExtension(s) == ".js")
                                {
                                    tmp[2]++;
                                    continue;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        tmp[4]++;
                    }
                });
            }
        }
        static void Main(string[] args)
        {
            string dirName = " "; //enter your path
            FileReader fileReader = new FileReader(dirName);
            fileReader.PrintInfo();
        }
    }
}
