using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DBSelection
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding ANSI = Encoding.GetEncoding(1251);
            HashSet<string> FileData = new HashSet<string>();
            double Date;


            Console.WriteLine("Введите имя датчика.");
            string SensorName = Console.ReadLine();
            Console.WriteLine("Введите имя файла.");
            string FileName = Console.ReadLine();

            //string FileName = SensorName.Substring(2, 3);
            string FilePath = $"{@"06-09_07_2018\"}{FileName}{".txt"}";

            using (StreamReader sr = new StreamReader(FilePath, ANSI))
            {
                Console.WriteLine("Идет поиск.");
                using (StreamWriter sw = new StreamWriter("Report.txt", false, System.Text.Encoding.Default))
                {
                    sw.WriteLine("Time {0}", SensorName);
                
                    string line;
                    string[] example;
                    string[] DateArr;
                    string[] DateDayArr;
                    while ((line = await sr.ReadLineAsync()) != null)
                    {
                        if (String.Compare(line.Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries)[2], SensorName) == 0)
                        {
                            example = line.Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries);
                            DateArr = example[1].Trim().Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                            DateDayArr = example[0].Trim().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                            Date = double.Parse(DateArr[0]) * 3600 + double.Parse(DateArr[1]) * 60 + double.Parse(DateArr[2]) + (double.Parse(DateDayArr[0]) - 6)*24*3600;
                            await sw.WriteLineAsync($"{Date} {example[3]}");
                            //FileData.Add(line);
                        }
                    }
                }
            }
            Console.WriteLine("Готово!");

        }
    }
}
