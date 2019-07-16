using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metadata
{
   static class Logger
    {
        public static void Log(object message)
        {
            System.IO.File.AppendAllText(@".\Metadata.log", DateTime.Now + ":" + message + Environment.NewLine);
        }
    }
}
