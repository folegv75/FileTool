using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTool
{
    public static class ConstantValue
    {
        public static readonly string[] FileAction = { "CreateDir" ,"MoveFile", "SaveToFile" };
    }

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
                    if (CommandParameter == null) CommandParameter = new WorkParameter(true);
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
        /// стартовый каталог запуска. Если не заполнен, то используется текущий каталог
        /// </summary>
        public string StartDirectory;
        /// <summary>
        /// Параметры отбора файлов для обработки
        /// </summary>
        public Filter Filter;
        /// <summary>
        /// Действие обработки
        /// </summary>
        public List<FileAction> FileActionList;
        /// </summary>
        /// 

        public WorkParameter()
        {

        }

        /// <summary>
        /// конструктор для инициализации примеров значений
        /// </summary>
        /// <param name="pInit">true- инициализировать значения</param>
        public WorkParameter(bool pInit)
        {
            if (!pInit) return;
            StartDirectory = "";
            Filter = new Filter(true);
            FileActionList = new List<FileAction>();
            FileActionList.Add(new FileAction(true));
        }
    }

    /// <summary>
    /// Определение фильтра файлов
    /// </summary>
    public class Filter
    {
        /// <summary>
        /// Основная маска фильтра (расширение), по которому отбираем файлы
        /// </summary>
        public string MainExtension;
        /// <summary>
        /// Признак, исключить файлы, которые начинаются с цифры
        /// </summary>
        public bool ExcludeFirstDigit;
        /// <summary>
        /// Исключить файл, если он находится в каталоге ExcludeParentDir 
        /// </summary>
        public string ExcludeParentDir;
        /// <summary>
        /// Маска фильтра (расширение), файлы которые отбираются на равенство имени с основным списком
        /// </summary>
        public string AddExtension;

        public Filter() { }
        public Filter(bool pInit)
        {
            if (!pInit) return;
            MainExtension = "*.flac";
            ExcludeFirstDigit = true;
            ExcludeParentDir = "RIP";
            AddExtension = "*.cue";
        }
    }

    /// <summary>
    /// Определяет действие над выборкой файлов
    /// </summary>
    public class FileAction
    {
        /// <summary>
        /// команда обработки
        /// </summary>
        public string Command;
        /// <summary>
        /// Целевой каталог для выполнения действия (например, для действия MoveFile файлы перемещаются в каталог TargetDirectory
        /// </summary>
        public string TargetDirectory;
        /// <summary>
        /// Имя файла, в который будет выводится список файлов
        /// </summary>
        public string SaveFilename;

        public FileAction() { }
        public FileAction(bool pInit)
        {
            if (!pInit) return;
            Command = "MoveFile||SaveToFile";
            TargetDirectory = "RIP";
            SaveFilename = "filelist.txt";
        }
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
    public class Worker
    {
        private WorkParameter WorkParameter;
        private List<FilenameInfo> WorkList;

        public Worker(WorkParameter pWorkParameter)
        {
            WorkParameter = pWorkParameter;
        }


        private string StartRootDir;

        /// <summary>
        /// Выполнить обработку файлов
        /// </summary>
        public void Process()
        {
            if (string.IsNullOrWhiteSpace(Settings.CommandParameter.StartDirectory))
            {
                // Возвращает каталог без последней обратной черты
                StartRootDir = Directory.GetCurrentDirectory();
            }
            else
            {
                StartRootDir = WorkParameter.StartDirectory;
            }

            WorkList = MakeWorkList(WorkParameter.Filter, StartRootDir);
            MakeAddList(WorkParameter.Filter, WorkList);
            foreach(var someAction in WorkParameter.FileActionList)
            {
                switch (someAction.Command)
                {
                    case "CreateDir":
                        CreateDir(someAction, WorkList);
                        break;
                    case "MoveFile":
                        MoveFile(someAction, WorkList);
                        break;
                    case "SaveToFile":
                        SaveToFile(someAction, WorkList);
                        break;
                }
                qwerty
            }
            
        }

        private void SaveToFile(FileAction pFileAction, List<FilenameInfo> pWorkList)
        {

            using (StreamWriter sw = new StreamWriter(pFileAction.SaveFilename))
            {
                foreach (var someMainFile in pWorkList)
                {
                    sw.WriteLine(someMainFile.Fullname);
                    foreach(var linkFile in someMainFile.LinkedFiles)
                        sw.WriteLine("        "+linkFile.Fullname);
                }
            }
        }

        /// <summary>
        /// Формирует список файлов для перемещения
        /// </summary>
        private List<FilenameInfo> MakeWorkList(Filter pFilter, string RootDir)
        {

            // получает строки с полным именем файла
            var Files = Directory.EnumerateFiles(RootDir, pFilter.MainExtension, SearchOption.AllDirectories);
            var workList = new List<FilenameInfo>();
            foreach (var someFilename in Files)
            {
                var someInfo = new FilenameInfo(someFilename);
                if (pFilter.ExcludeFirstDigit)
                {
                    if (char.IsDigit(someInfo.Name[0])) continue;
                    if (someInfo.Directory.EndsWith(pFilter.ExcludeParentDir, StringComparison.OrdinalIgnoreCase)) continue;
                    workList.Add(someInfo);
                }

            }
            // Ищем каталоги в которых найдено более одного файла
            var S = from A in workList group A by A.Directory into G select new { Name = G.Key, Kolvo = G.Count() };
            foreach (var some in S)
            {
                if (some.Kolvo == 1) continue;
                var Many = from A in workList where A.Directory == some.Name select A;
                int Order = 1;
                foreach (var someMany in Many)
                {
                    someMany.Order = Order;
                    Order++;
                }
            }
            return workList;

        }

        /// <summary>
        /// Формирует список дополнительных файлов, которые будут перемещаться вместе с основным
        /// Поиск по указанному расширению.
        /// </summary>
        private void MakeAddList(Filter pFilter, List<FilenameInfo> pWorkList)
        {
            if (pWorkList.Count() == 0) return;
            if (string.IsNullOrWhiteSpace(pFilter.AddExtension)) return;
            foreach (var someFileInfo in pWorkList)
            {
                var AddFiles = Directory.EnumerateFiles(someFileInfo.Directory, pFilter.AddExtension, SearchOption.TopDirectoryOnly);
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
        private void CreateDir(FileAction pFileAction, List<FilenameInfo> pWorkList)
        {
            foreach(var someInfo in pWorkList)
            {
                string LeftDirectory;
                string RightDirectory;
                if (someInfo.Directory.EndsWith("\\")) LeftDirectory = someInfo.Directory.Remove(someInfo.Directory.Length - 1);
                else LeftDirectory = someInfo.Directory;

                if (pFileAction.TargetDirectory.StartsWith("\\"))
                    RightDirectory = pFileAction.TargetDirectory.Substring(1);
                else RightDirectory = pFileAction.TargetDirectory;
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
        private void MoveFile(FileAction pFileAction, List<FilenameInfo> pWorkList)
        {
            foreach(var mainFile in pWorkList)
            {
                File.Move(mainFile.Fullname,mainFile.TargetDir+"\\"+mainFile.Name);
                foreach(var someLinkFile in mainFile.LinkedFiles)
                {
                    File.Move(someLinkFile.Fullname, mainFile.TargetDir + "\\" + someLinkFile.Name);
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
            var w = new Worker(Settings.CommandParameter);
            w.Process();
        }
    }
}
