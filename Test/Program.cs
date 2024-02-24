using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Test
{
    internal class Class
    {
        #region Private Methods

        private static void Main()
        {
            TimeObserverAnalysis analysis = new TimeObserverAnalysis();
            analysis.ReadTimeFiles();
            Console.WriteLine("RatheStart " + analysis.RatheStart());
            Console.WriteLine("RatheEnd " + analysis.RatheEnd());
            Console.WriteLine("LateStart " + analysis.LateStart());
            Console.WriteLine("LateEnd " + analysis.LateEnd());
            Console.WriteLine("CountEmergencyExit " + analysis.CountEmergencyExit());
            Console.WriteLine("CountTimeInterval " + analysis.CountTimeInterval());
            Console.WriteLine("MaxTimeInterval " + analysis.MaxTimeInterval());
            Console.WriteLine("MinTimeInterval " + analysis.MinTimeInterval());
            Console.WriteLine("AverageTimeInterval " + analysis.AverageTimeInterval());
            _ = Console.ReadLine();
        }

        #endregion Private Methods
    }

    public static class Debug
    {
        #region Public Methods

        public static void Error(string message)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }

        public static void Log(string message) => Console.WriteLine(message);

        public static void Log() => Console.WriteLine();

        #endregion Public Methods
    }

    public static class Extentions
    {
        #region Public Methods

        public static int ToInt(this string text) => Convert.ToInt32(text);

        #endregion Public Methods
    }

    public class TimeObserverAnalysis
    {
        #region Private Fields

        private const string EMERGENCY_EXIT = "Аварийно выходил: время неизвестно\r";
        private readonly string FILE_DIRECTORY = AppDomain.CurrentDomain.BaseDirectory;
        private List<TimeInterval> intervals = new List<TimeInterval>();

        #endregion Private Fields

        #region Private Methods

        private void ReadTimeFile(string path)
        {
            if (!File.Exists(path))
                return;

            List<string> timeTags = File.ReadAllText(path).Split('\n').ToList();
            timeTags.RemoveAt(timeTags.Count - 1);//убираем пустышку

            if (timeTags.Count % 2 != 0)
            {
                Debug.Error($"Количество записей ({timeTags.Count}) не равно чётному числу");
                return;
            }

            for (int i = 0; i < timeTags.Count; i += 2)
            {
                if (timeTags[i + 1] != EMERGENCY_EXIT)
                    intervals.Add(new TimeInterval(ToDateTime(timeTags[i]), ToDateTime(timeTags[i + 1])));
                else
                    intervals.Add(new TimeInterval(ToDateTime(timeTags[i])));
            }
        }

        private DateTime ToDateTime(string tag)
        {
            tag = tag.Replace("Входил: ", "").Replace("Выходил: ", "");

            string[] time = tag.Split(' ');
            string[] date = time[0].Split('.');
            string[] clock = time[1].Split(':');
            return new DateTime(date[2].ToInt(), date[1].ToInt(), date[0].ToInt(),
                                clock[0].ToInt(), clock[1].ToInt(), clock[2].ToInt());
        }

        #endregion Private Methods

        #region Public Methods

        public TimeSpan AverageTimeInterval()
        {
            TimeSpan sum = TimeSpan.Zero;
            int countSucsesfullIntervals = intervals.Count - CountEmergencyExit();
            for (int i = 0; i < intervals.Count; i++)
            {
                if (!intervals[i].emergencyExit)
                    sum += intervals[i].Interval();
            }

            TimeSpan avg = new TimeSpan(sum.Ticks / countSucsesfullIntervals);
            return avg;
        }

        public int CountEmergencyExit()
        {
            int count = 0;
            for (int i = 0; i < intervals.Count; i++)
            {
                if (intervals[i].emergencyExit)
                    count++;
            }

            return count;
        }

        public int CountTimeInterval() => intervals.Count;

        /// <summary>
        /// Поздний конец
        /// </summary>
        /// <returns></returns>
        public DateTime LateEnd()
        {
            DateTime time = DateTime.Now;
            DateTime tempTime, time_;
            for (int i = 0; i < intervals.Count; i++)
            {
                if (intervals[i].emergencyExit)
                    continue;
                tempTime = intervals[i].end;
                time_ = new DateTime(tempTime.Year, tempTime.Month, tempTime.Day, time.Hour, time.Minute, time.Second);
                if (tempTime > time_)
                    time = tempTime;
            }

            return time;
        }

        /// <summary>
        /// Поздний старт
        /// </summary>
        /// <returns></returns>
        public DateTime LateStart()
        {
            DateTime time = intervals[0].start;
            DateTime tempTime, time_;
            for (int i = 0; i < intervals.Count; i++)
            {
                tempTime = intervals[i].start;
                time_ = new DateTime(tempTime.Year, tempTime.Month, tempTime.Day, time.Hour, time.Minute, time.Second);
                if (tempTime > time_)
                    time = tempTime;
            }

            return time;
        }

        public TimeSpan MaxTimeInterval()
        {
            TimeSpan span = TimeSpan.Zero;
            for (int i = 0; i < intervals.Count; i++)
            {
                if (intervals[i].Interval() > span)
                    span = intervals[i].Interval();
            }

            return span;
        }

        public TimeSpan MinTimeInterval()
        {
            TimeSpan span = TimeSpan.MaxValue;
            for (int i = 0; i < intervals.Count; i++)
            {
                if (!intervals[i].emergencyExit && intervals[i].Interval() < span)
                    span = intervals[i].Interval();
            }

            return span;
        }

        /// <summary>
        /// Раний конец
        /// </summary>
        /// <returns></returns>
        public DateTime RatheEnd()
        {
            DateTime time = DateTime.Now;
            DateTime tempTime, time_;
            for (int i = 0; i < intervals.Count; i++)
            {
                if (intervals[i].emergencyExit)
                    continue;
                tempTime = intervals[i].end;
                time_ = new DateTime(tempTime.Year, tempTime.Month, tempTime.Day, time.Hour, time.Minute, time.Second);
                if (tempTime < time_)
                    time = tempTime;
            }

            return time;
        }

        /// <summary>
        /// Раний старт
        /// </summary>
        /// <returns></returns>
        public DateTime RatheStart()
        {
            DateTime time = intervals[0].start;
            DateTime tempTime, time_;
            for (int i = 0; i < intervals.Count; i++)
            {
                tempTime = intervals[i].start;
                time_ = new DateTime(tempTime.Year, tempTime.Month, tempTime.Day, time.Hour, time.Minute, time.Second);
                if (tempTime < time_)
                    time = tempTime;
            }

            return time;
        }

        public void ReadTimeFiles()
        {
            intervals.Clear();
            DirectoryInfo directory = new DirectoryInfo(FILE_DIRECTORY);
            foreach (FileInfo file in directory.GetFiles())
            {
                if (file.Name.EndsWith(".txt"))
                    ReadTimeFile(file.FullName);
            }

            intervals.RemoveAt(intervals.Count - 1);
        }

        #endregion Public Methods
    }
}