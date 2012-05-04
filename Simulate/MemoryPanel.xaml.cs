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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Microsoft.Win32;
using System.Collections;
using System.Threading;

namespace Simulate
{
    /// <summary>
    /// MemoryPanel.xaml 的交互逻辑
    /// </summary>
    public partial class MemoryPanel : UserControl
    {
        public delegate void NoParameterFun();
        public delegate void _itemNumberChanged(int x);
        public _itemNumberChanged itemNumberChanged;
        int _itemNumber;
        int itemNumber
        {
            get
            {
                return _itemNumber;
            }
            set
            {
                _itemNumber = value;
                if (itemNumberChanged != null)
                    itemNumberChanged(value);
            }
        }
        int chooseItem;
        //StringItem[,] ValueInfo;
        MemoryPanelControl mpc;
        public delegate void eventdelegate(object sender, object[] args);
        public event eventdelegate AddressChooseChange;

        //private static int MaxItemNumber = 40;
        private static StringItem xZero = new StringItem("x00000000", 0);
        private static StringItem bZero = new StringItem("00000000", 0);
        private static StringItem iZero = new StringItem("NOP", 0);

        public MemoryPanel()
        {
            InitializeComponent();
            this.SizeChanged += new SizeChangedEventHandler(MemoryPanel_SizeChanged);
            //this.ValueInfo = new StringItem[MaxItemNumber, 7];
            //for (int i = 0; i < MaxItemNumber; i++)
            //{
            //    ValueInfo[i, 0] = new StringItem("x00000000", 0);
            //    ValueInfo[i, 1] = new StringItem("00000000", 0);
            //    ValueInfo[i, 2] = new StringItem("00000000", 0);
            //    ValueInfo[i, 3] = new StringItem("00000000", 0);
            //    ValueInfo[i, 4] = new StringItem("00000000", 0);
            //    ValueInfo[i, 5] = new StringItem("x00000000", 0);
            //    ValueInfo[i, 6] = new StringItem("NOP", 0);
            //}
            this.Trap0Switch.SetImage("\\icon\\trap0breakpoint.png");
            this.InterruptSwitch.SetImage("\\icon\\interrupt.png");            

            this.MouseWheel += new MouseWheelEventHandler(MemoryPanel_MouseWheel);

            this.scrollBar.Maximum = 1073741823;
            this.scrollBar.SmallChange = 1;
            this.scrollBar.ValueChanged += new RoutedPropertyChangedEventHandler<double>(scrollBar_ValueChanged);

            this.chooseItem = 0;

            mpc = MemoryPanelControl.getInstance();

            this.RegisterViewer.ToolTip = new RegisterTipPanel();
            this.ConsoleViewer.ToolTip = new ConsoleTipPanel();
            this.MergeViewer.ToolTip = new MergeTipPanel();

            this.AddressChoose.KeyDown += new KeyEventHandler(AddressChoose_KeyDown);
            this.AddressChoose.GotFocus += new RoutedEventHandler(AddressChoose_GotFocus);
            this.AddressChoose.LostFocus += new RoutedEventHandler(AddressChoose_LostFocus);
            this.AddressChoose.Text = "Input address";

            this.AddressChoose.SelectionChanged += new SelectionChangedEventHandler(AddressChoose_SelectionChanged);

            CPUInfo.getInstance().HaltChangeEvent += new CPUInfo.eventdelegate(MemoryPanel_HaltChangeEvent);

            this.OpenfileButton.Click += new RoutedEventHandler(OpenfileButton_Click);
            this.SetpcButton.Click += new RoutedEventHandler(SetpcButton_Click);
            this.StepintoButton.Click += new RoutedEventHandler(StepintoButton_Click);
            this.StopButton.Click += new RoutedEventHandler(StopButton_Click);
            this.RunButton.Click += new RoutedEventHandler(RunButton_Click);
            this.DebugButton.Click += new RoutedEventHandler(DebugButton_Click);
            this.ConditionButton.Click += new RoutedEventHandler(ConditionButton_Click);
            this.BreakPointButton.Click+=new RoutedEventHandler(BreakpointButton_Click);
            this.SetvalueButton.Click += new RoutedEventHandler(SetvalueButton_Click);
            this.StepoutButton.Click += new RoutedEventHandler(StepoutButton_Click);
            this.StepoverButton.Click += new RoutedEventHandler(StepoverButton_Click);

            this.LoadProgramItem.Click += new RoutedEventHandler(OpenfileButton_Click);
            this.RunItem.Click += new RoutedEventHandler(RunButton_Click);
            this.DebugItem.Click += new RoutedEventHandler(DebugButton_Click);
            this.StopItem.Click += new RoutedEventHandler(StopButton_Click);
            this.StepintoItem.Click += new RoutedEventHandler(StepintoButton_Click);
            this.StepoutItem.Click += new RoutedEventHandler(StepoutButton_Click);
            this.StepoverItem.Click += new RoutedEventHandler(StepoverButton_Click);
            this.SetpcItem.Click += new RoutedEventHandler(SetpcButton_Click);
            this.SetValueItem.Click += new RoutedEventHandler(SetvalueButton_Click);
            this.TBreakpointsItem.Click += new RoutedEventHandler(BreakpointButton_Click);
            this.CBreakpointItem.Click += new RoutedEventHandler(ConditionButton_Click);
            this.ResetMemoryItem.Click += new RoutedEventHandler(ResetMemoryItem_Click);
            this.ExitItem.Click += new RoutedEventHandler(ExitItem_Click);
            this.SaveMemoryItem.Click += new RoutedEventHandler(SaveMemoryItem_Click);
            this.AboutItem.Click += new RoutedEventHandler(AboutItem_Click);
            this.DeviceItem.Click += new RoutedEventHandler(DeviceItem_Click);

            this.InterruptSwitch.CheckChanged += new CheckButton.CheckChangeHandle(InterruptSwitch_CheckChanged);
            CPUInfo.getInstance().computer.offInterrupt();

            this.Trap0Switch.Check = true;
            this.Trap0Switch.CheckChanged += new CheckButton.CheckChangeHandle(Trap0Switch_CheckChanged);

            //this.InterruptSwitch.Check = false;
        }

        void Trap0Switch_CheckChanged(object sender, bool value)
        {
            
        }

        void InterruptSwitch_CheckChanged(object sender, bool value)
        {
            if (value)
                CPUInfo.getInstance().computer.onInterrupt();
            else
                CPUInfo.getInstance().computer.offInterrupt();
            ChildFormControl.getInstance().update();
        }

        void DeviceItem_Click(object sender, RoutedEventArgs e)
        {
            ChildFormControl.getInstance().DevicesForm();            
        }

        void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            ChildFormControl.getInstance().AboutForm();
        }

        void SaveMemoryItem_Click(object sender, RoutedEventArgs e)
        {
            ChildFormControl.getInstance().SaveValueForm();
        }

        void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            ChildFormControl.getInstance().ProcessExit();
        }

        void ResetMemoryItem_Click(object sender, RoutedEventArgs e)
        {
            this.mpc.ReSet();
        }

        void StepoverButton_Click(object sender, RoutedEventArgs e)
        {
            CPUInfo.getInstance().RunProgramThreadStart("stepover");
        }

        void StepoutButton_Click(object sender, RoutedEventArgs e)
        {
            CPUInfo.getInstance().RunProgramThreadStart("stepout");
        }

        void SetvalueButton_Click(object sender, RoutedEventArgs e)
        {
            ChildFormControl.getInstance().SetValueForm();
        }

        void ConditionButton_Click(object sender, RoutedEventArgs e)
        {
            ChildFormControl.getInstance().ConditionBreakpointsForm();
        }

        void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            CPUInfo.getInstance().RunProgramThreadStart("debug");
        }

        void RunButton_Click(object sender, RoutedEventArgs e)
        {
            CPUInfo.getInstance().RunProgramThreadStart("run");            
        }

        void StopButton_Click(object sender, RoutedEventArgs e)
        {
            CPUInfo.getInstance().PowerOff();
        }
        void StepintoButton_Click(object sender, RoutedEventArgs e)
        {
            CPUInfo.getInstance().RunProgramThreadStart("stepinto");
        }

        void OpenfileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openfiledialog = new OpenFileDialog();
            openfiledialog.Filter = "Binary File(*.bin)|*.bin|Source File(*.sbin)|*.sbin";
            openfiledialog.FileOk += new System.ComponentModel.CancelEventHandler(openfiledialog_FileOk);
            openfiledialog.ShowDialog();
            ChildFormControl.getInstance().update();
        }
        void openfiledialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                //CPUInfo.getInstance().setPC(this.updateAddressChoose(CPUInfo.getInstance().load(((OpenFileDialog)sender).FileName)));
                //Debug.Write("aaa");
                this.load(((OpenFileDialog)sender).FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Open File Error!");
                Debug.WriteLine(ex);
            }
        }
        public bool load(String name)
        {
            try
            {
                this.stateShowDirect("Loading file...");
                List<int> ls = CPUInfo.getInstance().load(name);
                int pc = this.updateAddressChoose(ls);
                CPUInfo.getInstance().setPC(pc);
                ChildFormControl.getInstance().update();
                jumptoPC();
                this.stateShowReset();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                MessageBox.Show("Open File Error!");
                this.stateShowReset();
                Debug.WriteLine(ex);
                return false;
            }
        }

        private void setStop(bool a)
        {
            this.StopButton.IsEnabled = a;
            this.StopItem.IsEnabled = a;
            this.RunButton.IsEnabled = !a;
            this.ResetMemoryItem.IsEnabled = !a;
            this.LoadProgramItem.IsEnabled = !a;
            this.SaveMemoryItem.IsEnabled = !a;
        }
        private delegate void setStopdelegate(bool a);
        void MemoryPanel_HaltChangeEvent(object sender, object[] args)
        {
            setStopdelegate fun = new setStopdelegate(setStop);
            if ((byte)args[1] == 1)
                this.Dispatcher.Invoke(fun, true);
            else
                this.Dispatcher.Invoke(fun, false);
        }
       
        void AddressChoose_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.AddressChoose.SelectedIndex != -1)
            {
                String s = ((string)this.AddressChoose.Items[this.AddressChoose.SelectedIndex]);
                this.jump(SmallTool.StringLocationParse(s.Substring(s.Length - 9)));
            }
        }
        public void jump(UInt32 address)
        {
            this.scrollBar.Value = address / 4;
        }
        public void jumptoPC()
        {
            if (SmallTool.InttoUint(CPUInfo.getInstance().getPC()) - this.mpc.getLocation() < this.itemNumber * 4)
            {
                this.mpc.setCurrentLine(SmallTool.InttoUint(CPUInfo.getInstance().getPC()));
            }
            else
            {
                UInt32 l = SmallTool.InttoUint(CPUInfo.getInstance().getPC()) / 4;
                this.scrollBar.Value = l;
                this.mpc.setCurrentLine(l * 4);
            }
        }
        public int updateAddressChoose(List<Int32> address)
        {
            this.AddressChoose.Items.Clear();
            int pcvalue= this.addAddressChoose(address);
            ArrayList breakpoints = CPUInfo.getInstance().getBreakpoints();
            for (int i = 0; i < breakpoints.Count; i++)
                if (!(breakpoints[i] is ConditionBreakpoint))
                    this.AddressChoose.Items.Add("B " + SmallTool.intToHexString((int)breakpoints[i]));
            return pcvalue;
        }
        public int addAddressChoose(List<Int32> address)
        {           
            int ci = address[0];
            int[] ca = new int[ci];
            for (int i = 0; i < ci; i++)
                ca[i] = address[i + 1];
            this.AddressChoose.Items.Add("PC " + SmallTool.intToHexString(address[ci + 1]));
            for (int i = 0; i < ci; i++)
                this.AddressChoose.Items.Add("D " + SmallTool.intToHexString(ca[i]));
            for (int j = ci + 3; j < ci + 3 + address[ci + 2]; j++)
                this.AddressChoose.Items.Add("C " + SmallTool.intToHexString(address[j]));

            if (this.Trap0Switch.Check)
            {
                for (int j = ci + address[ci + 2] + 4; j < address.Count; j++)
                {
                    CPUInfo.getInstance().getBreakpoints().Add(address[j]);
                    //MessageBox.Show((address[j]) + "");
                }
            }
            return address[ci + 1];
        }

        void SetpcButton_Click(object sender, RoutedEventArgs e)
        {
            this.mpc.setPcCurrent();
            ChildFormControl.getInstance().update();

            //this.updateAddressChoose(CPUInfo.getInstance().load("D:\\trap.bin"));
        }
        void BreakpointButton_Click(object sender, RoutedEventArgs e)
        {
            int loca = SmallTool.UinttoInt(this.mpc.getCurrentLint());
            if (CPUInfo.getInstance().getBreakpoints().IndexOf(loca) != -1)
                CPUInfo.getInstance().getBreakpoints().Remove(loca);
            else
                CPUInfo.getInstance().getBreakpoints().Add(loca);
            ChildFormControl.getInstance().update();
        }
        void AddressChoose_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.AddressChoose.Text.Length == 0)
                this.AddressChoose.Text = "Input address";
        }
        void AddressChoose_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.AddressChoose.Text.Equals("Input address"))
                this.AddressChoose.Text = "";
           
        }      
        void AddressChoose_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (this.AddressChoose.Text.IndexOf(' ') != -1 || this.AddressChoose.Text.IndexOf('\t') != -1)
                    {
                        String str = "";
                        for (int i = 0; i < this.AddressChoose.Text.Length; i++)
                            if (this.AddressChoose.Text[i] != ' ' && this.AddressChoose.Text[i] != '\t')
                                str += this.AddressChoose.Text[i];
                        this.AddressChoose.Text = str;
                    }
                    UInt32 value = SmallTool.StringLocationParse(this.AddressChoose.Text);
                    this.jump(value);
                    if (this.AddressChooseChange != null)
                    {
                        this.AddressChooseChange.Invoke(this.AddressChoose, new object[] { true, value });
                    }
                }
                catch (Exception ex)
                {
                    this.AddressChoose.Text = "Input address";
                    if (this.AddressChooseChange != null)
                    {
                        this.AddressChooseChange.Invoke(this.AddressChoose, new object[] { false, 0 });
                    }
                    MessageBox.Show("Invalid Address!");
                }
            }
        }       
        
        void MemoryPanel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
                this.scrollBar.Value += this._itemNumber / 2;
            else
                this.scrollBar.Value -= this._itemNumber / 2;
        }

        void scrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //MessageBox.Show(((UInt32)this.scrollBar.Value) + "");
            Debug.WriteLine(this.scrollBar.Value);
            this.scrollBar.Value = Math.Floor(this.scrollBar.Value);
            this.mpc.setLoction((UInt32)(this.scrollBar.Value*4));
            this.update();
        }

        void MemoryPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                adjustSize((int)MainPanel.ActualHeight);
                this.update();
            }
            this.scrollBar.LargeChange = this._itemNumber;

        }
        
        private void adjustSize(int previousHeight)
        {
            try
            {
                int height = previousHeight;
                itemNumber = height / 19;
                this.ItemPanel.Height = itemNumber * 19;
                int number = this.ItemPanel.Children.Count;
                if (number > this.itemNumber)
                    for (int i = number - 1; i > itemNumber - 1; i--)
                        this.ItemPanel.Children.RemoveAt(i);
                else
                    for (int i = 0; i < itemNumber - number; i++)
                    {
                        this.ItemPanel.Children.Add(new MemoryPanelItem(MemoryPanelItem_AllClick, MemoryPanelItem_SignClick, MemoryPanelItem_ThirdClick));
                    }
                this.scrollBar.Maximum = 1073741823 - this.itemNumber + 1;
     
            }
            catch (Exception ex)
            {
                this.itemNumber = this.ItemPanel.Children.Count;
            }
        }

        void MemoryPanelItem_AllClick(object sender, object[] args)
        {
            this.mpc.setCurrentLine((UInt32)args[0]);
            this.updateChoose(this.mpc.getChoose(this.itemNumber));
        }
        void MemoryPanelItem_SignClick(object sender, object[] args)
        {
            ArrayList breakPoint = CPUInfo.getInstance().getBreakpoints();
            //Debug.WriteLine(SmallTool.UinttoInt((UInt32)args[0]));
            if (breakPoint.IndexOf(SmallTool.UinttoInt((UInt32)args[0])) != -1)
            {
                breakPoint.Remove(SmallTool.UinttoInt((UInt32)args[0]));
                this.AddressChoose.Items.Remove("B " + SmallTool.intToHexString(SmallTool.UinttoInt((UInt32)args[0])));
            }
            else
            {
                breakPoint.Add(SmallTool.UinttoInt((UInt32)args[0]));
                this.AddressChoose.Items.Add("B " + SmallTool.intToHexString(SmallTool.UinttoInt((UInt32)args[0])));            
            }
            ChildFormControl.getInstance().update();
            //for (int i = 0; i < breakPoint.Count; i++)
            //    Debug.Write(breakPoint[i]);
            //Debug.WriteLine("");
        }
        void MemoryPanelItem_ThirdClick(object sender, object[] args)
        {
            this.mpc.setPcCurrent();
            ChildFormControl.getInstance().update();
        }
       
        public void updateValue(StringItem[,] newInfo)
        {
            for (int i = 0; i < this.itemNumber; i++)
            {
                //bool[] change = new bool[5];
                //bool changeb = false;
                //change[0] = change[1] = change[2] = change[3] = change[4] = false;
                //for (int j = 0; j < 5; j++)
                //    if (newInfo[i, j].inf != this.ValueInfo[i, j].inf)
                //    {
                //        change[j] = true;
                //        this.ValueInfo[i, j].inf = newInfo[i, j].inf;
                //        this.ValueInfo[i, j].str = newInfo[i, j].str;
                //        if (j > 0)
                //            changeb = true;
                //    }
                String[] strs = new String[7];
                //for (int j = 0; j < 5; j++)
                //    if (change[j])
                //        strs[j] = ValueInfo[i, j].str;
                //    else
                //        strs[j] = null;
                //if (changeb)
                //{
                //    this.ValueInfo[i, 5] = newInfo[i, 5];
                //    this.ValueInfo[i, 6] = newInfo[i, 6];
                //    strs[5] = this.ValueInfo[i, 5].str;
                //    strs[6] = this.ValueInfo[i, 6].str;
                //}
                //else
                //{
                //    strs[5] = null;
                //    strs[6] = null;
                //}
                //Debug.WriteLine(newInfo[i, 0].str + " " + newInfo[i, 1].str + " " + newInfo[i, 2].str + " " + newInfo[i, 3].str + " " + newInfo[i, 4].str + " " + newInfo[i, 5].str);
                for (int j = 0; j < 7; j++)
                {
                    strs[j] = newInfo[i, j].str;
                }
                    ((MemoryPanelItem)this.ItemPanel.Children[i]).valueUpdate(strs);
            }
            //for (int i = this.itemNumber; i < MaxItemNumber; i++)
            //{
            //    this.ValueInfo[i, 0] = xZero;
            //    this.ValueInfo[i, 1] = bZero;
            //    this.ValueInfo[i, 2] = bZero;
            //    this.ValueInfo[i, 3] = bZero;
            //    this.ValueInfo[i, 4] = bZero;
            //    this.ValueInfo[i, 5] = xZero;
            //    this.ValueInfo[i, 6] = iZero;
            //}
        }
        public void updateLight(bool[,] isLight)
        {
            for (int i = 0; i < this.itemNumber; i++)
                ((MemoryPanelItem)this.ItemPanel.Children[i]).lightUpdate(isLight[i, 0], isLight[i, 1], isLight[i, 2], isLight[i, 3]);
        }
        public void updateSign(int[] signs)
        {
            for (int i = 0; i < this.itemNumber; i++)
                ((MemoryPanelItem)this.ItemPanel.Children[i]).signUpdate(signs[i]);
        }
        public void updateChoose(int s)
        {
            if (this.chooseItem >= this.itemNumber)
                this.chooseItem = -1;
            if (s == -1)
            {
                if (this.chooseItem != -1)
                    ((MemoryPanelItem)this.ItemPanel.Children[this.chooseItem]).IsChoose = false;
            }
            else
            {
                if (this.chooseItem != -1)
                    ((MemoryPanelItem)this.ItemPanel.Children[this.chooseItem]).IsChoose = false;
                ((MemoryPanelItem)this.ItemPanel.Children[s]).IsChoose = true;
            }
            this.chooseItem = s;

        }
        public void update()
        {
           // lock (this)
           // {
                this.updateValue(mpc.getValue(this.itemNumber));
                this.updateSign(mpc.getSign(this.itemNumber));
                this.updateLight(mpc.getLight(this.itemNumber));
                this.updateChoose(mpc.getChoose(this.itemNumber));
           // }
        }


        public Button getButton(String name)
        {
            if (name.Equals("Openfile"))
                return this.OpenfileButton;
            if (name.Equals("Run"))
                return this.RunButton;
            if (name.Equals("Debug"))
                return this.DebugButton;
            if (name.Equals("Stepinto"))
                return this.StepintoButton;
            if (name.Equals("Stepout"))
                return this.StepoutButton;
            if (name.Equals("Stepover"))
                return this.StepoverButton;
            if (name.Equals("Stop"))
                return this.StopButton;
            if (name.Equals("BreakPoint"))
                return this.BreakPointButton;
            if (name.Equals("Setpc"))
                return this.SetpcButton;
            if (name.Equals("Setvalue"))
                return this.SetvalueButton;
            if (name.Equals("Condition"))
                return this.ConditionButton;
            return null;
        }
        public Canvas getViewer(String name)
        {
            if (name.Equals("Merge"))
                return this.MergeViewer;
            if (name.Equals("Register"))
                return this.RegisterViewer;
            if (name.Equals("Console"))
                return this.ConsoleViewer;
            return null;
        }

        public void MoveScollTo(double value)
        {
            this.scrollBar.Value += value;
            this.update();
        }
        //state show
        private object stateContent;
        Thread stateThread;
        private delegate void ObjectFun(object o);
        private void instructionNumberShow(object o)
        {
            this.InstructionsLabel.Content = o + " Instructions Excute";
        }
        public void instructionCount(object o)
        {
            this.Dispatcher.Invoke(new ObjectFun(this.instructionNumberShow), o);
        }
        public void stateReset(object o)
        {
            this.StateLabel.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            this.StateLabel.Content = "Ready";
        }
        public void stateShowRed(object o)
        {
            this.StateLabel.Foreground = new SolidColorBrush(Color.FromRgb(180, 0, 0));
            this.StateLabel.Content = o + "";
        }
        public void stateShow(object o)
        {
            this.StateLabel.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            this.StateLabel.Content = o + "";
        }
        public void stateShowD()
        {
            this.StateLabel.Dispatcher.Invoke(new ObjectFun(this.stateShow), this.stateContent);
        }
        public void stateShowP()
        {
            this.StateLabel.Dispatcher.Invoke(new ObjectFun(this.stateShowRed), this.stateContent);
            Thread.Sleep(2000);
            this.StateLabel.Dispatcher.Invoke(new ObjectFun(this.stateReset),"");
        }
        public void stateShowDirect(object o)
        {
            this.stateContent = o;
            this.stateShowD();
        }
        public void stateShowReset()
        {
            this.stateContent = "Ready";
            this.stateShowD();
            this.instructionCount(0);
        }
        public void breakPause(object o)
        {
            this.stateContent = o;
            if (stateThread != null)
            {
                try
                {
                    stateThread.Abort();
                }
                catch (Exception e) { }
            }
            stateThread = null;
            stateThread = new Thread(new ThreadStart(this.stateShowP));
            stateThread.IsBackground = true;
            stateThread.Start();
        }
        
    }
}
