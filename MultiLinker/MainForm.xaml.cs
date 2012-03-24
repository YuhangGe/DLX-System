using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using Microsoft.Win32;

namespace MultiLinker
{
    /// <summary>
    /// MainForm.xaml 的交互逻辑
    /// </summary>
    public partial class MainForm : Window
    {
        private static string DLXAssemblerName = "DLXAssembler.exe";
        private static string DLXLinkerName = "DLXLinker.exe";
        private static string DLXSimulateName = "Simulate.exe";
        private static string DlxExt = ".dlx";
        private static string LinkExt = ".link";
        private static string BinExt = ".bin";
        private static string CurrentPath = AppDomain.CurrentDomain.BaseDirectory;

        public MainForm()
        {
            InitializeComponent();

            this.Init();
        }

        private void Init()
        {

            this.PathText.Text = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            this.NameText.Text = "noname.bin";

            this.CmdButton.Click += new RoutedEventHandler(CmdButton_Click);
            this.AddButton.Click += new RoutedEventHandler(AddButton_Click);
            this.SaveButton.Click += new RoutedEventHandler(SaveButton_Click);
            this.RunButton.Click += new RoutedEventHandler(RunButton_Click);
            this.BuildButton.Click += new RoutedEventHandler(BuildButton_Click);

            this.BuildItem.Click+=new RoutedEventHandler(BuildButton_Click);
            this.RunItem.Click+=new RoutedEventHandler(RunButton_Click);
            this.AddFileItem.Click+=new RoutedEventHandler(AddButton_Click);
            this.ExitItem.Click += new RoutedEventHandler(ExitItem_Click);
            this.RemoveAllItem.Click += new RoutedEventHandler(RemoveAllItem_Click);
            this.AboutItem.Click += new RoutedEventHandler(AboutItem_Click);

            this.NameText.LostFocus += new RoutedEventHandler(NameText_LostFocus);

            this.Drop += new DragEventHandler(MainForm_Drop);
            this.KeyDown += new KeyEventHandler(MainForm_KeyDown);

            this.CanBuild();
        }

        List<Key> keys = new List<Key>();
        void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            Key[] skeys = new Key[3];
            skeys[0] = Key.D2; skeys[1] = Key.D0; skeys[2] = Key.D9;
            keys.Add(e.Key);
            for (int i = 0; i < 3 && i < keys.Count; i++)
            {
                if (keys[i] != skeys[i])
                {
                    keys.Clear();
                    return;
                }
            }
            if (keys.Count != 3)
                return;
            this.keys.Clear();
            this.egg();
        }

        void MainForm_Drop(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData(DataFormats.FileDrop) is System.Array))
                return;
            System.Array array = (System.Array)e.Data.GetData(DataFormats.FileDrop);
            for (int i = 0; i < array.Length; i++)
            {
                if (array.GetValue(i) is string)
                {
                    this.AddItem((string)array.GetValue(i));
                }
            }
        }

        void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            AboutForm af = new AboutForm();
            af.Owner = this;
            af.ShowDialog();
        }

        void RemoveAllItem_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < this.ItemPanel.Children.Count; i++)
            {
                if (this.ItemPanel.Children[i] is FileItem)
                {
                    ((FileItem)this.ItemPanel.Children[i]).OnClose();
                    i--;
                }
            }
        }

        void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        void NameText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.NameText.Text == "")
                this.NameText.Text = "noname.bin";
        }

        void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            this.compilelink();
        }
        void RunButton_Click(object sender, RoutedEventArgs e)
        {
            this.compilelinkrun();
        }
        void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".bin";
            sfd.AddExtension = true;
            sfd.Filter = "DLX Bin Documents(*.bin)|*.bin|All Files(*.*)|*.*";
            sfd.Title = "Target Path";
            if (this.NameText.Text == "")
                sfd.FileName = "noname";
            else
                sfd.FileName = this.NameText.Text;
            if (sfd.ShowDialog(this) == true)
            {
                this.PathText.Text = ParsePath(sfd.FileName);
                this.NameText.Text = ParseName(sfd.FileName);
            }
        }
        void AddButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            string p1 = ("DLX Link Documents(*" + LinkExt + ")|*" + LinkExt + "");
            string p2 = "DLX Source Document(*" + DlxExt + ")|*" + DlxExt + "";
            string p3 = "All Files(*.*)|*.*";
            ofd.Filter = p2 + "|" + p1 + "|" + p3;
            ofd.Title = "Add File";
            ofd.Multiselect = true;
            if (ofd.ShowDialog(this) == true)
            {
                for (int i = 0; i < ofd.FileNames.Length; i++)
                    this.AddItem(ofd.FileNames[i]);
            }
        }
        void CmdButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                String str = Environment.CurrentDirectory;
                Environment.CurrentDirectory = MainForm.CurrentPath;
                OutProcess.CallOutProcessSample("cmd.exe", "");
                Environment.CurrentDirectory = str;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Calling cmd.exe failed");
                Debug.WriteLine(ex.ToString());
            }
        }

        public void AddItem(String str)
        {
            string filepath = ParsePath(str);
            string filename = ParseName(str);
            string ext = GetExt(str);
            FileItem f = new FileItem();
            if (ext.Equals(LinkExt))
                f.FileModel(filename, filepath, true, false);
            else
                if (ext.Equals(DlxExt))
                    f.FileModel(filename, filepath, false, true);
                else
                    f.FileModel(filename, filepath, false, false);
            f.CloseItem += new FileItem.CloseItemHandle(f_CloseItem);
            this.ItemPanel.Children.Add(f);
            this.outputPanel.Append("{F," + filepath + "}Add File : " + CombineName(filepath, filename));
            this.CanBuild();
        }
        void f_CloseItem(FileItem fi)
        {
            this.ItemPanel.Children.Remove(fi);
            this.outputPanel.Append("{F," + fi.FilePath + "}Remove File : " + CombineName(fi.FilePath, fi.FileName));
            this.CanBuild();
        }

        private void CanBuild()
        {
            this.NumberLebel.Content = "Files: " + this.ItemPanel.Children.Count + "";
            bool can = (this.ItemPanel.Children.Count > 0);
            this.RunButton.IsEnabled = can;
            this.BuildButton.IsEnabled = can;
            this.RunItem.IsEnabled = can;
            this.BuildItem.IsEnabled = can;
        }

        private void egg()
        {
            try
            {
                OutProcess.CallOutProcessSample(
                    MainForm.CombineName(MainForm.CurrentPath, "tkdz.exe"),
                    ""
                    );
            }
            catch (Exception ex)
            {
                MessageBox.Show("T_T 游戏找不到了！@o@");
            }
        }

        private int compile()
        {
            List<string> failure = new List<string>();
            this.outputPanel.Append("Assembling Start...");
            List<string> dlxs = CompileFiles();
            if (dlxs.Count == 0)
            {
                this.outputPanel.Append("\nAssembling Finish!");
                return 1;
            }
            for (int i = 0; i < dlxs.Count; i++)
            {
                this.outputPanel.Append("\nAssembling " + dlxs[i] + "...");
                int index = compile(dlxs[i]);
                if (index != 0)
                {
                    this.outputPanel.Append("Assembling " + dlxs[i] + " Failed!");
                    failure.Add(dlxs[i]);
                }
            }
            this.outputPanel.Append("\nAssembling Finish!\n");
            if (failure.Count != 0)
            {
                this.outputPanel.Append("Files Failed To Assemble (" + failure.Count + ") :");
                for (int i = 0; i < failure.Count; i++)
                    this.outputPanel.Append("{F," + MainForm.ParsePath(failure[i]) + "}\t" + failure[i] + "");
                this.outputPanel.Append("");
                return 2;
            }
            return 0;
        }
        private List<string> CompileFiles()
        {
            List<string> dlxfilenames = new List<string>();
            for (int i = 0; i < this.ItemPanel.Children.Count; i++)
            {
                FileItem fi = (FileItem)this.ItemPanel.Children[i];
                if (!fi.IsLink)
                    if (dlxfilenames.IndexOf(CombineName(fi.FilePath, fi.FileName)) == -1)
                        dlxfilenames.Add(CombineName(fi.FilePath, fi.FileName));
            }
            return dlxfilenames;
        }
        private String compileInfo = "";
        private int compile(String name)
        {
            try
            {
                String p = name;
                String processName = MainForm.CombineName(MainForm.CurrentPath, MainForm.DLXAssemblerName);
                this.outputPanel.Append("----------\tCall " + MainForm.DLXAssemblerName + "\t----------");
                int index = OutProcess.CallOutProcess(processName, p, new OutProcess.dReadLine(this.compileOutput));
                this.outputPanel.Append(OutputParse.Parse(0, index, compileInfo));

                return index;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("compile : " + ex.ToString());
                this.outputPanel.Append("调用" + MainForm.DLXAssemblerName + "失败!");
                return 2;
            }
        }
        private void compileOutput(string str)
        {
            this.compileInfo = str;
        }

        private int link()
        {
            this.outputPanel.Append("Linking Start...\n");
            int index = link(LinkFiles());
            this.outputPanel.Append("Linking Finish!");
            return index;
        }
        private List<string> LinkFiles()
        {
            List<string> dlxfilenames = this.CompileFiles();
            List<string> linkfilenames = new List<string>();
            for (int i = 0; i < dlxfilenames.Count; i++)
            {
                string str = ReplayExt(dlxfilenames[i], LinkExt);
                if (linkfilenames.IndexOf(str) == -1)
                    linkfilenames.Add(str);
            }
            for (int i = 0; i < this.ItemPanel.Children.Count; i++)
            {
                FileItem fi = (FileItem)this.ItemPanel.Children[i];
                if (fi.IsLink)
                {
                    string str = MainForm.CombineName(fi.FilePath, fi.FileName);
                    if (linkfilenames.IndexOf(str) == -1)
                        linkfilenames.Add(str);
                }
            }
            return linkfilenames;
        }
        private String linkInfo = "";
        private int link(List<string> fileNames)
        {
            try
            {
                if (fileNames.Count == 0)
                    return 1;
                String p = "";
                for (int i = 0; i < fileNames.Count; i++)
                    p += fileNames[i] + " ";
                p += MainForm.CombineName(this.PathText.Text, this.NameText.Text);
                Debug.WriteLine(p);
                String processName = MainForm.CombineName(MainForm.CurrentPath, MainForm.DLXLinkerName);
                this.outputPanel.Append("----------\tCall " + MainForm.DLXLinkerName + "\t----------");
                int index = OutProcess.CallOutProcess(processName, p, new OutProcess.dReadLine(this.linkOutput));
                this.outputPanel.Append(OutputParse.Parse(1, index, linkInfo));

                return index;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("link : " + ex.ToString());
                this.outputPanel.Append("调用" + MainForm.DLXLinkerName + "失败!");
                return 2;
            }
        }
        private void linkOutput(string str)
        {
            this.linkInfo = str;
        }

        private int compilelink()
        {
            this.UIable(false);
            this.outputPanel.Clear();
            int index = this.compile();
            if (index != 0)
            {
                this.outputPanel.Append("Any Errors in Assembling. Linking is Canceled!");
                this.UIable(true);
                return 1;
            }
            index = this.link();
            this.UIable(true);
            return index;
        }
        private void compilelinkrun()
        {
            if (this.compilelink() == 0)
            {
                try
                {
                    this.outputPanel.Append("");
                    String processName = MainForm.CombineName(MainForm.CurrentPath, MainForm.DLXSimulateName);
                    String p1 = MainForm.CombineName(this.PathText.Text, this.NameText.Text);
                    this.outputPanel.Append("----------\tCall " + MainForm.DLXSimulateName + "\t----------\n");
                    OutProcess.CallOutProcessSample(processName, p1);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("run : " + ex.ToString());
                    this.outputPanel.Append("调用" + MainForm.DLXSimulateName + "失败!");
                }
            }
        }

        private void UIable(bool b)
        {
            this.NameText.IsReadOnly = !b;
            this.PathText.IsReadOnly = !b;
            this.SaveButton.IsEnabled = b;
            this.AddButton.IsEnabled = b;
            this.RunButton.IsEnabled = b;
            this.BuildButton.IsEnabled = b;
            for (int i = 0; i < ItemPanel.Children.Count; i++)
                ((FileItem)ItemPanel.Children[i]).SetClose(b);
        }

        public static String ParsePath(String str)
        {
            String str1 = str;
            for (int i = 0; i < str1.Length - 1; i++)
            {
                if (str1[i] == '\\' && str1[i + 1] == '\\')
                    str1 = str1.Substring(0, i) + str1.Substring(i + 1);
            }
            int index = str1.LastIndexOf('\\');
            return str1.Substring(0, index + 1);
        }
        public static String ParseName(String str)
        {
            int index = str.LastIndexOf('\\');
            return str.Substring(index + 1);
        }
        public static String CombineName(String path, String name)
        {
            if (path[path.Length - 1] == '\\')
                return path + name;
            else
                return path + "\\" + name;
        }
        public static String ReplayExt(String name, String n)
        {
            int index = name.LastIndexOf('.');
            string ext = "";
            if (n.Length!=0 && n[0] == '.')
                ext = n;
            else
                ext = "." + n;
            if (index == -1)
                return name + ext;
            string st = name.Substring(0, index);
            return st + ext;
        }
        public static String GetExt(String name)
        {
            if (name.LastIndexOf('.') != -1)
                return (name.Substring(name.LastIndexOf('.'))).ToLower();
            else
                return ".";
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
