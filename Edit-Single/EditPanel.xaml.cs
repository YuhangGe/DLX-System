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
using Abraham;
using System.Diagnostics;

namespace Edit_Single
{
    /// <summary>
    /// EditPanel.xaml 的交互逻辑
    /// </summary>
    public partial class EditPanel : UserControl
    {
        UIControl uiControl;
        DLXEditor editor;

        public IInputElement MainTextEditor
        {
            get
            {
                if (this.editor != null)
                    return this.editor.MainTextEditor;
                else
                    return null;
            }
        }

        public EditPanel(UIControl uic)
        {
            InitializeComponent();
            
            this.uiControl = uic;
            uic.editPanel = this;

            this.Init();
            this.InitOver();
        }
        private void Init()
        {
            this.AddEditor();
        }
        private void AddEditor()
        {
            bool numberCheck = this.uiControl.getDeploy().NumberSwitch;
            bool promptCheck = this.uiControl.getDeploy().PromptSwitch;
            this.MainGrid.Children.Clear();
            editor = new DLXEditor();
            editor.FontSize = this.uiControl.getDeploy().FontSize;
            editor.HighlightFile = this.uiControl.getDeploy().HLFile;
            editor.DocumentFormatted += new DLXEditor.DocumentFormattedEventHandler(editor_DocumentFormatted);
            editor.DocumentChanged += new DLXEditor.DocumentChangedEventHandler(editor_DocumentChanged);
            this.MainGrid.Children.Add(editor);
            this.editor.ShowAutoComplete = promptCheck;
            this.editor.ShowLineNumbers = numberCheck;
            this.uiControl.mainForm.SetCheck("prompt", promptCheck);
            this.uiControl.mainForm.SetCheck("number", numberCheck);
            this.uiControl.mainForm.EditorCommandBinding();
        }

        private void InitOver()
        {
            this.uiControl.mainForm.CheckChanged += new MainForm.CheckButtonHandle(mainForm_CheckChanged);
        }

        void mainForm_CheckChanged(object sender, string name, bool value)
        {
            if (name.Equals("prompt"))
                this.editor.ShowAutoComplete = value;
            if (name.Equals("number"))
                this.editor.ShowLineNumbers = value;
        }

        void editor_DocumentChanged(object sender, EventArgs e)
        {
            this.uiControl.fileControl.SetChange(true);
            if (this.uiControl.editPanel.getText().Length > 0 && this.uiControl.editPanel.getText()[0] == 'h')
            {
                this.uiControl.outputPanel.Show(Deploy.h);
            }
        }

        void editor_DocumentFormatted(object sender, CheckResult e)
        {
            if (this.uiControl.editPanel.getText().Length > 0 && this.uiControl.editPanel.getText()[0] == 'h')
            {
                this.uiControl.outputPanel.Show(Deploy.h);
                return;
            }

            if (e.Errors.Count > 0)
            {
                String s = "";
                s += " 存在语法错误：\n";
                for (int i = 0; i < e.Errors.Count; i++)
                {
                    s += ("{E," + e.Errors[i].Line + "," + e.Errors[i].Colum + "," + e.Errors[i].ToString() + "}");
                    s += "    ->";
                    s += e.Errors[i].ToString();
                    s += "\n";
                }
                s += ("{O,1} 共" + e.Errors.Count + "个错误!");
                uiControl.FormattedErrorShow(s);
            }
            else
            {
                String s = " --------------------------\n";
                s += "          就绪  :-)\n";
                s += " --------------------------";
                uiControl.FormattedErrorShow(s);
            }
        }

        public void Fresh()
        {
            this.AddEditor();
        }

        public void FocusOn()
        {
            this.editor.Focus();
        }

        public void ErrorTip(int line, int col)
        {
            this.editor.SelectDocument(line, col);
        }

        public void setText(String s)
        {
            this.editor.Text = s;
        }
        public String getText()
        {
            return (String)this.editor.Text;
        }
    }
}
