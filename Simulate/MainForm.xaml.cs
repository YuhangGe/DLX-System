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

namespace Simulate
{
    /// <summary>
    /// MainForm.xaml 的交互逻辑
    /// </summary>
    public partial class MainForm : Window
    {
        bool CloseForm = false;
        ChildWindow RegisterForm = null;
        ChildWindow ConsoleForm = null;
        ChildFormControl formControl = ChildFormControl.getInstance();
        double activeTop = 0;
        double activeLeft = 0;

        public MainForm()
        {
            InitializeComponent();
            this.init();
        }

        private void init()
        {       

            this.RegisterForm = makeRegisterForm();
            this.ConsoleForm = makeConsoleForm();
            initState();
            //this.RegisterForm.Owner = this;
            //this.ConsoleForm.Owner = this;
            this.RegisterForm.Show();
            this.ConsoleForm.Show();
           
            formControl.setWindows(this, this.RegisterForm, this.ConsoleForm);
            
            //Event
            this.LocationChanged += new EventHandler(MainForm_LocationChanged);
            this.Activated += new EventHandler(MainForm_Activated);
            this.memoryPanel.getViewer("Merge").MouseDown += new MouseButtonEventHandler(MainForm_MouseDown);
            this.KeyDown += new KeyEventHandler(MainForm_KeyDown);

            try
            {
                if (this.formControl.clargs != null)
                    this.memoryPanel.load(this.formControl.clargs);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            ChildFormControl.getInstance().update();
        }

        void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)            
                SmallTool.PerformClick(this.memoryPanel.getButton("Debug"));
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F11)
                SmallTool.PerformClick(this.memoryPanel.getButton("Stepover"));
            if (e.Key == Key.F11)
                SmallTool.PerformClick(this.memoryPanel.getButton("Stepinto"));
            if (e.Key == Key.F12)
                SmallTool.PerformClick(this.memoryPanel.getButton("Stepout"));
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F5)
                SmallTool.PerformClick(this.memoryPanel.getButton("Run"));
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F6)
                SmallTool.PerformClick(this.memoryPanel.getButton("Stop"));
            if (e.Key == Key.F9)
                SmallTool.PerformClick(this.memoryPanel.getButton("BreakPoint"));
            if (e.Key == Key.F8)
                SmallTool.PerformClick(this.memoryPanel.getButton("Setpc"));

        }
        
        void MainForm_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.initState();
        }

        void MainForm_Activated(object sender, EventArgs e)
        {
            formControl.active(this);            
        }
        void MainForm_LocationChanged(object sender, EventArgs e)
        {
            this.formControl.move(this.Left - this.activeLeft, this.Top - this.activeTop);
            this.activeLeft = this.Left;
            this.activeTop = this.Top;
        }

        private void initState()
        {
            this.MinWidth = 525;
            this.MinHeight = 165;
            double height = SystemParameters.WorkArea.Height;
            double width = SystemParameters.WorkArea.Width;
            this.Width = width - 278 < 680 ? width - 278 : 680;
            int workHeight = ((int)height) < (646 + 46) ? ((int)height) : (646 + 46);
            this.Height = (double)((3 * workHeight / 4 - 165) / 19 * 19 + 165);
            this.RegisterForm.Width = 278;
            this.RegisterForm.MinWidth = 278;
            this.RegisterForm.MaxWidth = 278;
            this.RegisterForm.Height = (workHeight - 46) / 19 * 19 + 46;
            this.ConsoleForm.Width = this.Width;
            this.ConsoleForm.Height = this.RegisterForm.Height - this.Height;

            this.Top = (height - this.RegisterForm.Height) / 2;
            this.Left = (width - this.Width - this.RegisterForm.Width) / 2;
            this.RegisterForm.Top = this.Top;
            this.RegisterForm.Left = this.Left + this.Width;
            this.ConsoleForm.Top = this.Top + this.Height;
            this.ConsoleForm.Left = this.Left;
            this.activeTop = this.Top;
            this.activeLeft = this.Left;

            this.RegisterForm.Visibility = Visibility.Visible;
            this.ConsoleForm.Visibility = Visibility.Visible;
        }

        private ChildWindow makeRegisterForm()
        {
            ChildWindow rw = new ChildWindow();
            rw.Title = "RegisterWindow";
            rw.Height = 693;           
            rw.Width = 278;            
            rw.getMainPanel().Children.Add(new ScrollPanel(new RegisterPanel()));
            return rw;
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
