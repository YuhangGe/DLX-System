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
using System.Threading;

namespace Simulate
{
    /// <summary>
    /// ConsolePanel.xaml 的交互逻辑
    /// </summary>
    public partial class ConsolePanel : UserControl
    {


        private byte ScreenCurrentChar = 0;
        private Thread ScreenStateThr = null;
        private byte KeyboardCurrentChar = 0;
        private Thread KeyboardStateThr = null;

        public ConsolePanel()
        {
            InitializeComponent();
            this.ConsoleShow.BackColor = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            this.ConsoleShow.ForeSize = 14;
            this.ConsoleShow.ForeColor = new SolidColorBrush(Color.FromArgb(255, 220, 220, 220));
            this.ConsoleShow.ConsoleKeyDown += new DLXConsole.ConsoleKeyDownHandler(ConsoleShow_ConsoleKeyDown);
            this.Loaded += new RoutedEventHandler(ConsolePanel_Loaded);
            CPUInfo.getInstance().ValueChangeEvent += new CPUInfo.eventdelegate(ConsolePanel_ValueChangeEvent);
            CPUInfo.getInstance().computer.memory.Accessed += new VM.Memory.deleAccessed(memory_Accessed);
        }

        void memory_Accessed(int addr)
        {
            //if (addr == SmallTool.UinttoInt(SmallTool.StringLocationParse("xFFFF0007")))
            if((UInt32)addr == (UInt32)0xFFFF0007)
            {
                CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901760), 0);
                CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901761), 0);
                CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901762), 0);
                CPUInfo.getInstance().computer.memory[SmallTool.UinttoInt(4294901763)] &= 254;
                //CPUInfo.getInstance().computer.KBSR[0] = 0;
            }
        }

        /* 取消了DLX控制台下方的显示器键盘状态显示.
         * 这是由于显示状态使用的机制是每次状态更新都用一个新的线程来控制,并把旧线程用Abort函数终止掉
         * 每次都启动一个新线程的代价已经很高.而性能分析显示调用Abort函数的时间是启动一个新线程的20倍左右
         * 而VM的执行将会等待Abort函数的执行,给模拟器的速度带来很大不利影响
         * 目前认为这个状态显示不是很必要,因为指令执行速度很快,每秒可以输出数十个字符,每个状态显示时间内
         * 是很难看清的.
         * Shore Ray */
        void ConsolePanel_ValueChangeEvent(object sender, object[] args)
        {
            /* 此函数是性能攸关的函数,这里用String作地址,再转换成int型数据会产生显著的性能开销
             * Shore Ray */
            //if (((int)args[0]) == SmallTool.UinttoInt(SmallTool.StringLocationParse("xFFFF000F")))
            if (((int)args[0]) == SmallTool.UinttoInt(0xFFFF000F))
            {
                CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901768), 0);
                CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901769), 0);
                CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901770), 0);
                CPUInfo.getInstance().computer.memory[SmallTool.UinttoInt(4294901771)] &= 254;
                //CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901771), 0);
                //CPUInfo.getInstance().computer.DSR[0] = 0;
                this.ConsoleShow.AppendChar((char)CPUInfo.getInstance().computer.memory[(int)args[0]]);
                //this.ScreenStateThread(CPUInfo.getInstance().getMemoryValue(SmallTool.StringLocationParse("xFFFF000F")));                
                CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901768), 0);
                CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901769), 0);
                CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901770), 0);
                CPUInfo.getInstance().computer.memory[SmallTool.UinttoInt(4294901771)] |= 1;
                //CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901771), 1);
                //CPUInfo.getInstance().computer.DSR[0] = 1;
            }
        }
        void ConsolePanel_Loaded(object sender, RoutedEventArgs e)
        {
            this.ConsoleShow.AppendString("DLX Simulator Console[1.0.0.0]\nAll Rights Reserved 2010(©) Team209\nSoftware Institute of Nanjing University\n\n\n");
            CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901768), 0);
            CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901769), 0);
            CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901770), 0);
            CPUInfo.getInstance().computer.memory[SmallTool.UinttoInt(4294901771)] |= 1;
            //CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901771), 1);
            //CPUInfo.getInstance().computer.DSR[0] = 1;
        }

        /* 取消了DLX控制台下方的显示器键盘状态显示.
        * 这是由于显示状态使用的机制是每次状态更新都用一个新的线程来控制,并把旧线程用Abort函数终止掉
        * 每次都启动一个新线程的代价已经很高.而性能分析显示调用Abort函数的时间是启动一个新线程的20倍左右
        * 而VM的执行将会等待Abort函数的执行,给模拟器的速度带来很大不利影响
        * 目前认为这个状态显示不是很必要,因为指令执行速度很快,每秒可以输出数十个字符,每个状态显示时间内
        * 是很难看清的.
        * Shore Ray */
        void ConsoleShow_ConsoleKeyDown(char key)
        {
            //if (((CPUInfo.getInstance().getMemoryValue(SmallTool.StringLocationParse("xFFFF0003"))) & 1) != 0)
            if (((CPUInfo.getInstance().getMemoryValue((UInt32)0xFFFF0003)) & 1) != 0)
                return;
            else
            {
                CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901764), 0);
                CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901765), 0);
                CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901766), 0);
                CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901767), (byte)key);
                CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901760), 0);
                CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901761), 0);
                CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901762), 0);
                CPUInfo.getInstance().computer.memory[SmallTool.UinttoInt(4294901763)] |= 1;
                //CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901763), 1);
                //CPUInfo.getInstance().computer.KBSR[0] = 1;
                //this.KeyboardStateThread((byte)key);
                //Debug.WriteLine("Keyboard:" + key);
            }
        }
       


       

        //状态栏显示
        private delegate void NullParameterFun();
        private void ScreenStateFun()
        {
            this.ScreenState.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            if (this.ScreenCurrentChar != '\n')
                this.ScreenState.Content = "Screen : '" + (char)this.ScreenCurrentChar + "'(" + this.ScreenCurrentChar + ")";
            else
                this.ScreenState.Content = "Screen : '\\n'(" + this.ScreenCurrentChar + ")";
        }
        private void ScreenStateReset()
        {
            this.ScreenState.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            this.ScreenState.Content = "Screen Ready";
        }
        private void ScreenStateShow()
        {
            
            this.Dispatcher.Invoke(new NullParameterFun(this.ScreenStateFun));
            Thread.Sleep(800);
            this.Dispatcher.Invoke(new NullParameterFun(this.ScreenStateReset));
             
        }                      
        /* 这个显示状态的函数现在不使用了.这是由于Abort函数的执行非常缓慢,给模拟器运行速度
         * 带来很大不利影响.
         * 现在直接取消了这个功能.
         * 如果要恢复此功能,应当修改此函数的实现.
         * 建议使用单一线程处理所有状态更新请求.没有状态更新时此线程在事件上等待即可.
         * Shore Ray */
        private void ScreenStateThread(byte c)
        {
            /**/
            this.ScreenCurrentChar = c;
            if (this.ScreenStateThr != null)
                this.ScreenStateThr.Abort();
            this.ScreenStateThr = new Thread(new ThreadStart(this.ScreenStateShow));
            this.ScreenStateThr.IsBackground = true;
            this.ScreenStateThr.Start();
             
        }
        private void KeyboardStateFun()
        {
            this.KeyboardState.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            if (this.KeyboardCurrentChar != '\n')
                this.KeyboardState.Content = "Keyboard : '" + (char)this.KeyboardCurrentChar + "'(" + this.KeyboardCurrentChar + ")";
            else
                this.KeyboardState.Content = "Keyboard : '\\n'(" + this.KeyboardCurrentChar + ")";
        }
        private void KeyboardStateReset()
        {
            this.KeyboardState.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            this.KeyboardState.Content = "Keyboard Ready";
        }
        private void KeyboardStateShow()
        {
            this.Dispatcher.Invoke(new NullParameterFun(this.KeyboardStateFun));
            Thread.Sleep(800);
            this.Dispatcher.Invoke(new NullParameterFun(this.KeyboardStateReset));
        }
        /* 这个显示状态的函数现在不使用了.这是由于Abort函数的执行非常缓慢,给模拟器运行速度
        * 带来很大不利影响.
        * 现在直接取消了这个功能.
        * 如果要恢复此功能,应当修改此函数的实现.
        * 建议使用单一线程处理所有状态更新请求.没有状态更新时此线程在事件上等待即可.
        * Shore Ray */
        private void KeyboardStateThread(byte c)
        {
            /* */
            this.KeyboardCurrentChar = c;
            if (this.KeyboardStateThr != null)
                this.KeyboardStateThr.Abort();
            this.KeyboardStateThr = new Thread(new ThreadStart(this.KeyboardStateShow));
            this.KeyboardStateThr.IsBackground = true;
            this.KeyboardStateThr.Start();
            
        }
        

        //void TextField_KeyUp(object sender, KeyEventArgs e)
        //{
        //    if (e.Key.Equals(Key.LeftShift) || e.Key.Equals(Key.RightShift))
        //        this.shiftDown = false;

        //}
        ////void TextField_KeyDown(object sender, KeyEventArgs e)
        ////{
        ////    if (e.Key.Equals(Key.LeftShift) || e.Key.Equals(Key.RightShift))
        ////        this.shiftDown = true;
        ////    KepPressDone(keyParse(e.Key));
        ////    //this.TextField.AppendText(e.Key.ToString());
        ////}
   
        //private char keyParse(Key eKey)
        //{
        //    switch (eKey)
        //    {                   
        //        case Key.A:
        //            return (shiftDown ? 'A' : 'a');
        //        case Key.B:
        //            return (shiftDown ? 'B' : 'b');
        //        case Key.C:
        //            return (shiftDown ? 'C' : 'c');
        //        case Key.D:
        //            return (shiftDown ? 'D' : 'd');
        //        case Key.E:
        //            return (shiftDown ? 'E' : 'e');
        //        case Key.F:
        //            return (shiftDown ? 'F' : 'f');
        //        case Key.G:
        //            return (shiftDown ? 'G' : 'g');
        //        case Key.H:
        //            return (shiftDown ? 'H' : 'h');
        //        case Key.I:
        //            return (shiftDown ? 'I' : 'i');
        //        case Key.J:
        //            return (shiftDown ? 'J' : 'j');
        //        case Key.K:
        //            return (shiftDown ? 'K' : 'k');
        //        case Key.L:
        //            return (shiftDown ? 'L' : 'l');
        //        case Key.M:
        //            return (shiftDown ? 'M' : 'm');
        //        case Key.N:
        //            return (shiftDown ? 'N' : 'n');
        //        case Key.O:
        //            return (shiftDown ? 'O' : 'o');
        //        case Key.P:
        //            return (shiftDown ? 'P' : 'p');
        //        case Key.Q:
        //            return (shiftDown ? 'Q' : 'q');
        //        case Key.R:
        //            return (shiftDown ? 'R' : 'r');
        //        case Key.S:
        //            return (shiftDown ? 'S' : 's');
        //        case Key.T:
        //            return (shiftDown ? 'T' : 't');
        //        case Key.U:
        //            return (shiftDown ? 'U' : 'u');
        //        case Key.V:
        //            return (shiftDown ? 'V' : 'v');
        //        case Key.W:
        //            return (shiftDown ? 'W' : 'w');
        //        case Key.X:
        //            return (shiftDown ? 'X' : 'x');
        //        case Key.Y:
        //            return (shiftDown ? 'Y' : 'y');
        //        case Key.Z:
        //            return (shiftDown ? 'Z' : 'z');
        //        case Key.D0:
        //            return (!shiftDown ? '0' : ')');
        //        case Key.D1:
        //            return (!shiftDown ? '1' : '!');
        //        case Key.D2:
        //            return (!shiftDown ? '2' : '@');
        //        case Key.D3:
        //            return (!shiftDown ? '3' : '#');
        //        case Key.D4:
        //            return (!shiftDown ? '4' : '$');
        //        case Key.D5:
        //            return (!shiftDown ? '5' : '%');
        //        case Key.D6:
        //            return (!shiftDown ? '6' : '^');
        //        case Key.D7:
        //            return (!shiftDown ? '7' : '&');
        //        case Key.D8:
        //            return (!shiftDown ? '8' : '*');
        //        case Key.D9:
        //            return (!shiftDown ? '9' : '(');
        //        case Key.OemTilde:
        //            return (shiftDown ? '~' : '`');
        //        case Key.OemMinus:
        //            return (shiftDown ? '_' : '-');
        //        case Key.OemPlus:
        //            return (shiftDown ? '+' : '=');
        //        case Key.OemOpenBrackets:
        //            return (shiftDown ? '{' : '[');
        //        case Key.OemCloseBrackets:
        //            return (shiftDown ? '}' : ']');
        //        case Key.OemPipe:
        //            return (shiftDown ? '|' : '\\');
        //        case Key.OemSemicolon:
        //            return (shiftDown ? ':' : ';');
        //        case Key.OemQuotes:
        //            return (shiftDown ? '\"' : '\'');
        //        case Key.OemComma:
        //            return (shiftDown ? '<' : ',');
        //        case Key.OemPeriod:
        //            return (shiftDown ? '>' : '.');
        //        case Key.OemQuestion:
        //            return (shiftDown ? '?' : '/');
        //        default:
        //            return '\0';

        //    }
        //}
        //private void KepPressDone(char c)
        //{
        //    if(c!='\0')
        //        this.TextField.AppendText(c+"");
        //}
    }
}
