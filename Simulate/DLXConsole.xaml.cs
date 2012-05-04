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
using System.Runtime.InteropServices;
namespace Simulate
{
    /// <summary>
    /// DLXConsole.xaml 的交互逻辑
    /// </summary>
    public partial class DLXConsole : UserControl
    {
        public DLXConsole()
        {
            InitTable();
            InitializeComponent();
            this.Focusable = true;
            this.KeyDown += new KeyEventHandler(DealKeyDown);
            //this.ConsoleKeyDown += new ConsoleKeyDownHandler(TestDealConsoleKeyDown);

            scrollViewer.Content = cc;
        }
        
        [DllImport("user32")]
        public static extern int GetKeyboardState(byte[] pbKeyState);
        private const int VK_SHIFT = 0x10;
        private const int VK_CAPITAL = 0x14;
        private const int VK_LSHIFT = 0xa0;
        private const int VK_RSHIFT = 0xa1;
        private const int VK_CONTROL = 0x11;
        private struct KeyBoardState
        {
            public bool IsCapsLock;
            public bool IsShift;
            public bool IsControl;
        }
        private KeyBoardState GetKeyState()
        {
            KeyBoardState rtn = new KeyBoardState();
            byte[] states = new byte[256];
            GetKeyboardState(states);
            if ((states[VK_CAPITAL] & 0x1) != 0)//最低位不为0说明激活
                rtn.IsCapsLock = true;
            else
                rtn.IsCapsLock = false;
            if ((states[VK_SHIFT] & 0x80) != 0 || (states[VK_LSHIFT] & 0x80) != 0 || (states[VK_RSHIFT] & 0x80) != 0)//最高不为0说明按下
                rtn.IsShift = true;
            else
                rtn.IsShift = false;
            if ((states[VK_CONTROL] & 0x80) != 0)//最高不为0说明按下
                rtn.IsControl = true;
            else
                rtn.IsControl = false;
            return rtn;
        }

        public delegate void ConsoleKeyDownHandler(char key);
        public event ConsoleKeyDownHandler ConsoleKeyDown = null;
        public bool Flash
        {
            get
            {
                return cc.Flash;
            }
            set
            {
                cc.Flash = value;
            }
        }
        private ConsoleCanvas cc = new ConsoleCanvas();
   
        public Color HighLightColor
        {
            get { return cc.HighLightColor; }
            set { cc.HighLightColor = value; }
        }
        public double HighLightOpacity
        {
            get { return cc.HighLightOpacity; }
            set { cc.HighLightOpacity = value; }
        }
        public void AppendChar(char c)
        {
            cc.AppendChar(c);

        }
        public void AppendString(string msg)
        {
            foreach (char c in msg)
                cc.AppendChar(c);
        }
        public double CursorThickness
        {
            get { return cc.CursorThickness; }
            set { cc.CursorThickness = value; }
        }
        public double ForeSize
        {
            get { return cc.FontSize; }
            set { cc.FontSize = value; }
        }
        public Brush ForeColor
        {
            get { return cc.Foreground; }
            set { cc.Foreground = value; this.Foreground = value; }
        }
        public Brush BackColor
        {
            get { return cc.Background; }
            set { cc.Background = value; this.Background = value; }
        }
        public string FontName
        {
            get { return cc.FontName; }
            set { cc.FontName = value; }
        }
        private void DealKeyDown(object sender, KeyEventArgs e)
        {
            KeyBoardState state = GetKeyState();
            if (state.IsControl)
            {
                if (keyToChar.ContainsKey(e.Key))
                {
                    if (keyToChar[e.Key][0] == 'c')
                    {
                        cc.DoCopy();
                        return;
                    }
                    else if (keyToChar[e.Key][0] == 'l')
                    {
                        cc.Clear();
                        return;
                    }
                }
            }
            if (this.ConsoleKeyDown != null)
            {
                if (keyToChar.ContainsKey(e.Key))
                {
                    char[] keys = keyToChar[e.Key];

                    char pass_key = keys[0];
                    if (state.IsShift)
                    {
                        if (char.IsLetter(pass_key))
                        {
                            if (!state.IsCapsLock)
                                pass_key = keys[1];
                        }
                        else
                        {
                            if (keys[1] != '\0')
                                pass_key = keys[1];
                        }
                    }
                    else
                    {
                        if (char.IsLetter(pass_key))
                        {
                            if (state.IsCapsLock)
                                pass_key = keys[1];
                        }
                    }
                    this.ConsoleKeyDown.Invoke(pass_key);
                }

            }
        }

        //private void TestDealConsoleKeyDown(char key)
        //{
        //    cc.AppendChar(key);
        //}
        private Dictionary<Key, char[]> keyToChar = new Dictionary<Key, char[]>();
        private void InitTable()
        {

            keyToChar.Add(Key.Back, new char[2] { '\b', '\0' });
            keyToChar.Add(Key.Enter, new char[2] { '\n', '\0' });
            keyToChar.Add(Key.Space, new char[2] { ' ', '\0' });
            //                keyToChar.Add(Key.Tab, new char[2] { '\t', '\0' });
            keyToChar.Add(Key.A, new char[2] { 'a', 'A' });
            keyToChar.Add(Key.B, new char[2] { 'b', 'B' });
            keyToChar.Add(Key.C, new char[2] { 'c', 'C' });
            keyToChar.Add(Key.D, new char[2] { 'd', 'D' });
            keyToChar.Add(Key.E, new char[2] { 'e', 'E' });
            keyToChar.Add(Key.F, new char[2] { 'f', 'F' });
            keyToChar.Add(Key.G, new char[2] { 'g', 'G' });
            keyToChar.Add(Key.H, new char[2] { 'h', 'H' });
            keyToChar.Add(Key.I, new char[2] { 'i', 'I' });
            keyToChar.Add(Key.J, new char[2] { 'j', 'J' });
            keyToChar.Add(Key.K, new char[2] { 'k', 'K' });
            keyToChar.Add(Key.L, new char[2] { 'l', 'L' });
            keyToChar.Add(Key.M, new char[2] { 'm', 'M' });
            keyToChar.Add(Key.N, new char[2] { 'n', 'N' });
            keyToChar.Add(Key.O, new char[2] { 'o', 'O' });
            keyToChar.Add(Key.P, new char[2] { 'p', 'P' });
            keyToChar.Add(Key.Q, new char[2] { 'q', 'Q' });
            keyToChar.Add(Key.R, new char[2] { 'r', 'R' });
            keyToChar.Add(Key.S, new char[2] { 's', 'S' });
            keyToChar.Add(Key.T, new char[2] { 't', 'T' });
            keyToChar.Add(Key.U, new char[2] { 'u', 'U' });
            keyToChar.Add(Key.V, new char[2] { 'v', 'V' });
            keyToChar.Add(Key.W, new char[2] { 'w', 'W' });
            keyToChar.Add(Key.X, new char[2] { 'x', 'X' });
            keyToChar.Add(Key.Y, new char[2] { 'y', 'Y' });
            keyToChar.Add(Key.Z, new char[2] { 'z', 'Z' });
            keyToChar.Add(Key.D0, new char[2] { '0', ')' });
            keyToChar.Add(Key.D1, new char[2] { '1', '!' });
            keyToChar.Add(Key.D2, new char[2] { '2', '@' });
            keyToChar.Add(Key.D3, new char[2] { '3', '#' });
            keyToChar.Add(Key.D4, new char[2] { '4', '$' });
            keyToChar.Add(Key.D5, new char[2] { '5', '%' });
            keyToChar.Add(Key.D6, new char[2] { '6', '^' });
            keyToChar.Add(Key.D7, new char[2] { '7', '&' });
            keyToChar.Add(Key.D8, new char[2] { '8', '*' });
            keyToChar.Add(Key.D9, new char[2] { '9', '(' });
            keyToChar.Add(Key.OemMinus, new char[2] { '-', '_' });
            keyToChar.Add(Key.OemPlus, new char[2] { '+', '=' });
            keyToChar.Add(Key.NumPad0, new char[2] { '0', '\0' });
            keyToChar.Add(Key.NumPad1, new char[2] { '1', '\0' });
            keyToChar.Add(Key.NumPad2, new char[2] { '2', '\0' });
            keyToChar.Add(Key.NumPad3, new char[2] { '3', '\0' });
            keyToChar.Add(Key.NumPad4, new char[2] { '4', '\0' });
            keyToChar.Add(Key.NumPad5, new char[2] { '5', '\0' });
            keyToChar.Add(Key.NumPad6, new char[2] { '6', '\0' });
            keyToChar.Add(Key.NumPad7, new char[2] { '7', '\0' });
            keyToChar.Add(Key.NumPad8, new char[2] { '8', '\0' });
            keyToChar.Add(Key.NumPad9, new char[2] { '9', '\0' });
            keyToChar.Add(Key.Multiply, new char[2] { '*', '\0' });
            keyToChar.Add(Key.Divide, new char[2] { '/', '\0' });
            keyToChar.Add(Key.Add, new char[2] { '+', '\0' });
            keyToChar.Add(Key.Subtract, new char[2] { '-', '\0' });
            keyToChar.Add(Key.OemPeriod, new char[2] { '.', '>' });
            keyToChar.Add(Key.OemComma, new char[2] { ',', '<' });
            keyToChar.Add(Key.OemTilde, new char[2] { '`', '~' });
            keyToChar.Add(Key.OemQuestion, new char[2] { '/', '?' });
            keyToChar.Add(Key.OemQuotes, new char[2] { '\'', '"' });
            keyToChar.Add(Key.OemSemicolon, new char[2] { ';', ':' });
            keyToChar.Add(Key.OemPipe, new char[2] { '\\', '|' });


        }
        public void Clear()
        {
            cc.Clear();
        }
        private void scrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            cc.ReSize();
        }

        private void scrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            scrollViewer.SizeChanged+=new SizeChangedEventHandler(scrollViewer_SizeChanged);
        }
    }
}
