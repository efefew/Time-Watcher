using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace TimeWatcher
{
    internal class Class
    {
        #region Private Fields

        private const int MillisecondsTimeout = 5000;

        #endregion Private Fields

        #region Private Methods

        private static void Main()
        {
            ComputerPowerOnWatcher watcher = new ComputerPowerOnWatcher();
            while (true)
                Thread.Sleep(MillisecondsTimeout);
        }

        #endregion Private Methods
    }

    /// <summary>
    /// Расширения
    /// </summary>
    public static class Extentions
    {
        #region Public Methods

        /// <summary>
        /// Включает ли массив значение value
        /// </summary>
        /// <param name="array">массив</param>
        /// <param name="value">значение</param>
        /// <returns>Включает ли?</returns>
        public static bool Include(this string[] array, string value)
        {
            for (int id = 0; id < array.Length; id++)
                if (value == array[id])
                    return true;
            return false;
        }

        #endregion Public Methods
    }

    /// <summary>
    /// Запись включения и выключения компъютера
    /// </summary>
    public class ComputerPowerOnWatcher
    {
        #region Private Fields

        private const string AUTORUN_PATH = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        private const int COUNT_CHARS_EMERGENCY_EXITED = 36;

        private readonly string APPLICATION_NAME = Assembly.GetEntryAssembly().GetName().Name;

        private readonly string APPLICATION_PATH = Assembly.GetEntryAssembly().Location;

        private readonly string FILE_PATH = AppDomain.CurrentDomain.BaseDirectory + ' ' + DateTime.Now.Year + '.' + DateTime.Now.Month + ".txt";

        private string saveText;

        #endregion Private Fields

        #region Public Constructors

        public ComputerPowerOnWatcher()
        {
            AddAutoLoad(true);
            CreateFileTimeWatcher();
            SystemEvents.SessionEnded += new SessionEndedEventHandler(OnShutdown);
        }

        #endregion Public Constructors

        #region Private Methods

        /// <summary>
        /// Создание файла с записью времени включения и выключения компъютера
        /// </summary>
        private void CreateFileTimeWatcher()
        {
            FileStream file = new FileStream(FILE_PATH, FileMode.OpenOrCreate);
            file.Close();
            string lastText = File.ReadAllText(FILE_PATH);
            string addText = $"Входил: {DateTime.Now}\r\n";
            string tempText = "Аварийно выходил: время неизвестно\r\n";//на случай выключения света
            saveText = lastText + addText + tempText;
            File.WriteAllText(FILE_PATH, saveText);
            saveText = saveText.Remove(saveText.Length - COUNT_CHARS_EMERGENCY_EXITED, COUNT_CHARS_EMERGENCY_EXITED);
        }

        /// <summary>
        /// Момент выключения компа
        /// </summary>
        private void OnShutdown(object sender, SessionEndedEventArgs e)
        {
            File.WriteAllText(FILE_PATH, saveText + $"Выходил: {DateTime.Now}\r\n");
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Подключение автозагрузки
        /// </summary>
        /// <param name="autorun">подключить?</param>
        public void AddAutoLoad(bool autorun)
        {
            using (RegistryKey registry = Registry.CurrentUser.OpenSubKey(AUTORUN_PATH, true))
            {
                if (autorun)
                {
                    if (!registry.GetValueNames().Include(APPLICATION_NAME))
                        registry.SetValue(APPLICATION_NAME, $"\"{APPLICATION_PATH}\"");
                }
                else
                    registry.DeleteValue(APPLICATION_NAME, false);
            }
        }

        #endregion Public Methods
    }
}