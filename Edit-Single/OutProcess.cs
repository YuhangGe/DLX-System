using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows;

namespace Edit_Single
{
    class OutProcess
    {
        public delegate void dReadLine(string strLine);
        public static int CallOutProcess(string strFile, string args, dReadLine onReadLine)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = new System.Diagnostics.ProcessStartInfo();
            p.StartInfo.FileName = strFile;
            p.StartInfo.Arguments = args;
            p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            System.IO.StreamReader reader = p.StandardOutput;//截取输出流
            //  p.WaitForExit();
            p.WaitForExit();
            string line = reader.ReadToEnd();
            onReadLine.Invoke(line);
            return p.ExitCode;
        }
        public static void CallOutProcessSample(string strFile, string args)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = new System.Diagnostics.ProcessStartInfo();
            p.StartInfo.FileName = strFile;
            p.StartInfo.Arguments = args;
            Debug.WriteLine(strFile + " " + args);
            p.Start();
        }

        public static void CallExplorer(String path)
        {
            try
            {
                OutProcess.CallOutProcessSample("explorer.exe", path);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Open Explorer.exe Error!");
            }
        }
    }
}
