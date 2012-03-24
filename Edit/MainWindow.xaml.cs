using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Diagnostics;
using Microsoft.Win32;
using System.Collections;
using Abraham;
using System.Collections.Generic;


namespace Edit
{
    public partial class MainWindow
    {
        private bool _OutputOrError;
        private bool OutputOrError
        {
            get
            {
                return _OutputOrError;
            }
            set
            {
                this._OutputOrError = value;
                if (value)
                {
                    this.OutputGrid.Visibility = Visibility.Visible;
                    this.ErrorGrid.Visibility = Visibility.Hidden;
                    this.OutputButton.IsEnabled = false;
                    this.ErrorButton.IsEnabled = true;
                    this.OutputSign.Content = "Output";
                }
                else
                {
                    this.OutputGrid.Visibility = Visibility.Hidden;
                    this.ErrorGrid.Visibility = Visibility.Visible;
                    this.OutputButton.IsEnabled = true;
                    this.ErrorButton.IsEnabled = false;
                    this.OutputSign.Content = "Error";
                }

            }
        }
        Hashtable IndexToFile = new Hashtable();
        Hashtable FileToIndex = new Hashtable();
        List<OpenedFileStruction> openedFiles = new List<OpenedFileStruction>();
        private string HighLightPath = Environment.CurrentDirectory;
        public MainWindow()
        {
            this.InitializeComponent();
            Debug.WriteLine(Environment.CurrentDirectory);
            this.init();
            // 在此点之下插入创建对象所需的代码。
        }
        private void init()
        {
            this.OutputOrError = true;

            this.ProjectNameCanvas.SizeChanged += new SizeChangedEventHandler(ProjectNameCanvas_SizeChanged);
            this.OutputButton.Click += new RoutedEventHandler(OutputButton_Click);
            this.ErrorButton.Click += new RoutedEventHandler(ErrorButton_Click);
            this.NewSingleProjectButton.Click += new RoutedEventHandler(NewSingleProjectButton_Click);
            this.TreeViewer.FileDoubleClick += new TreePanel.eventdelegate(TreeViewer_FileDoubleClick);
            this.OpenFileButton.Click += new RoutedEventHandler(OpenFileButton_Click);
            this.SaveFileButton.Click += new RoutedEventHandler(SaveFileButton_Click);
            this.SaveFilesButton.Click += new RoutedEventHandler(SaveFilesButton_Click);
            this.CompileButton.Click += new RoutedEventHandler(CompileButton_Click);
            this.CompileLinkButton.Click += new RoutedEventHandler(CompileLinkButton_Click);
            this.CompileLinkRunButton.Click += new RoutedEventHandler(CompileLinkRunButton_Click);
            
        }
        void CompileLinkRunButton_Click(object sender, RoutedEventArgs e)
        {
            this.OutputText.Clear();
            if (this.compiler())
            {
                this.Output(Environment.NewLine);
                if(this.linker())
                    CallSimulate();
            }            
        }

        void CompileLinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.OutputText.Clear();
            if (this.compiler())
            {
                this.Output(Environment.NewLine);
                this.linker();
            }
        }

        void CompileButton_Click(object sender, RoutedEventArgs e)
        {
            this.OutputText.Clear();
            compiler();
        }


        void SaveFilesButton_Click(object sender, RoutedEventArgs e)
        {
            this.StoreAllFile();
        }

        void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            this.StoreDlxFile(this.CodeTabControl.SelectedItem());
        }

        void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Dlx Project(*.dlx,*.dlxp)|*.dlx;*.dlxp";
            ofd.Title = "Open Dlx Project";
            if (ofd.ShowDialog() == true)
            {
                if (!IsAllFileSave())
                    return;
                FileInfo file = new FileInfo(ofd.FileName);
                if (file.Extension.ToUpper().Equals(".DLX"))
                    this.OpenSingleProject(file.FullName, file.Name);
                else
                {
                }
            }
        }

        void TreeViewer_FileDoubleClick(object sender, object[] args)
        {
            if ((bool)args[2])
            {
                FileInfo f = new FileInfo((string)args[1]);
                this.OpenDlxFile(f.FullName, f.Name);
            }
        }
        void NewSingleProjectButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Dlx Code File(.dlx)|*.dlx";
            sfd.Title = "New Single Project";
            if (sfd.ShowDialog() == true)
            {
                if (!IsAllFileSave())
                    return;
                if (sfd.FileName != "")
                {
                    System.IO.FileStream fs =
                            (System.IO.FileStream)sfd.OpenFile();
                    fs.Close();
                    OpenSingleProject(sfd.FileName, sfd.SafeFileName);
                }
            }
        }
        void ErrorButton_Click(object sender, RoutedEventArgs e)
        {
            this.OutputOrError = false;
        }
        void OutputButton_Click(object sender, RoutedEventArgs e)
        {
            this.OutputOrError = true;
        }


        void ProjectNameCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ProjectName.Width = this.ProjectNameCanvas.ActualWidth;
        }

        public bool OpenSingleProject(string Path, string Name)
        {
            String fn = Path;
            String name = Name;
            String path = fn.Substring(0, fn.Length - name.Length);
            this.TreeViewer.setDirectories(path, name, true);
            this.SetProgramName(name);
            try
            {
                this.OpenDlxFile(fn, name);
            }
            catch
            {
                Debug.Write(path + ": Open Error!");
                return false;
            }
            return true;
        }

        public bool IsAllFileSave()
        {
            CloseProject();
            return true;
        }
        public void SetProgramName(String program)
        {
            this.ProjectName.Content = "Project-" + program;
        }
        public void CloseProject()
        {
            this.ProjectName.Content = "Project-none";
            this.TreeViewer.CloseProject();
            this.CodeTabControl.CloseAll();
        }
        public void StoreAllFile()
        {
            for (int i = 0; i < this.CodeTabControl.allItems.Count; i++)
                if (this.CodeTabControl.allItems[i].content is DLXEditor)
                    StoreDlxFile(this.CodeTabControl.allItems[i]);
        }

        public bool OpenDlxFile(String path, String name)
        {
            if (!FileToIndex.ContainsKey(path))
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                }
                catch (Exception e) { Debug.WriteLine(path + ":Stream Error!"); return false; }

                DlxTabItem d = new DlxTabItem();
                d.header = name;
                d.CloseClick += new DlxTabItem.eventdelegate(d_CloseClick);
                int i = this.CodeTabControl.AddItem(d);
                this.IndexToFile.Add(i, path);
                this.FileToIndex.Add(path, i);
                DLXEditor edit = new DLXEditor();
                edit.HighlightFile = this.HighLightPath+"\\Highlight.xshd";
                edit.FontSize = 20;
                d.content = edit;
                edit.DocumentFormatted += new DLXEditor.DocumentFormattedEventHandler(edit_DocumentFormatted);
                edit.Text = OpenedFileStruction.ReadFile(fs);
                OpenedFileStruction fsn = new OpenedFileStruction(path, name, false, edit);
                fsn.IsChangeChange += new OpenedFileStruction.eventdelegate(fsn_IsChangeChange);
                this.openedFiles.Add(fsn);
                edit.DocumentChanged += new DLXEditor.DocumentChangedEventHandler(edit_DocumentChanged);
                return true;
            }
            else
            {
                this.CodeTabControl.ActiveIndex((int)this.FileToIndex[path]);
                return true;
            }
        }

        void fsn_IsChangeChange(object sender, object[] args)
        {
            try
            {
                bool a1 = (bool)args[0];
                bool a2 = (bool)args[1];
                OpenedFileStruction ofs = (OpenedFileStruction)sender;
                int index = (int)this.FileToIndex[ofs.path];
                if (a1 != a2)
                {
                    if (a1)
                        this.CodeTabControl.GetItem(index).header += '*';
                    else
                    {
                        String s = this.CodeTabControl.GetItem(index).header;
                        s = s.Substring(0, s.Length - 1);
                        this.CodeTabControl.GetItem(index).header = s;
                    }
                }
            }
            catch (Exception e) { Debug.WriteLine(e); }
        }
        public bool StoreDlxFile(DlxTabItem d)
        {
            if (d == null)
                return false;
            if (d.content is DLXEditor)
            {
                try
                {
                    bool b = StoreDlxFile((String)this.IndexToFile[d.index], (String)((DLXEditor)d.content).Text);
                    if (b)
                    {
                        this.findFileStruction((String)this.IndexToFile[d.index]).change = false;
                    }
                    return b;
                }
                catch (Exception e) { return false; }
            }
            else { return false; }
        }
        public OpenedFileStruction findFileStruction(String path)
        {
            for (int i = 0; i < this.openedFiles.Count; i++)
            {
                if (this.openedFiles[i].path.Equals(path))
                    return this.openedFiles[i];
            }
            return null;
        }
        public bool StoreDlxFile(String path, String context)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.GetEncoding("gb2312"));
                sw.Write(context);
                sw.Flush();
                sw.Close();
                fs.Close();
                return true;
            }
            catch (Exception e) { MessageBox.Show(e.ToString()); return false; }
        }


        public bool compiler()
        {
            if (this.TreeViewer.IsSingle)
            {
                this.StoreAllFile();
                string[] path = new string[1];
                path[0] = this.TreeViewer.singleFileName;
                return compiler(path);
            }
            return false;
        }
        public bool linker()
        {
            if (this.TreeViewer.IsSingle)
            {
                this.StoreAllFile();
                string[] path = new string[1];
                path[0] = this.TreeViewer.singleFileName;
                return linker(path, this.TreeViewer.projectName);
            }
            return false;
        }
        public bool compiler(string[] path)
        {
            this.Output("Compiling..." + Environment.NewLine);
            string p = "";
            for (int i = 0; i < path.Length; i++)
            {
                this.Output('\t' + path[i] + Environment.NewLine);
                p = path[i] + " ";
            }
            this.Output("Calling DlxCompiler..." + Environment.NewLine);
            OutProcess.CallOutProcess("dlxcompiler.exe", p, new OutProcess.dReadLine(this.Output), new OutProcess.dExicColde(this.OnCompilerExitCode));
            this.Output(DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond + Environment.NewLine);
            return this.CompilerPass;
        }
        public bool linker(string[] path, string projectName)
        {
            this.Output("Linking..." + Environment.NewLine);
            string p = "";
            for (int i = 0; i < path.Length; i++)
            {
                this.Output('\t' + ReplaceExtention(path[i], "link") + Environment.NewLine);
                p = ReplaceExtention(path[i], "link") + " ";
            }
            p += ReplaceExtention(projectName, "bin");
            this.Output("Calling Linker..." + Environment.NewLine);
            OutProcess.CallOutProcess("dlxlinker.exe", p, new OutProcess.dReadLine(this.Output), new OutProcess.dExicColde(this.OnLinkerExitCode));
            this.Output(DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond + Environment.NewLine);
            return LinkerPass;
        }
        public void CallSimulate()
        {
            if (this.TreeViewer.IsSingle)
            {
                Debug.WriteLine(ReplaceExtention(this.TreeViewer.singleFileName, "bin"));
                OutProcess.CallOutProcessSample(this.HighLightPath + "\\Simulate.exe", ReplaceExtention(this.TreeViewer.singleFileName, "bin"));
            }
        }
        public string ReplaceExtention(string path, string e)
        {
            if (path.LastIndexOf('.') != -1)
                return path.Substring(0, path.LastIndexOf('.') + 1) + e;
            else
                return path;
        }

        delegate void StringParaFun(string s);
        public void Output(string s)
        {
            this.Dispatcher.Invoke(new StringParaFun(this.OutputToText), s);
        }
        public void OutputToText(string s)
        {
            this.OutputText.AppendText(s);
            this.OutputText.ScrollToEnd();
        }
        bool CompilerPass = false;
        public void OnCompilerExitCode(int i)
        {
            if (i == 0)
                CompilerPass = true;
        }
        bool LinkerPass = false;
        public void OnLinkerExitCode(int i)
        {
            if (i == 0)
                LinkerPass = true;
        }

        void edit_DocumentChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < this.openedFiles.Count; i++)
                if (this.openedFiles[i].edit.Equals(sender))
                    this.openedFiles[i].change = true;
        }
        void edit_DocumentFormatted(object sender, CheckResult e)
        {
            this.ErrorShow.SetError(e.Errors);
        }

        void d_CloseClick(object sender, object[] args)
        {
            object p = this.IndexToFile[((DlxTabItem)sender).ControlIndex];
            this.FileToIndex.Remove(p);
            this.IndexToFile.Remove(((DlxTabItem)sender).ControlIndex);
            for (int i = 0; i < this.openedFiles.Count; i++)
                if (openedFiles[i].path.Equals(p))
                {
                    this.openedFiles.RemoveAt(i);
                }
        }
    }

    public class OpenedFileStruction
    {
        public delegate void eventdelegate(object sender, object[] args);
        public event eventdelegate IsChangeChange;
        public string path = "";
        public string name = "";
        private bool _change;
        public bool change
        {
            get
            {
                return _change;
            }
            set
            {
                bool b = _change;
                _change = value;
                if (this.IsChangeChange != null)
                    this.IsChangeChange.Invoke(this, new object[] { value, b });
            }
        }
        public DLXEditor edit;
        public OpenedFileStruction() { }
        public OpenedFileStruction(string path, string name, bool ch, DLXEditor ed)
        {
            this.path = path;
            this.name = name;
            this.change = ch;
            this.edit = ed;
        }

        public static String ReadFile(FileStream f)
        {
            StreamReader sr = new StreamReader(f, System.Text.Encoding.GetEncoding("gb2312"));
            String s = sr.ReadToEnd();
            sr.Close();
            return s;
        }

    }
}