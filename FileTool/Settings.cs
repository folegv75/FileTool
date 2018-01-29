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
        public static readonly string[] FileAction = { "CreateDir", "MoveFile", "SaveToFile" };
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

}
