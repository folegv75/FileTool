using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTool
{
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
