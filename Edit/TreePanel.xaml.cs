using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace Edit
{
    /// <summary>
    /// TreePanel.xaml 的交互逻辑
    /// </summary>
    public partial class TreePanel : UserControl
    {
        public delegate void eventdelegate(object sender, object[] args);
        public event eventdelegate FileDoubleClick;

        String path="";
        public String projectName="";
        List<string> filesName;
        List<string> folderName;
        public bool IsSingle;
        public String singleFileName = "";
        public TreePanel()
        {
            InitializeComponent();

            Thread t = new Thread(new ThreadStart(TestChange));
            t.IsBackground = true;
            t.Start();
        }
        public void setDirectories(String path,String pn,bool isSingle)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            this.path = path;           
            this.projectName = pn;
            filesName = new List<string>();
            folderName = new List<string>();
            this.IsSingle = isSingle;
            this.singleFileName = path  + pn;
        }
        public void CloseProject()
        {
            this.path = "";
            this.projectName = "";
            this.singleFileName = "";
            filesName = new List<string>();
            folderName = new List<string>();
            paths = new List<string>();
            this.ProjectTreeViewer.Items.Clear();
        }
        private List<string> ReadDirectory(DirectoryInfo di)
        {
            List<string> result = new List<string>();
            DirectoryInfo[] children = di.GetDirectories();
            for (int i = 0; i < children.Length; i++)
            {
                result.Add(children[i].FullName);
                List<string> tem=this.ReadDirectory(children[i]);
                for (int j = 0; j < tem.Count; j++)
                    result.Add(tem[j]);
            }
            return result;
        }
        private List<string> ReadFiles(DirectoryInfo di)
        {
            List<string> result = new List<string>();
            FileInfo[] childrenFile = di.GetFiles();
            for (int i = 0; i < childrenFile.Length; i++)
                result.Add(childrenFile[i].FullName);
            DirectoryInfo[] childrenDirectory = di.GetDirectories();
            for (int i = 0; i < childrenDirectory.Length; i++)
            {
                List<string> tem = ReadFiles(childrenDirectory[i]);
                for (int j = 0; j < tem.Count; j++)
                    result.Add(tem[j]);
            }
            return result;
        }
        
        private delegate void NoneParmFun();
        bool filesChange = false;
        private void CheckFile()
        {
            if (path == "")
                return;
            List<string> files = ReadFiles(new DirectoryInfo(this.path));
            List<string> folder = ReadDirectory(new DirectoryInfo(this.path));

            if (this.filesName.Count != files.Count || this.folderName.Count != folder.Count)
                this.filesChange = true;
            for (int i = 0; i < this.filesName.Count; i++)
                if (!File.Exists(filesName[i]))
                    this.filesChange = true;
            for (int i = 0; i < this.folderName.Count; i++)
                if (!Directory.Exists(folderName[i]))
                    this.filesChange = true;
            if (this.filesChange)
            {
                this.filesName = files;
                this.folderName = folder;
            }
        }
        private void ChangeShow()
        {
            this.ProjectTreeViewer.Items.Clear();
            TreeItemPanel pp = new TreeItemPanel(ItemType.Project, this.projectName);
            TreeViewItem p = new TreeViewItem();
            p.Header = pp;
            p.IsExpanded = true;
            this.ProjectTreeViewer.Items.Add(p);
            AddShow(new DirectoryInfo(this.path), p);
            filesChange = false;
        }
        private void AddShow(DirectoryInfo di, TreeViewItem ti)
        {
            DirectoryInfo[] ds = di.GetDirectories();
            for (int i = 0; i < ds.Length; i++)
            {
                TreeItemPanel tem = new TreeItemPanel(ItemType.Folder, ds[i].Name);
                tem.IsFile = false;
                tem.Path = ds[i].FullName;
                TreeViewItem temi = new TreeViewItem();
                temi.Header = tem;
                ti.Items.Add(temi);
                this.AddShow(ds[i], temi);
            }
            FileInfo[] fs = di.GetFiles();
            for (int i = 0; i < fs.Length; i++)
            {
                TreeItemPanel tem = null;
                if ((fs[i].Extension.ToUpper()).Equals(".DLX"))
                    tem = new TreeItemPanel(ItemType.CodeFile, fs[i].Name);
                if ((fs[i].Extension.ToUpper()).Equals(".LINK"))
                    tem = new TreeItemPanel(ItemType.LinkFile, fs[i].Name);
                if ((fs[i].Extension.ToUpper()).Equals(".BIN"))
                    tem = new TreeItemPanel(ItemType.BinFile, fs[i].Name);
                if (tem != null)
                {
                    tem.IsFile = true;
                    tem.Path = fs[i].FullName;
                    TreeViewItem temi = new TreeViewItem();
                    temi.Header = tem;
                    temi.MouseDoubleClick += new MouseButtonEventHandler(temi_MouseDoubleClick);
                    ti.Items.Add(temi);
                }
            }
        }
        void temi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                TreeItemPanel p = ((TreeItemPanel)(((TreeViewItem)sender).Header));
                if (this.FileDoubleClick != null)
                    this.FileDoubleClick.Invoke(this, new object[] { this.projectName, p.Path, p.IsFile });
            }
        }
        bool singleFileChange = false;
        string ShowPath="" ;
        List<String> paths = new List<string>();
        private void CheckSingleFile()
        {
            if (path == "")
                return;
            if (!ShowPath.Equals(this.singleFileName))
            {
                this.singleFileChange = true;
                ShowPath = this.singleFileName;
            }
            else
            {
                FileInfo f = new FileInfo(this.singleFileName);
                String filename = f.FullName;
                String extension = f.Extension;
                String _fileName = filename.Substring(0, filename.Length - extension.Length);
                DirectoryInfo di = new DirectoryInfo(this.path);
                FileInfo[] fs= di.GetFiles();
                int N = 0;
                List<string> tems = new List<string>();
                for (int i = 0; i < fs.Length; i++)
                {
                    if (fs[i].FullName.ToUpper().Equals((_fileName + ".DLX").ToUpper()) ||
                       fs[i].FullName.ToUpper().Equals((_fileName + ".LINK").ToUpper()) ||
                       fs[i].FullName.ToUpper().Equals((_fileName + ".BIN").ToUpper()))
                    {
                        if (this.paths.IndexOf(fs[i].FullName.ToUpper()) == -1)
                        {
                            this.singleFileChange = true;                            
                        }
                        tems.Add(fs[i].FullName.ToUpper()); 
                        N++;
                    }
                }
                if (N != this.paths.Count)
                    this.singleFileChange = true;
                if (this.singleFileChange)
                {
                    this.paths = tems;
                }
            }
        }
        private void ChangeSingleShow()
        {
            this.ProjectTreeViewer.Items.Clear();
            TreeItemPanel pp = new TreeItemPanel(ItemType.Project, this.projectName);
            TreeViewItem p = new TreeViewItem();
            p.Header = pp;
            p.IsExpanded = true;
            this.ProjectTreeViewer.Items.Add(p);            
            AddSingleShow(new DirectoryInfo(this.path), p);
            singleFileChange = false;
        }
        private void AddSingleShow(DirectoryInfo di, TreeViewItem ti)
        {
            FileInfo f = new FileInfo(this.singleFileName);
            String filename = f.Name;
            String extension = f.Extension;
            String _fileName = filename.Substring(0, filename.Length - extension.Length);
            FileInfo[] fs = di.GetFiles();
            for (int i = 0; i < fs.Length; i++)
            {
                TreeItemPanel tem = null;
                if ((fs[i].Name.ToUpper()).Equals(_fileName.ToUpper()+".DLX"))
                    tem = new TreeItemPanel(ItemType.CodeFile, fs[i].Name);
                if ((fs[i].Name.ToUpper()).Equals(_fileName.ToUpper() + ".LINK"))
                    tem = new TreeItemPanel(ItemType.LinkFile, fs[i].Name);
                if ((fs[i].Name.ToUpper()).Equals(_fileName.ToUpper() + ".BIN"))
                    tem = new TreeItemPanel(ItemType.BinFile, fs[i].Name);
                if (tem != null)
                {
                    tem.IsFile = true;
                    tem.Path = fs[i].FullName;
                    TreeViewItem temi = new TreeViewItem();
                    temi.Header = tem;
                    temi.MouseDoubleClick += new MouseButtonEventHandler(temi_MouseDoubleClick);
                    ti.Items.Add(temi);
                }
            }
        }

        private void TestChange()
        {
            while (true)
            {
                Thread.Sleep(50);
                if (!this.IsSingle)
                {
                    CheckFile();
                   // this.Dispatcher.Invoke(new NoneParmFun(CheckFile));
                    if (this.filesChange)
                        this.Dispatcher.Invoke(new NoneParmFun(ChangeShow));
                }
                else
                {
                    CheckSingleFile();
                    if (this.singleFileChange)
                        this.Dispatcher.Invoke(new NoneParmFun(ChangeSingleShow));
                }
            }
        }
    }
}
