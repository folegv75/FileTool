using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTool
{
    /// <summary>
    /// Содержит команды обработки
    /// </summary>
    public class WorkParameter
    {
        /// <summary>
        /// стартовый каталог запуска. Если не заполнен, то используется текущий каталог
        /// </summary>
        public string RootDirectory;
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
            RootDirectory = "";
            FileActionList = new List<FileAction>();
            FileActionList.Add(new FileAction(true));
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
            WorkList = new List<FilenameInfo>();
        }

        private string StartRootDir;

        /// <summary>
        /// Выполнить обработку файлов
        /// </summary>
        public void Process()
        {
            if (string.IsNullOrWhiteSpace(Settings.CommandParameter.RootDirectory))
            {
                // Возвращает каталог без последней обратной черты
                StartRootDir = Directory.GetCurrentDirectory();
            }
            else
            {
                StartRootDir = WorkParameter.RootDirectory;
            }

            foreach (var someAction in WorkParameter.FileActionList)
            {
                switch (someAction.Command)
                {
                    case "MakeWorkList":
                        MakeWorkList(someAction, WorkList);
                        break;
                    case "MakeAddList":
                        //todo
                        MakeAddList(someAction, WorkList);
                        break;
                    case "CreateDir":
                        CreateDir(someAction, WorkList);
                        break;
                    case "MoveFile":
                        MoveFile(someAction, WorkList);
                        break;
                    case "SaveToFile":
                        SaveToFile(someAction, WorkList);
                        break;
                    case "CheckExistCueTrack":
                        CheckExistCueTrack(someAction, WorkList);
                        break;
                }
            }

        }

        private void SaveToFile(FileAction pFileAction, List<FilenameInfo> pWorkList)
        {

            using (StreamWriter sw = new StreamWriter(pFileAction.SaveFilename))
            {
                foreach (var someMainFile in pWorkList)
                {
                    // проверим результат предыдущей проверки
                    if (pFileAction.UseCheck)
                        if (someMainFile.CheckValue != pFileAction.CheckValue) continue;
                    sw.WriteLine(someMainFile.Fullname);
                    foreach (var linkFile in someMainFile.LinkedFiles)
                        sw.WriteLine("        " + linkFile.Fullname);
                }
            }
        }

        /// <summary>
        /// Формирует список файлов для перемещения
        /// </summary>
        private void MakeWorkList(FileAction pFileAction, List<FilenameInfo> pWorkList)
        {
            var pFilter = pFileAction.Filter;
            // получает строки с полным именем файла
            var Files = Directory.EnumerateFiles(pFilter.StartDirectory, pFilter.Extension, SearchOption.AllDirectories);
            foreach (var someFilename in Files)
            {
                var someInfo = new FilenameInfo(someFilename);
                if (pFilter.ExcludeFirstDigit)
                    if (char.IsDigit(someInfo.Name[0])) continue;

                if (!string.IsNullOrWhiteSpace(pFilter.ExcludeParentDir))
                    if (someInfo.Directory.EndsWith(pFilter.ExcludeParentDir, StringComparison.OrdinalIgnoreCase)) continue;
                // добавить информацию cue, если расширение файла cue и влючение режим использования cue
                if (pFilter.DetectCueSheet)
                    if (string.Compare(someInfo.Extension, "cue", true) == 0)
                    {
                        try
                        {
                            someInfo.CueSheet = new CueSharp.CueSheet(someFilename);
                        }
                        catch
                        {
                            //
                        }
                    }
                pWorkList.Add(someInfo);

            }
            // Ищем каталоги в которых найдено более одного файла
            var S = from A in pWorkList group A by A.Directory into G select new { Name = G.Key, Kolvo = G.Count() };
            foreach (var some in S)
            {
                if (some.Kolvo == 1) continue;
                var Many = from A in pWorkList where A.Directory == some.Name select A;
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
        private void MakeAddList(FileAction pFileAction, List<FilenameInfo> pWorkList)
        {
            var pFilter = pFileAction.Filter;
            if (pWorkList.Count() == 0) return;
            if (string.IsNullOrWhiteSpace(pFilter.Extension)) return;
            foreach (var someFileInfo in pWorkList)
            {
                IEnumerable<string> AddFiles = null;
                try
                {
                    AddFiles = Directory.EnumerateFiles(someFileInfo.Directory + "\\" + pFilter.StartDirectory, pFilter.Extension, SearchOption.TopDirectoryOnly);
                }
                catch
                {
                }
                if (AddFiles!=null)
                foreach (var someFilename in AddFiles)
                {
                    var AddInfo = new FilenameInfo(someFilename);
                    if (pFileAction.JoinByName)
                    {
                        if (AddInfo.NameOnly.StartsWith(someFileInfo.NameOnly, StringComparison.OrdinalIgnoreCase)) someFileInfo.LinkedFiles.Add(AddInfo);
                    }
                    else someFileInfo.LinkedFiles.Add(AddInfo);

                }
            }
        }

        /// <summary>
        /// Создание каталогов
        /// </summary>
        private void CreateDir(FileAction pFileAction, List<FilenameInfo> pWorkList)
        {
            foreach (var someInfo in pWorkList)
            {
                string LeftDirectory;
                string RightDirectory;
                if (someInfo.Directory.EndsWith("\\")) LeftDirectory = someInfo.Directory.Remove(someInfo.Directory.Length - 1);
                else LeftDirectory = someInfo.Directory;

                if (pFileAction.TargetDirectory.StartsWith("\\"))
                    RightDirectory = pFileAction.TargetDirectory.Substring(1);
                else RightDirectory = pFileAction.TargetDirectory;
                string targetDir = LeftDirectory + "\\" + RightDirectory;
                if (someInfo.Order > 0)
                {
                    RightDirectory = string.Format("CD{0:d}", someInfo.Order);
                    if (targetDir.EndsWith("\\")) targetDir += RightDirectory;
                    else targetDir += "\\" + RightDirectory;
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
            foreach (var mainFile in pWorkList)
            {
                File.Move(mainFile.Fullname, mainFile.TargetDir + "\\" + mainFile.Name);
                foreach (var someLinkFile in mainFile.LinkedFiles)
                {
                    File.Move(someLinkFile.Fullname, mainFile.TargetDir + "\\" + someLinkFile.Name);
                }

            }
        }

        /// <summary>
        /// Переименовать каталог, в котором находится файл, если проверка была успешна
        /// </summary>
        /// <param name="pFileAction"></param>
        /// <param name="pWorkList"></param>
        private void RenameDir(FileAction pFileAction, List<FilenameInfo> pWorkList)
        {

        }

        /// <summary>
        /// Проверить наличие файлов указанные в cue файле
        /// </summary>
        /// <param name="pFileAction"></param>
        /// <param name="pWorkList"></param>
        private void CheckExistCueTrack(FileAction pFileAction, List<FilenameInfo> pWorkList)
        {
            foreach (var mainFile in pWorkList)
            {
                if (mainFile.CueSheet==null)
                {
                    mainFile.CheckValue = false;
                }
                foreach(var someTrack in mainFile.CueSheet.Tracks)
                {
                    mainFile.CheckValue = true;
                    string trackFilename = string.Format("{0:00}. {1}", someTrack.TrackNumber, someTrack.Title);
                    //TODO заменить недопутимые сивмволы cue файлов на "_". Недопустимые символы 
                    //  \ — разделитель подкаталогов
                    //  / — разделитель ключей командного интерпретатора
                    //  : — отделяет букву диска или имя альтернативного потока данных
                    //  * — заменяющий символ(маска «любое количество любых символов»)
                    //  ? — заменяющий символ(маска «один любой символ»)
                    //  " — используется для указания путей, содержащих пробелы
                    //  < — перенаправление ввода
                    //  > — перенаправление вывода
                    //  | — обозначает конвейер
                    trackFilename = trackFilename.Replace('/', '_');
                    trackFilename = trackFilename.Replace('\\', '_');
                    trackFilename = trackFilename.Replace(':', '_');
                    trackFilename = trackFilename.Replace('*', '_');
                    trackFilename = trackFilename.Replace('?', '_');
                    trackFilename = trackFilename.Replace('"', '_');
                    trackFilename = trackFilename.Replace('>', '_');
                    trackFilename = trackFilename.Replace('<', '_');
                    trackFilename = trackFilename.Replace('|', '_');
                    var Z = from a in mainFile.LinkedFiles where a.NameOnly == trackFilename select a;
                    // Найдем в подчиненных файлах, файл с именем равным trackFilename
                    // Не найдено совпадений установим флаг ошибки
                    if (Z.Count() == 0)
                    {
                        mainFile.CheckValue = false;
                        break;
                    }
                }
            }

        }

    }


}
