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
                    if (CommandParameter==null) CommandParameter = new WorkParameter(true);
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
        public WorkParameter()
        {

        }

        public WorkParameter(bool pInit)
        {
            if (!pInit) return;
            Command = "CmdName";
            StartDirectory = "";
            CmdMoveFile = new CmdMoveFile(true);
        }

        /// <summary>
        /// команда обработки
        /// </summary>
        public string Command;
        /// <summary>
        /// стартовый каталог запуска. Если не заполнен, то используется текущий каталог
        /// </summary>
        public string StartDirectory;
        public CmdMoveFile CmdMoveFile;
    }

    /// <summary>
    /// Команда CmdMoveFile
    /// Команды выполняет поиск файлов с указанным расширением MainExtension к текущем каталоге
    /// Исключаются файлы которые начинаются с цифры 
    /// К найденному списку файлов дополняется файлы которые также называются как в списке, но имеет другое указанное расширение
    /// Если эти файлы не находятся в указанном каталоге, то в текущем каталоге создается каталог и 
    /// Файлы из списка перемещаются в этот каталог
    /// Класс содержит параметры для Команды CmdMoveFile перемещения файлов в каталог
    /// </summary>
    public class CmdMoveFile
    {
        public CmdMoveFile() { }
        public CmdMoveFile(bool pInit)
        {
            if (!pInit) return;
            MainExtension = "*.flac";
            ExcludeFirsrDigit = true;
            AddExtension = "*.cue";
            TargetDirectory = "RIP";
        }
    
        public string MainExtension;
        public bool ExcludeFirsrDigit;
        public string AddExtension;
        public string TargetDirectory;
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
