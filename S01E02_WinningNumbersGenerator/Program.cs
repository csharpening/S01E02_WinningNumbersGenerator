using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace S01E02_WinningNumbersGenerator
{
    internal class Program
    {
        private const int NumberCount = 10;

        private static void Main(string[] args)
        {
            var startDate = new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var endDate = new DateTimeOffset(2050, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var rnd = new Random(3);

            var districts = 10000;

            using (var file = File.Open(@"LotteryResults.txt", FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            using (var writer = new StreamWriter(file))
            {
                object writerLock = new object();

                writer.WriteLine("# This is a tab-separated file with the following fields: date, district_number, winning_number_1, winning_number_2, winning_number_3, winning_number_4, winning_number_5, winning_number_6, winning_number_7, winning_number_8, winning_number_9, winning_number_10");

                for (var date = startDate; date < endDate; date = date.AddDays(1))
                {
                    byte[] buffer = new byte[NumberCount * districts];
                    rnd.NextBytes(buffer);

                    // Reduce numbers to 0-50 and introduce an intentional bias here.
                    for (var i = 0; i < buffer.Length; i++)
                        buffer[i] = (byte)(buffer[i] % 50);

                    Parallel.For(0, districts, districtNumber =>
                    {
                        var row = GenerateDataRow(date, districtNumber, buffer, districtNumber * NumberCount);

                        lock (writerLock)
                            writer.Write(row);
                    });

                    Console.WriteLine(date);
                }
            }
        }

        public static string GenerateDataRow(DateTimeOffset date, int districtNumber, byte[] buffer, int startIndex)
        {
            var writer = new StringBuilder(1000);

            writer.Append(date.ToString("d", CultureInfo.InvariantCulture));
            writer.Append('\t');
            writer.Append(districtNumber);

            for (var i = 0; i < NumberCount; i++)
            {
                writer.Append('\t');
                writer.Append(buffer[startIndex + i]);
            }

            writer.AppendLine();

            return writer.ToString();
        }
    }
}
