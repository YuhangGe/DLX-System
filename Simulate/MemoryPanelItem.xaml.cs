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
using System.Globalization;

namespace Simulate
{
    /// <summary>
    /// MemoryPanelItem.xaml 的交互逻辑
    /// </summary>
    public partial class MemoryPanelItem : UserControl
    {
        private List<Label> byteLabels;
        private bool _isChoose;
        private bool [] _isLight=new bool[4];
        private bool light0
        {
            get { return _isLight[0]; }
            set
            {
                this._isLight[0] = value;
                if (value)
                    Byte1Lebel.Style = (Style)this.FindResource("ByteLight");
                else
                    Byte1Lebel.Style = (Style)this.FindResource("ByteCommon");
            }
        }
        private bool light1
        {
            get { return _isLight[1]; }
            set
            {
                this._isLight[1] = value;
                if (value)
                    Byte2Lebel.Style = (Style)this.FindResource("ByteLight");
                else
                    Byte2Lebel.Style = (Style)this.FindResource("ByteCommon");
            }
        }
        private bool light2
        {
            get { return _isLight[2]; }
            set
            {
                this._isLight[2] = value;
                if (value)
                    Byte3Lebel.Style = (Style)this.FindResource("ByteLight");
                else
                    Byte3Lebel.Style = (Style)this.FindResource("ByteCommon");
            }
        }
        private bool light3
        {
            get { return _isLight[3]; }
            set
            {
                this._isLight[3] = value;
                if (value)
                    Byte4Lebel.Style = (Style)this.FindResource("ByteLight");
                else
                    Byte4Lebel.Style = (Style)this.FindResource("ByteCommon");
            }
        }
        private int _sign;
        private int Sign
        {
            get { return _sign; }
            set
            {
                _sign = value;
                switch (value)
                {
                    case 0: 
                        this.Diamond.Visibility = Visibility.Visible;
                        this.Circle.Visibility = Visibility.Hidden;
                        this.Arrow.Visibility = Visibility.Hidden;
                        break;
                    case 1:
                        this.Diamond.Visibility = Visibility.Hidden;
                        this.Circle.Visibility = Visibility.Visible;
                        this.Arrow.Visibility = Visibility.Hidden;
                        break;
                    case 2:
                        this.Diamond.Visibility = Visibility.Hidden;
                        this.Circle.Visibility = Visibility.Hidden;
                        this.Arrow.Visibility = Visibility.Visible;
                        break;
                    case 3:
                        this.Diamond.Visibility = Visibility.Hidden;
                        this.Circle.Visibility = Visibility.Visible;
                        this.Arrow.Visibility = Visibility.Visible;
                        break;
                    default:
                        this.Diamond.Visibility = Visibility.Visible;
                        this.Circle.Visibility = Visibility.Hidden;
                        this.Arrow.Visibility = Visibility.Hidden;
                        break;
                }
            }
        }

        public bool IsChoose
        {
            get
            {
                return _isChoose;
            }
            set
            {
                _isChoose = value;
                if (value)
                {
                    this.BackRectangle.Stroke = (Brush)this.FindResource("StrokeLight");
                }
                else
                {
                    this.BackRectangle.Stroke = (Brush)this.FindResource("StrokeCommon");
                }
            }
        }

        public delegate void eventdelegate(object sender, object[] args);
        public event eventdelegate AllClick,DiamondClick,DoubleClick;

        public MemoryPanelItem(eventdelegate AllClickHander,eventdelegate DiamondClickHander,eventdelegate ThirdClickHander)
        {           
            InitializeComponent();            
            init();
            AllClick = AllClickHander;
            DiamondClick = DiamondClickHander;
            DoubleClick = ThirdClickHander;
        }
        private void init()
        {
            byteLabels = new List<Label>();
            byteLabels.Add(this.Byte1Lebel);
            byteLabels.Add(this.Byte2Lebel);
            byteLabels.Add(this.Byte3Lebel);
            byteLabels.Add(this.Byte4Lebel);

            this.SizeChanged += new SizeChangedEventHandler(MemoryPanelItem_SizeChanged);
            this.ValueCanvas.MouseDown += new MouseButtonEventHandler(MainCanvas_MouseDown);
            this.InstructCanvas.MouseDown += new MouseButtonEventHandler(MainCanvas_MouseDown);
            this.Diamond.MouseDown += new MouseButtonEventHandler(Sign_MouseDown);
            this.Arrow.MouseDown+=new MouseButtonEventHandler(Sign_MouseDown);
            this.Circle.MouseDown+=new MouseButtonEventHandler(Sign_MouseDown);
            this.ValueCanvas.ContextMenu = makeContextMenu();

            IsChoose = false;
            light0 = light1 = light2 = light3 = false;
            
        }
        private ContextMenu makeContextMenu()
        {
            ContextMenu cm = new ContextMenu();
            MenuItem m1 = new MenuItem();
            MenuItem m2 = new MenuItem();
            MenuItem m3 = new MenuItem();
            m1.Header = "Set PC";
            m2.Header = "Breakpoint";
            m3.Header = "Set Value";
            cm.Items.Add(m1);
            cm.Items.Add(m2); 
            cm.Items.Add(m3);
            m1.Click += new RoutedEventHandler(m1_Click);
            m2.Click += new RoutedEventHandler(m2_Click);
            m3.Click += new RoutedEventHandler(m3_Click);
            return cm;
        }

        void m3_Click(object sender, RoutedEventArgs e)
        {
            ChildFormControl.getInstance().SetValueForm((string)this.Location.Content);
        }

        void m2_Click(object sender, RoutedEventArgs e)
        {
            SignClickWork();
        }

        void m1_Click(object sender, RoutedEventArgs e)
        {
            setPC();
        }


        void Sign_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SignClickWork();
        }

        void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
                AllClickWork(e);
            else if (e.ClickCount == 2)
                SignClickWork();
            else if (e.ClickCount == 3)
                setPC();
        }     

        public void valueUpdate(String[] str)
        {
            if (str[0] != null)
                this.Location.Content = str[0];
            for (int i = 0; i < 4; i++)
                if (str[1 + i] != null)
                    this.byteLabels[i].Content = str[1 + i];
            if (str[5] != null)
                this.HexValueLabel.Content = str[5];
            if (str[5] != null)
                this.InstructionLabel.Content = str[6];
        }
        public void signUpdate(int s)
        {
            if (s != this.Sign)
                Sign = s;
        }
        public void lightUpdate(bool b1, bool b2, bool b3, bool b4)
        {
            if (b1 != light0)
                light0 = b1;
            if (b2 != light1)
                light1 = b2;
            if (b3 != light2)
                light2 = b3; 
            if (b4 != light3)
                light3 = b4;
        }
        
        void MemoryPanelItem_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double w = e.NewSize.Width - 473;
            if (w <= 0)
            {
                this.InstructionLabel.Width = 0;
                this.ILine.Visibility = Visibility.Hidden;
                
            }
            else
            {
                this.InstructionLabel.Width = w;
                this.ILine.Visibility = Visibility.Visible;
            }
            if (e.NewSize.Width - 378 > 0)
                this.HexValueLabel.Width = e.NewSize.Width - 378;
            else
                this.HexValueLabel.Width = 0;
        }
        private void AllClickWork(MouseButtonEventArgs e)
        {
            if (AllClick != null)
                AllClick(this, new object[] { UInt32.Parse(((string)this.Location.Content).Substring(1, 8), NumberStyles.HexNumber),e });
        }
        private void SignClickWork()
        {
            if (DiamondClick != null)
                DiamondClick(this, new object[] { UInt32.Parse(((string)this.Location.Content).Substring(1, 8), NumberStyles.HexNumber) });
        }
        private void setPC()
        {
            if(DoubleClick!=null)
                DoubleClick(this, new object[] { UInt32.Parse(((string)this.Location.Content).Substring(1, 8), NumberStyles.HexNumber) });
        }
    }
}
