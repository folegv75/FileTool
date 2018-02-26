using CueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTool
{
    /// <summary>
    /// Определение фильтра файлов
    /// </summary>
    public class Filter
    {
        /// <summary>
        /// маска фильтра (расширение), по которому отбираем файлы
        /// </summary>
        public string Extension;
        public string StartDirectory;
        public bool DetectCueSheet;
        /// <summary>
        /// Признак, исключить файлы, которые начинаются с цифры
        /// </summary>
        public bool ExcludeFirstDigit;
        /// <summary>
        /// Исключить файл, если он находится в каталоге ExcludeParentDir 
        /// </summary>
        public string ExcludeParentDir;

        public Filter() { }
        public Filter(bool pInit)
        {
            if (!pInit) return;
            Extension = "*.flac";
            ExcludeFirstDigit = true;
            ExcludeParentDir = "RIP";
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
        /// Параметры отбора файлов для обработки
        /// </summary>
        public Filter Filter;
        /// <summary>
        /// Целевой каталог для выполнения действия (например, для действия MoveFile файлы перемещаются в каталог TargetDirectory
        /// </summary>
        public string TargetDirectory;
        /// <summary>
        /// Имя файла, в который будет выводится список файлов
        /// </summary>
        public string SaveFilename;
        /// <summary>
        /// Проверять ли состояние результаты проверки при выполнение команды
        /// </summary>
        public bool UseCheck;
        /// <summary>
        /// Содержит результат выполнения проверка
        /// </summary>
        public bool CheckValue;
        /// <summary>
        /// Просоединять ли доп файлы, если совпадает начало имени файла
        /// </summary>
        public bool JoinByName;
        /// <summary>
        /// Полное имя исходного файла, является источником данных
        /// </summary>
        public string SourceFilename;

        public FileAction() { }
        public FileAction(bool pInit)
        {
            if (!pInit) return;
            Command = "MoveFile||SaveToFile";
            Filter = new Filter(true);

            TargetDirectory = "RIP";
            SaveFilename = "filelist.txt"; 
        }
    }

    /// <summary>
    /// Информация о пути файла
    /// </summary>
    public class FilenameInfo
    {
        public bool IsDirectory;
        /// <summary>
        /// Содержит только наименование файла и расширение
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
        /// <summary>
        /// Содержит структуру cue файла
        /// </summary>
        public CueSheet CueSheet;
        /// <summary>
        /// Содержит результат выполнения алгоритма проверки
        /// </summary>
        public bool CheckValue;

        public List<FilenameInfo> LinkedFiles = new List<FilenameInfo>();

        public FilenameInfo(string pFilename, bool pIsDirectory)
        {
            IsDirectory = pIsDirectory;
            Fullname = pFilename;
            int lastpos = pFilename.LastIndexOf('\\');
            Name = pFilename.Substring(lastpos + 1);
            Directory = pFilename.Remove(lastpos);

            if (IsDirectory)
            {
                    NameOnly = Name;
                    Extension = "";            }
            else
            {
                lastpos = Name.LastIndexOf('.');
                if (lastpos >= 0)
                {
                    NameOnly = Name.Remove(lastpos);
                    Extension = Name.Substring(lastpos + 1);
                }
                else
                {
                    NameOnly = Name;
                    Extension = "";
                }
            }
        }

    }
}
