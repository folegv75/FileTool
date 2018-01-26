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
                var Res = SaveSettings(pFilename, out string saveErrorMessage);
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
    /// Информация о пути файла
    /// </summary>
    public class FilenameInfo
    {
        /// <summary>
        /// Содержит только наименование файла
        /// </summary>
        public string Name;
        /// <summary>
        /// Содержит полный путь файла
        /// </summary>
        public string Fullname;
        /// <summary>
        /// Содержит имя файла без каталога и без расширения
        /// </summary>
        public string NameOnly;
        /// <summary>
        /// Содержит только расширение файла без точки
        /// </summary>
        public string Extension;
        /// <summary>
        /// Содержит только каталог файла
        /// </summary>
        public string Directory;
        /// <summary>
        /// Порядковый номер файла в каталоге
        /// </summary>
        public int Order;
        /// <summary>
        /// Целевой каталог, в который будет перемещен файл
        /// </summary>
        public string TargetDir;

        public List<FilenameInfo> LinkedFiles = new List<FilenameInfo>();

        public FilenameInfo(string pFilename)
        {
            Fullname = pFilename;
            int lastpos = pFilename.LastIndexOf('\\');
            Name = pFilename.Substring(lastpos + 1);
            Directory = pFilename.Remove(lastpos);
            lastpos = Name.LastIndexOf('.');
            NameOnly = Name.Remove(lastpos);
            Extension = Name.Substring(lastpos + 1);
        }

    }

    /// <summary>
    /// Команда CmdMoveFile - перемещение файлов в каталог
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
            ExcludeFirstDigit = true;
            AddExtension = "*.cue";
            TargetDirectory = "RIP";
        }
    
        public string MainExtension;
        public bool ExcludeFirstDigit;
        public string AddExtension;
        public string TargetDirectory;

        private string RootDir;
        private List<FilenameInfo> WorkList;
        /// <summary>
        /// Выполнить обработку файлов
        /// </summary>
        public void Process()
        {
            MakeWorkList();
            MakeAddList();
            CreateDir();
            MoveFile();
            //todo перенос файлов

        }

        /// <summary>
        /// Формирует список файлов для перемещения
        /// </summary>
        private void MakeWorkList()
        {
            if (string.IsNullOrWhiteSpace(Settings.CommandParameter.StartDirectory))
            {
                // Возвращает каталог без последней обратной черты
                RootDir = Directory.GetCurrentDirectory();
            }
            else
            {
                RootDir = Settings.CommandParameter.StartDirectory;
            }

            //Console.WriteLine(RootDir);
            // получает строки с полным именем файла
            var Files = Directory.EnumerateFiles(RootDir, MainExtension, SearchOption.AllDirectories);
            WorkList = new List<FilenameInfo>();
            foreach (var someFilename in Files)
            {
                var someInfo = new FilenameInfo(someFilename);
                if (Settings.CommandParameter.CmdMoveFile.ExcludeFirstDigit)
                {
                    if (char.IsDigit(someInfo.Name[0])) continue;
                    if (someInfo.Directory.EndsWith(Settings.CommandParameter.CmdMoveFile.TargetDirectory, StringComparison.OrdinalIgnoreCase)) continue;
                    WorkList.Add(someInfo);
                }

            }
            // Ищем каталоги в которых найдено более одного файла
            var S = from A in WorkList group A by A.Directory into G select new { Name = G.Key, Kolvo = G.Count() };
            foreach (var some in S)
            {
                if (some.Kolvo == 1) continue;
                var Many = from A in WorkList where A.Directory == some.Name select A;
                int Order = 1;
                foreach (var someMany in Many)
                {
                    someMany.Order = Order;
                    Order++;
                }
            }

        }

        /// <summary>
        /// Формирует список дополнительных файлов, которые будут перемещаться вместе с основным
        /// Поиск по указанному расширению.
        /// </summary>
        private void MakeAddList()
        {
            if (WorkList.Count() == 0) return;
            if (string.IsNullOrWhiteSpace(Settings.CommandParameter.CmdMoveFile.AddExtension)) return;
            foreach (var someFileInfo in WorkList)
            {
                var AddFiles = Directory.EnumerateFiles(someFileInfo.Directory, Settings.CommandParameter.CmdMoveFile.AddExtension, SearchOption.TopDirectoryOnly);
                foreach(var someFilename in AddFiles)
                {
                    var AddInfo = new FilenameInfo(someFilename);
                    if (AddInfo.NameOnly.StartsWith(someFileInfo.NameOnly,StringComparison.OrdinalIgnoreCase)) someFileInfo.LinkedFiles.Add(AddInfo);
                }
            }
        }

        /// <summary>
        /// Создание каталогов
        /// </summary>
        private void CreateDir()
        {
            foreach(var someInfo in WorkList)
            {
                string LeftDirectory;
                string RightDirectory;
                if (someInfo.Directory.EndsWith("\\")) LeftDirectory = someInfo.Directory.Remove(someInfo.Directory.Length - 1);
                else LeftDirectory = someInfo.Directory;

                if (Settings.CommandParameter.CmdMoveFile.TargetDirectory.StartsWith("\\"))
                    RightDirectory = Settings.CommandParameter.CmdMoveFile.TargetDirectory.Substring(1);
                else RightDirectory = Settings.CommandParameter.CmdMoveFile.TargetDirectory;
                string targetDir = LeftDirectory + "\\" + RightDirectory;
                if (someInfo.Order>0)
                {
                    RightDirectory = string.Format("CD{0:d}", someInfo.Order);
                    if (targetDir.EndsWith("\\")) targetDir += RightDirectory;
                    else targetDir +="\\" + RightDirectory;
                }
                someInfo.TargetDir = targetDir;
                Directory.CreateDirectory(targetDir);
            }
        }

        /// <summary>
        /// Перемещение файлов
        /// </summary>
        private void MoveFile()
        {
            foreach(var mainFile in WorkList)
            {
                File.Move(mainFile.Fullname,mainFile.TargetDir+"\\"+mainFile.Name);
                foreach(var someLinkFile in mainFile.LinkedFiles)
                {
                    File.Move(someLinkFile.Fullname, mainFile.TargetDir + "\\" + someLinkFile.Name);

                }

            }
        }
    }

    public class Worker
    {
        public void Run()
        {
            switch (Settings.CommandParameter.Command)
            {
                case "CmdMoveFile":
                    {
                        Settings.CommandParameter.CmdMoveFile.Process();
                        break;
                    }
            }

        }

    }
 
    class Program
    {
        static void Main(string[] args)

        {
            if (args.Count() < 1)
            {
                Console.WriteLine("Укажите в параметрах json файл с командами обработки");
                return;
            }
            if (!Settings.ReadSettings(args[0], out string ErrorMessage))
            {
                Console.WriteLine(ErrorMessage);
                return;
            }
            var w = new Worker();
            w.Run();
        }
    }
}
