using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Maha.Spotted
{
    internal class DefaultLog : ILog, IDisposable
    {
        public string LogPath { get; set; }
        StreamWriter Writer;
        public void Init()
        {

            LogPath = Path.Combine(Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).AbsolutePath), "Files");
            Writer = new StreamWriter(Path.Combine(LogPath, DateTime.Now.ToShortDateString() + ".txt"), true);
            WriteText("时间    类型     数据", false);
        }

        public void WriteError(object obj)
        {
            if (obj == null) return;
            if (obj is Exception ex)
            {
                WriteText($"{DateTime.Now.ToShortTimeString()} Error {ex.Message}");
                return;
            }
            if (obj is string str)
            {
                WriteText($"{DateTime.Now.ToShortTimeString()} Error {str}");
                return;
            }
            WriteText($"{DateTime.Now.ToShortTimeString()} Error {obj.ToString()}");
        }
        private void WriteText(string text, bool isShow = true)
        {
            if (isShow)
                Console.WriteLine(text);
            Writer.WriteLineAsync(text);
        }
        public void WriteInfo(object obj)
        {
            if (obj == null) return;
            if (obj is Exception ex)
            {
                WriteText($"{DateTime.Now.ToShortTimeString()} Info {ex.Message}");
                return;
            }
            if (obj is string str)
            {
                WriteText($"{DateTime.Now.ToShortTimeString()} Info {str}");
                return;
            }
            WriteText($"{DateTime.Now.ToShortTimeString()} Info {obj.ToString()}");
        }

        public void Dispose()
        {
            WriteText("-------------------------------------------------",false);
        }
    }
}
