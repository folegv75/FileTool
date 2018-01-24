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
                pErrorMessage = $"Файл не найден: {pFilename}";
                return false;
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
