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
using System.Windows.Input;

namespace Edit_Single
{
	public partial class MainForm
	{
        public delegate void CheckButtonHandle(object sender, String name, bool value);
        public event CheckButtonHandle CheckChanged = null;

        UIControl uiControl;
		public MainForm()
		{
			this.InitializeComponent();
            this.Init();
            this.InitOver();
            // 在此点之下插入创建对象所需的代码。
		}

        
  
        private void Init()
        {
            this.uiControl = new UIControl();
            this.uiControl.mainForm = this;
            OutputPanel op = new OutputPanel(this.uiControl);
            this.OutputGrid.Children.Add(op);
            EditPanel dp = new EditPanel(this.uiControl);
            this.EditGrid.Children.Add(dp);

            this.NewButton.Click += new RoutedEventHandler(NewButton_Click);
            this.OpenButton.Click += new RoutedEventHandler(OpenButton_Click);
            this.SaveButton.Click += new RoutedEventHandler(SaveButton_Click);
            this.CompileButton.Click += new RoutedEventHandler(CompileButton_Click);
            this.CompileLinkButton.Click += new RoutedEventHandler(CompileLinkButton_Click);
            this.RunCompileLinkButton.Click += new RoutedEventHandler(RunCompileLinkButton_Click);
            this.MultiLinkButton.Click += new RoutedEventHandler(MultiLinkButton_Click);

            this.NewItem.Click += new RoutedEventHandler(NewItem_Click);
            this.OpenItem.Click += new RoutedEventHandler(OpenItem_Click);
            this.SaveItem.Click += new RoutedEventHandler(SaveItem_Click);
            this.SaveAsItem.Click += new RoutedEventHandler(SaveAsItem_Click);
            this.ExitItem.Click += new RoutedEventHandler(ExitItem_Click);

            this.AboutItem.Click += new RoutedEventHandler(AboutItem_Click);

            this.PromptCheck.SetImage("icon\\prompt.png");
            this.NumberCheck.SetImage("icon\\number.png");
            this.PromptCheck.CheckChanged += new CheckButton.CheckChangeHandle(CheckButton_CheckChanged);
            this.NumberCheck.CheckChanged += new CheckButton.CheckChangeHandle(CheckButton_CheckChanged);

            this.FindButton.Click += new RoutedEventHandler(FindButton_Click);

            this.KeyDown += new KeyEventHandler(MainForm_KeyDown);

            this.Closing += new System.ComponentModel.CancelEventHandler(MainForm_Closing);
        }

        void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            AboutForm af = new AboutForm();
            af.Owner = this;
            af.ShowDialog();
        }

        void FindButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N)
                UIControl.PerformMenuItemClick(this.NewItem);
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.O)
                UIControl.PerformMenuItemClick(this.OpenItem);
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
                UIControl.PerformMenuItemClick(this.SaveItem);
        }

        void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int index = this.uiControl.fileControl.SaveTip();
            if (index == 2)
            {
                e.Cancel = true;
                return;
            }
            if (index == 0)
            {
                int vs = this.uiControl.fileControl.SaveFile();
                if (vs == 1)
                {
                    e.Cancel = true;
                    return;
                }
                else if (vs < 0)
                {
                    MessageBox.Show("Save File Error!");
                    e.Cancel = true;
                    return;
                }
            }
        }
        void CheckButton_CheckChanged(object sender, bool value)
        {
            String s = "";
            if (sender == this.PromptCheck)
                s = "prompt";
            else if (sender == this.NumberCheck)
                s = "number";
            if (this.CheckChanged != null)
                this.CheckChanged.Invoke(sender, s, value);
        }

        void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        void SaveAsItem_Click(object sender, RoutedEventArgs e)
        {
            this.uiControl.fileControl.SaveAsFile();
        }
        void SaveItem_Click(object sender, RoutedEventArgs e)
        {
            this.uiControl.fileControl.SaveFile();
        }
        void OpenItem_Click(object sender, RoutedEventArgs e)
        {
            this.uiControl.fileControl.OpenFile();
        }
        void NewItem_Click(object sender, RoutedEventArgs e)
        {
            this.uiControl.fileControl.NewFile();
        }

        void MultiLinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.uiControl.programControl.OpenMultiLinker();
        }
        void RunCompileLinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.uiControl.programControl.CompileLinkRun();
        }
        void CompileLinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.uiControl.programControl.CompileLink();
        }
        void CompileButton_Click(object sender, RoutedEventArgs e)
        {
            this.uiControl.programControl.Compile();
        }
        void NewButton_Click(object sender, RoutedEventArgs e)
        {
            this.uiControl.fileControl.NewFile();
        }
        void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            this.uiControl.fileControl.OpenFile();
        }
        void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.uiControl.fileControl.SaveFile();
        }

        private void InitOver()
        {
            this.uiControl.fileControl.newFile();
        }
        
        
        public void EditorCommandBinding()
        {
            IInputElement iie = this.uiControl.editPanel.MainTextEditor;
            
            if (iie != null)
            {
                this.CopyButton.Command = ApplicationCommands.Copy;
                this.CopyButton.CommandTarget = iie;
                this.CutButton.Command = ApplicationCommands.Cut;
                this.CutButton.CommandTarget = iie;
                this.PasteButton.Command = ApplicationCommands.Paste;
                this.PasteButton.CommandTarget = iie;
                this.UndoButton.Command = ApplicationCommands.Undo;
                this.UndoButton.CommandTarget = iie;
                this.RedoButton.Command = ApplicationCommands.Redo;
                this.RedoButton.CommandTarget = iie;

                this.FindButton.Command = ApplicationCommands.Find;
                this.FindButton.CommandTarget = iie;
                
                this.CopyItem.Command = ApplicationCommands.Copy;
                this.CopyItem.CommandTarget = iie;
                this.CutItem.Command = ApplicationCommands.Cut;
                this.CutItem.CommandTarget = iie;
                this.PasteItem.Command = ApplicationCommands.Paste;
                this.PasteItem.CommandTarget = iie;
                this.UndoItem.Command = ApplicationCommands.Undo;
                this.UndoItem.CommandTarget = iie;
                this.RedoItem.Command = ApplicationCommands.Redo;
                this.RedoItem.CommandTarget = iie;
                this.DeleteItem.Command = ApplicationCommands.Delete;
                this.DeleteItem.CommandTarget = iie;
                this.SelectAllItem.Command = ApplicationCommands.SelectAll;
                this.SelectAllItem.CommandTarget = iie;
            }
        }


        public void SetCheck(String str, bool b)
        {
            if (str.Equals("prompt"))
                this.PromptCheck.ChangeCheck(b);
            if (str.Equals("number"))
                this.NumberCheck.ChangeCheck(b);
        }


        public void SetName(String name)
        {
            this.Title = "Edit Single - " + name;
        }
	}
}