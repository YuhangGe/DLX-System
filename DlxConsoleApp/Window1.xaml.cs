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
using System.Windows.Interop;

namespace DlxConsoleApp
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        private DlxConsole dlxConsole1 = new DlxConsole();
        public Window1()
        {
            InitializeComponent();
            dlxConsole1.BackColor = Brushes.Black;
            dlxConsole1.Height = double.NaN;
            dlxConsole1.Width = double.NaN;
            grid1.Children.Add(dlxConsole1);

           this.Loaded+=new RoutedEventHandler(Window1_Loaded);
        }



        private IntPtr m_hImc;
        private IntPtr m_handle;
        public const int WM_IME_SETCONTEXT = 0x0281;

        private const int WM_IME_CHAR = 0x0286;

        private const int WM_CHAR = 0x0102;

        private const int WM_IME_COMPOSITION = 0x010F;

        private const int GCS_COMPSTR = 0x0008;

        private const int HC_ACTION = 0;

        private const int PM_REMOVE = 0x0001;
        private int GCS_RESULTSTR = 0x0800;
        [DllImport("Imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("Imm32.dll")]
        public static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr
        hIMC);
        [DllImport("imm32.dll")]
        static extern int ImmGetCompositionString(IntPtr hIMC, int dwIndex, StringBuilder lPBuf, int dwBufLen);

        public void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            m_handle = new WindowInteropHelper(this).Handle;
            m_hImc = ImmGetContext(m_handle);
            HwndSource source = HwndSource.FromHwnd(m_handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_IME_SETCONTEXT && wParam.ToInt32() == 1)
            {
                ImmAssociateContext(m_handle, m_hImc);

            }
            else if (msg == WM_IME_CHAR)
            {
                StringBuilder str = new StringBuilder();
                int size = ImmGetCompositionString(m_hImc, GCS_COMPSTR, null, 0);
                size += sizeof(Char);
                ImmGetCompositionString(m_hImc, GCS_RESULTSTR, str, size);
                dlxConsole1.AppendChar(str[0]);

            }

            return IntPtr.Zero;
        }

  

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            dlxConsole1.AppendString("DLX Console[1.0.0.0]\nAll Rights Reserved 2010(©) Team 209\nSoftware Institute of Nanjing University\n\n\n");
        //    dlxConsole1.AppendString("hello");
            dlxConsole1.Focus();
        }

  
      
    }
}
