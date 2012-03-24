using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Edit_Single
{
    public class ProgramControl
    {
        private UIControl uiControl = null;
        public ProgramControl(UIControl uic)
        {
            this.uiControl = uic;
        }

        private String compileInfo = "";
        private int compile()
        {
            try
            {
                if (this.uiControl.fileControl.isChange || !this.uiControl.fileControl.hasName())
                {
                    if (this.uiControl.fileControl.SaveFile() != 0)
                    {
                        this.uiControl.outputPanel.Append("保存文件失败...\n");
                        return 1;
                    }
                }
                String p = this.uiControl.fileControl.getName();
                String processName = FileControl.CombineName(UIControl.path, this.uiControl.getDeploy().DLXAssemblerName);
                this.uiControl.outputPanel.Append("----------\tCall " + this.uiControl.getDeploy().DLXAssemblerName + "\t----------");
                int index = OutProcess.CallOutProcess(processName, p, new OutProcess.dReadLine(this.compileOutput));
                this.uiControl.outputPanel.Append(OutputParse.Parse(0, index, compileInfo));

                return index;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("compile : " + ex.ToString());
                this.uiControl.outputPanel.Append("调用" + this.uiControl.getDeploy().DLXAssemblerName + "失败!");
                return 2;
            }
        }
        private void compileOutput(string str)
        {
            this.compileInfo = str;
        }
        private String linkInfo = "";
        private int link()
        {
            try
            {
                String p = this.uiControl.fileControl.getName();
                String p1 = FileControl.ReplayExt(p, this.uiControl.getDeploy().LinkExt);
                String p2 = FileControl.ReplayExt(p, this.uiControl.getDeploy().BinExt);
                p = p1 + " " + p2;
                String processName = FileControl.CombineName(UIControl.path, this.uiControl.getDeploy().DLXLinkerName);
                this.uiControl.outputPanel.Append("----------\tCall " + this.uiControl.getDeploy().DLXLinkerName + "\t----------");
                int index = OutProcess.CallOutProcess(processName, p, new OutProcess.dReadLine(this.linkOutput));
                this.uiControl.outputPanel.Append(OutputParse.Parse(1, index, linkInfo));

                return index;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("link : " + ex.ToString());
                this.uiControl.outputPanel.Append("调用" + this.uiControl.getDeploy().DLXLinkerName + "失败!");
                return 2;
            }
        }
        private void linkOutput(string str)
        {
            this.linkInfo = str;
        }
        private void runProgram()
        {
            try
            {
                String processName = FileControl.CombineName(UIControl.path, this.uiControl.getDeploy().DLXSimulateName);
                String p = this.uiControl.fileControl.getName();
                String p1 = FileControl.ReplayExt(p, this.uiControl.getDeploy().BinExt);
                this.uiControl.outputPanel.Append("----------\tCall " + this.uiControl.getDeploy().DLXSimulateName + "\t----------");
                OutProcess.CallOutProcessSample(processName, p1);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("run : " + ex.ToString());
                this.uiControl.outputPanel.Append("调用" + this.uiControl.getDeploy().DLXSimulateName + "失败!");
            }
        }

        public void Compile()
        {
            this.uiControl.outputPanel.Clear();
            this.compile();
        }
        public int CompileLink()
        {
            this.uiControl.outputPanel.Clear();
            int index = compile();
            if (index == 0)
            {
                int index1 = link();
                if (index1 == 0)
                    this.uiControl.outputPanel.Append("----------\tSuccess!\t----------");
                return index1;
            }
            return index;
        }
        public void CompileLinkRun()
        {
            if (CompileLink() == 0)
            {
                this.runProgram();
            }
        }

        public void OpenMultiLinker()
        {
            try
            {
                String processName = FileControl.CombineName(UIControl.path, this.uiControl.getDeploy().DLXMultiLinkerName);
                this.uiControl.outputPanel.Append("----------\tCall " + this.uiControl.getDeploy().DLXMultiLinkerName + "\t----------");
                OutProcess.CallOutProcessSample(processName, "");
            }
            catch (Exception ex)
            {
                this.uiControl.outputPanel.Append("调用" + this.uiControl.getDeploy().DLXMultiLinkerName + "失败!");
            }
        }
    }


    public class OutputParse
    {
        public static String Parse(int pro, int ec, String str)
        {
            if (pro == 0 && ec == 0)
                return C0Parse(str);
            if (pro == 0 && ec != 0)
                return CN1Parse(str);
            if (pro == 1 && ec == 0)
                return L0Parse(str);
            if (pro == 1 && ec != 0)
                return LN1Parse(str);
            return "";
        }
        private static String C0Parse(String str)
        {
            String s1 = str.Substring(0, str.Length - 1);
            int in1 = s1.LastIndexOf('\n');
            String s2 = s1.Substring(in1 + 1);
            int in2 = s2.LastIndexOf('至');
            String s3 = s2.Substring(in2 + 1);
            Debug.WriteLine(s3);
            String s4 = s1.Substring(0, in1 + 1);
            s4 += "{F," + s3 + "}" + s2;
            return s4;
        }
        private static String CN1Parse(String str)
        {
            return str;
        }
        private static String L0Parse(String str)
        {
            int index1 = str.LastIndexOf('到') + 2;
            int index2 = str.LastIndexOf('\n');
            int len = index2 - index1;
            return "{F," + str.Substring(index1, len) + "}" + str;
        }
        private static String LN1Parse(String str)
        {
            return str;
        }
    }
}
