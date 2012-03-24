using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Edit_Single
{
    public class FileControl
    {
        UIControl uiControl;

        private String path = "";
        private String name = "";
        public bool isChange = false;

        public FileControl(UIControl uic)
        {
            this.uiControl = uic;
        }

        public int SaveTip()
        {

            if (this.isChange)
            {
                MessageBoxResult dr = MessageBox.Show(this.uiControl.mainForm, "The file has been modified, Save it or not?", "", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                int index;
                switch (dr)
                {
                    case MessageBoxResult.Yes: index = 0; break;
                    case MessageBoxResult.No: index = 1; break;
                    case MessageBoxResult.Cancel: index = 2; break;
                    default: index = 2; break;
                }
                return index;
            }
            else
            {
                return -1;
            }
        }

        public void NewFile()
        {
            int index = this.SaveTip();
            if (index == 2)
                return;
            if (index == 0)
            {
                int vs = this.SaveFile();
                if (vs == 1)
                {
                    return;
                }
                else if (vs < 0)
                {
                    MessageBox.Show("Save File Error!");
                    return;
                }
            }
            this.newFile();
        }
        public void newFile()
        {
            this.uiControl.mainForm.SetName("NoName");
            this.uiControl.editPanel.Fresh();
            this.uiControl.outputPanel.Fresh();
            this.path = "";
            this.name = "";
            this.SetChange(false);
            this.uiControl.editPanel.FocusOn();
        }

        public int SaveFile()
        {
            if (this.name != "")
            {
                return saveFile(path, name);
            }
            else
                return SaveAsFile("DLX Save");
        }
        public int SaveAsFile()
        {
            return SaveAsFile("DLX Save As");
        }
        private int SaveAsFile(String str)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".dlx";
            sfd.AddExtension = true;
            sfd.Filter = "DLX Documents(*" + this.uiControl.getDeploy().DlxExt + ")|*" + this.uiControl.getDeploy().DlxExt + "|All Files(*.*)|*.*";
            sfd.Title = str;
            if (this.name != "")
                sfd.FileName = this.name;
            if (sfd.ShowDialog(this.uiControl.mainForm) == true)
            {
                return saveFile(ParsePath(sfd.FileName), ParseName(sfd.FileName));
            }
            else
            {
                return 1;
            }
        }
        private int saveFile(String path, String name)
        {
            int index = saveFile(CombineName(path, name));
            if (index == 0)
            {
                SavedUI(path, name);
                return 0;
            }
            else
            {
                return index;
            }
        }
        private int saveFile(String name)
        {
            try
            {
                FileStream fs = new FileStream(name, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.GetEncoding(this.uiControl.getDeploy().CharEncode));
                sw.Write(this.uiControl.editPanel.getText());
                sw.Flush();
                sw.Close();
                fs.Close();
                return 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Save File : " + ex.ToString());
                return -1;
            }
        }
        private void SavedUI(String path, String name)
        {
            this.path = path;
            this.name = name;
            this.uiControl.mainForm.SetName(name);
            this.SetChange(false);
        }

        public void OpenFile()
        {
            int index = this.SaveTip();
            if (index == 2)
                return;
            if (index == 0)
            {
                int vs = this.SaveFile();
                if (vs == 1)
                {
                    return;
                }
                else if (vs < 0)
                {
                    MessageBox.Show("Save File Error!");
                    return;
                }
            }
            if (this.openFile() < 0)
            {
                MessageBox.Show("Open File Error!");
                return;
            }
        }
        private int openFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "DLX Documents(*" + this.uiControl.getDeploy().DlxExt + ")|*" + this.uiControl.getDeploy().DlxExt + "|All Files(*.*)|*.*";
            ofd.Title = "DLX Open File";
            if (ofd.ShowDialog(this.uiControl.mainForm) == true)
            {
                return openFile(ParsePath(ofd.FileName), ParseName(ofd.FileName));
            }
            else
            {
                return 1;
            }
        }
        private int openFile(String path, String name)
        {
            try
            {
                FileStream fs = null;
                fs = new FileStream(CombineName(path, name), FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding(this.uiControl.getDeploy().CharEncode));
                String s = sr.ReadToEnd();
                sr.Close();
                fs.Close();
                this.uiControl.editPanel.setText(s);
                this.path = path;
                this.name = name;
                this.isChange = false;
                this.uiControl.mainForm.SetName(name);
                return 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Open File: " + ex.ToString());
                return -1;
            }
        }


        public void SetChange(bool b)
        {
            this.isChange = b;
            if (isChange == true)
            {
                if (this.name == "")
                    this.uiControl.mainForm.SetName("NoName*");
                else
                    this.uiControl.mainForm.SetName(this.name + "*");
            }
            else
            {
                if (this.name == "")
                    this.uiControl.mainForm.SetName("NoName");
                else
                    this.uiControl.mainForm.SetName(this.name + "");
            }
        }

        public String getName()
        {
            return CombineName(this.path, this.name);
        }
        public bool hasName()
        {
            if (this.name == "")
                return false;
            return true;
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
            if (n.Length != 0 && n[0] == '.')
                ext = n;
            else
                ext = "." + n;
            if (index == -1)
                return name + ext;
            string st = name.Substring(0, index);
            return st + ext;
        }
    }
}
