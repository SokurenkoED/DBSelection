using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DBSelection
{
    class Program
    {

        static string ConvertDataFormat(string OldFormat, IFormatProvider formatter)
        {
            string[] SplitStr = OldFormat.Trim().Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            return (double.Parse(SplitStr[0], formatter)*3600 + double.Parse(SplitStr[1], formatter) * 60 + double.Parse(SplitStr[2], formatter)).ToString();
        }

        static async Task Main(string[] args)
        {

            #region Настроечные данные

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding ANSI = Encoding.GetEncoding(1251);

            #endregion

            #region Ввод данных с клавиатуры

            Console.WriteLine("Введите имя датчика.");
            string SensorName = Console.ReadLine();
            //Console.WriteLine("Введите имя файла.");
            //string FileName = Console.ReadLine();
            Console.WriteLine($"Введите время начала выборки (чч:мм:сс).");
            string TempTimeFrom = Console.ReadLine().Trim();
            string TimeFrom = ConvertDataFormat(TempTimeFrom, formatter);
            Console.WriteLine($"Введите время конца выборки (чч:мм:сс).");
            string TimeTo = ConvertDataFormat(Console.ReadLine(), formatter);

            #endregion

            #region Setup

            Dictionary<double, string> FileData = new Dictionary<double, string>();
            double Date;
            string[] example;
            string[] DateArr;
            string[] DateDayArr;
            string RuteName = SensorName.Substring(2, 3);
            string RelatePath = "./06-09_07_2018";
            string[] filePaths = Directory.GetFiles(RelatePath);

            #endregion

            #region Запись данных в массив

            foreach (string item in filePaths)
            {
                string filename = Path.GetFileName(item);
                if (filename.IndexOf(RuteName) != -1)
                {
                    using (StreamReader sr = new StreamReader($"{RelatePath}/{filename}", ANSI))
                    {
                        Console.WriteLine($"\nИдет поиск в файле {filename}.");

                        string line;
                        while ((line = await sr.ReadLineAsync()) != null)
                        {
                            if (String.Compare(line.Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries)[2], SensorName) == 0)
                            {

                                #region Обработка данных

                                example = line.Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries);
                                DateArr = example[1].Trim().Replace(",", ".").Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                                DateDayArr = example[0].Trim().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                                Date = double.Parse(DateArr[0], formatter) * 3600 + double.Parse(DateArr[1], formatter) * 60 + double.Parse(DateArr[2], formatter) + (double.Parse(DateDayArr[0], formatter) - 6) * 24 * 3600;

                                #endregion

                                FileData.Add(Date, example[3]);
                            }
                        }
                    }
                }
            }

            #endregion

            #region Запись массива в файл

            using (StreamWriter sw = new StreamWriter($"{SensorName}_{TempTimeFrom.Replace(":", "-")}.dat", false, System.Text.Encoding.Default))
            {
                await sw.WriteLineAsync($"Time {SensorName}");
                string LastValue = null;
                int CountNods = 0;
                foreach (var item in FileData)
                {
                    if (item.Key >= double.Parse(TimeFrom, formatter) && item.Key <= double.Parse(TimeTo, formatter))
                    {
                        LastValue = item.Value;
                        if (CountNods == 0)
                        {
                            await sw.WriteLineAsync($"{double.Parse(TimeFrom, formatter) - double.Parse(TimeFrom, formatter)} {item.Value}");
                            await sw.WriteLineAsync($"{item.Key - double.Parse(TimeFrom, formatter)} {item.Value}");
                        }
                        else
                        {
                            await sw.WriteLineAsync($"{item.Key - double.Parse(TimeFrom, formatter)} {item.Value}");
                        }
                        CountNods++;
                    }
                    else if (item.Key > double.Parse(TimeTo, formatter))
                    {
                        break; // Так как больше совпадений не будет
                    }
                }
                await sw.WriteLineAsync($"{TimeTo} {LastValue}");
            }

            #endregion

            Console.WriteLine($"\nГотово!");

        }
    }
}
