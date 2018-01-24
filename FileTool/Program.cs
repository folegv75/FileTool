using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTool
{
    public static class Settings
    {
        public static WorkParameter CommandParameter;
        public static bool ReadSettings(string pFilename, out string pErrorMessage)
        {
            if (!File.Exists(pFilename))
            {
                string saveErrorMessage;
                    var Res = SaveSettings(pFilename, out saveErrorMessage);
                if (Res)
                {
                    pErrorMessage = $"Файл не найден: {pFilename}. Файл создан.";
                    return false;
                }
                else
                {
                    pErrorMessage = $"Файл не найден: {pFilename} Ошибка создания {saveErrorMessage}";
                    return false;
                }

            }

            using (StreamReader stream = new StreamReader(pFilename))
            {
                try
                {
                    string Data = stream.ReadToEnd();
                    CommandParameter = JsonConvert.DeserializeObject<WorkParameter>(Data);
                }
                catch (Exception ex)
                {
                    pErrorMessage = "Ошибка чтения параметров:" + ex.ToString();
                    return false;
                }
            }
            pErrorMessage = null;
            return true;

        }

        public static bool SaveSettings(string pFilename, out string pErrorMessage)
        {

            using (StreamWriter stream = new StreamWriter(pFilename))
            {
                try
                {
                    string Data = JsonConvert.SerializeObject(CommandParameter, Formatting.Indented);
                    stream.Write(Data);
                }
                catch (Exception ex)
                {
                    pErrorMessage = "Error write settings:" + ex.ToString();
                    return false;
                }
            }
            pErrorMessage = null;
            return true;
        }

    }

    /// <summary>
    /// Содержит команды обработки
    /// </summary>
    public class WorkParameter
    {
        /// <summary>
        /// команда обработки
        /// </summary>
        public string Command;
        /// <summary>
        /// стартовый каталог запуска. Если не заполнен, то используется текущий каталог
        /// </summary>
        public string StartDirectory;
    }

    public class Worker
    {
        public void Run()
        {

        }

    }

    class Program
    {
        static void Main(string[] args)

        {
            string ErrorMessage;
            if (args.Count()<1)
            {
                Console.WriteLine("Укажите в параметрах json файл с командами обработки");
                return;
            }
            if (!Settings.ReadSettings(args[0], out ErrorMessage))
            {
                Console.WriteLine(ErrorMessage);
                return;
            }
            var w = new Worker();
            w.Run();
        }
    }
}
