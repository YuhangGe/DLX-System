using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Input;

namespace Simulate
{
    public partial class ControlForm
    {
        bool CloseForm = false;
        ChildWindow RegisterForm = null;
        ChildWindow MemoryForm = null;
        ChildWindow ConsoleForm = null;
        public ControlForm()
        {

            this.InitializeComponent();
            TipInit();
            FormInit();
            EventInit();
            // 在此点之下插入创建对象所需的代码。
        }
        private void TipInit()
        {
            this.RegisterShow.ToolTip = new RegisterTipPanel();
            this.MemoryShow.ToolTip = new MergeTipPanel();
            this.ConsoleShow.ToolTip = new ConsoleTipPanel();
        }
        private void EventInit()
        {
            this.Closing += new System.ComponentModel.CancelEventHandler(ControlForm_Closing);
            this.StateChanged += new EventHandler(ControlForm_StateChanged);

            this.RegisterShow.MouseDown += new MouseButtonEventHandler(RegisterShow_MouseDown);
            this.MemoryShow.MouseDown += new MouseButtonEventHandler(MemoryShow_MouseDown);
            this.ConsoleShow.MouseDown += new MouseButtonEventHandler(ConsoleShow_MouseDown);
            this.OpenfileButton.Click += new RoutedEventHandler(OpenfileButton_Click);
            this.RunButton.Click += new RoutedEventHandler(RunButton_Click);
        }
        private void FormInit()
        {
            RegisterForm = makeRegisterForm();
            RegisterForm.Closing += new System.ComponentModel.CancelEventHandler(ChildFormClosing);
            RegisterForm.Activated += new EventHandler(ChildForm_Activated);
            MemoryForm = makeMemoryForm();
            MemoryForm.Closing += new System.ComponentModel.CancelEventHandler(ChildFormClosing);
            MemoryForm.Activated += new EventHandler(ChildForm_Activated);
            ConsoleForm = makeConsoleForm();
            ConsoleForm.Closing += new System.ComponentModel.CancelEventHandler(ChildFormClosing);
            ConsoleForm.Activated += new EventHandler(ChildForm_Activated);
            int AllWidth = (int)SystemParameters.WorkArea.Width;
            int AllHeight = (int)SystemParameters.WorkArea.Height;
            int wm = AllWidth * 2 / 3 - 278;
            if (wm < 424) wm = 424;
            int hr = (AllHeight * 3 / 4 - 48) / 19 * 19 + 48;
            if (hr < 181)
                hr = 181;
            int hm = (hr * 2 / 3 - 75) / 19 * 19 + 75;
            int hc = hr - hm;
            int wc = wm;
            int wr = 278;
            int left = (AllWidth - 278 - wm) / 2;
            int top = (AllHeight - hr) / 2;
            MemoryForm.Top = top;
            MemoryForm.Left = left;
            MemoryForm.Width = wm;
            MemoryForm.Height = hm;
            RegisterForm.Top = top;
            RegisterForm.Left = left + wm;
            RegisterForm.Width = wr;
            RegisterForm.Height = hr;
            ConsoleForm.Top = top + hm;
            ConsoleForm.Left = left;
            ConsoleForm.Width = wc;
            ConsoleForm.Height = hc;
            MemoryForm.Show();
            RegisterForm.Show();
            ConsoleForm.Show();
        }

        void ControlForm_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.RegisterForm.WindowState = WindowState.Minimized;
                this.MemoryForm.WindowState = WindowState.Minimized;
                this.ConsoleForm.WindowState = WindowState.Minimized;
            }
            else if (this.WindowState == WindowState.Normal)
            {
                this.RegisterForm.WindowState = WindowState.Normal;
                this.MemoryForm.WindowState = WindowState.Normal;
                this.ConsoleForm.WindowState = WindowState.Normal;
            }
        }
        void ControlForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.CloseForm = true;
            try
            {
                RegisterForm.Close();
            }
            catch (Exception ex) { }
            try
            {
                MemoryForm.Close();
            }
            catch (Exception ex) { }
            try
            {
                ConsoleForm.Close();
            }
            catch (Exception ex) { }
        }

        void RunButton_Click(object sender, RoutedEventArgs e)
        {
            CPUInfo.getInstance().PowerOff();
        }

        void OpenfileButton_Click(object sender, RoutedEventArgs e)
        {
            CPUInfo.getInstance().PowerOn();
            //CPUInfo.getInstance().RunProgramThreadStart();
        }

        void ConsoleShow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ConsoleForm.Visibility == Visibility.Hidden)
            {
                ConsoleForm.Visibility = Visibility.Visible;
                this.Activate();
                ConsoleForm.Activate();
            }
            else
            {
                ConsoleForm.Activate();
            }
        }
        void MemoryShow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MemoryForm.Visibility == Visibility.Hidden)
            {
                MemoryForm.Visibility = Visibility.Visible;
                this.Activate();
                MemoryForm.Activate();
            }
            else
            {
                MemoryForm.Activate();
            }
        }
        void RegisterShow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (RegisterForm.Visibility == Visibility.Hidden)
            {
                RegisterForm.Visibility = Visibility.Visible;
                this.Activate();
                RegisterForm.Activate();
            }
            else
            {
                RegisterForm.Activate();
            }
        }

        void ChildFormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!CloseForm)
            {
                ((ChildWindow)sender).Visibility = Visibility.Hidden;
                e.Cancel = true;
            }
            else
                return;
        }
        void ChildForm_Activated(object sender, EventArgs e)
        {
            this.Activate();
            ((ChildWindow)sender).Activate();
        }
       


        private ChildWindow makeRegisterForm()
        {
            ChildWindow rw = new ChildWindow();
            rw.Title = "RegisterWindow";
            rw.Height = 693;
            rw.MaxHeight = 693;
            rw.MinHeight = 84;
            rw.Width = 278;
            rw.MaxWidth = 278;
            rw.MinWidth = 278;
            if (SystemParameters.WorkArea.Height < 693)
                rw.Height = (SystemParameters.WorkArea.Height - 46) / 19 * 19 + 48;
            rw.getMainPanel().Children.Add(new ScrollPanel(new RegisterPanel()));
            return rw;
        }
        private ChildWindow makeMemoryForm()
        {
            ChildWindow mw = new ChildWindow();
            mw.Title = "MemoryWindow";
            mw.Height = 113;
            mw.MinHeight = 113;
            mw.Width = 630;
            mw.MinWidth = 424;
            mw.getMainPanel().Children.Add(new MemoryPanel());
            return mw;
        }
        private ChildWindow makeConsoleForm()
        {
            ChildWindow cw = new ChildWindow();
            cw.Title = "ConsoleWindow";
            cw.getMainPanel().Children.Add(new ConsolePanel());
            return cw;
        }
    }
}