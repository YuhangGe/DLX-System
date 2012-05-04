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
        private List<Label> valueLabels;
        private List<TextBox> valueInputs;
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
            this.MouseDown += new MouseButtonEventHandler(MemoryPanelItem_MouseDown);
            
            valueLabels = new List<Label>();
            valueLabels.Add(this.Byte1Lebel);
            valueLabels.Add(this.Byte2Lebel);
            valueLabels.Add(this.Byte3Lebel);
            valueLabels.Add(this.Byte4Lebel);
            valueLabels.Add(this.HexValueLabel);

            valueInputs = new List<TextBox>();
            valueInputs.Add(this.Byte1Input);
            valueInputs.Add(this.Byte2Input);
            valueInputs.Add(this.Byte3Input);
            valueInputs.Add(this.Byte4Input);
            valueInputs.Add(this.HexValueInput);

            this.SizeChanged += new SizeChangedEventHandler(MemoryPanelItem_SizeChanged);
            this.ValueCanvas.MouseDown += new MouseButtonEventHandler(MainCanvas_MouseDown);
            this.InstructCanvas.MouseDown += new MouseButtonEventHandler(MainCanvas_MouseDown);
            this.Diamond.MouseDown += new MouseButtonEventHandler(Sign_MouseDown);
            this.Arrow.MouseDown+=new MouseButtonEventHandler(Sign_MouseDown);
            this.Circle.MouseDown+=new MouseButtonEventHandler(Sign_MouseDown);
            this.ValueCanvas.ContextMenu = makeContextMenu();

            for (int i = 0; i < 5; i++)
            {
                valueLabels[i].MouseDoubleClick += new MouseButtonEventHandler(ByteLebel_MouseDoubleClick);
                valueInputs[i].LostFocus += new RoutedEventHandler(ByteInput_LostFocus);
                //valueInputs[i].KeyDown += new KeyEventHandler(MemoryPanelItem_KeyDown);
                valueInputs[i].AddHandler(TextBox.KeyDownEvent, new KeyEventHandler(MemoryPanelItem_KeyDown), true);
            }
           
            IsChoose = false;
            light0 = light1 = light2 = light3 = false;
            
        }
       
        void MemoryPanelItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
        }

        void MemoryPanelItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.EnsureByteInput((TextBox)sender);
            if (e.Key == Key.Tab)
            {
                this.ShowInput((this.valueInputs.IndexOf((TextBox)sender) + 1) % 5);
                e.Handled = true;
            }
            if(e.Key==Key.RightCtrl)
                this.ShowInput((this.valueInputs.IndexOf((TextBox)sender) + 1) % 5);
            if (e.Key == Key.LeftCtrl)
            {
                int index = this.valueInputs.IndexOf((TextBox)sender) - 1;
                index = index == -1 ? 4 : index;
                this.ShowInput(index);
            }
            if (e.Key == Key.Down)
            {
                this.EnsureByteInput((TextBox)sender);
                ChildFormControl.getInstance().getMemoryPanel().MoveScollTo(1);
                this.ShowInput(valueInputs.IndexOf((TextBox)sender));                
                //ChildFormControl.getInstance().update();
            }
            if (e.Key == Key.Up)
            {
                this.EnsureByteInput((TextBox)sender);
                ChildFormControl.getInstance().getMemoryPanel().MoveScollTo(-1);
                this.ShowInput(valueInputs.IndexOf((TextBox)sender));  
            }
        }

        bool SetValue(String stri,UInt32 index)
        {
            String str = SmallTool.StringStardard(stri);
            try
            {
                if (index != 4)
                {
                    byte value = SmallTool.StringValue8Parse(str);
                    int location = SmallTool.UinttoInt(SmallTool.StringLocationParse((String)this.Location.Content) + index);
                    CPUInfo.getInstance().setMemoryValue(location, value);
                    return true;
                }
                else
                {
                    UInt32 value = SmallTool.StringLocationParse(str);
                    for (UInt32 i = 3; i >= 0 && i <= 3; i--)
                    {
                        int l = SmallTool.UinttoInt(SmallTool.StringLocationParse((String)this.Location.Content) + i);
                        byte b = (byte)(value % 256);
                        CPUInfo.getInstance().setMemoryValue(l, b);
                        value = value / 256;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                ChildFormControl.getInstance().getMemoryPanel().breakPause("Wrong Value!");
                //MessageBox.Show("Wrong value!");
                this.valueInputs[(int)(index)].Text = (String)this.valueLabels[(int)index].Content;
                return false;
            }
        }
        void EnsureByteInput(TextBox tb)
        {
            SetValue(tb.Text, (UInt32)this.valueInputs.IndexOf(tb));

            int index = this.valueInputs.IndexOf(tb);
            this.valueInputs[index].Visibility = Visibility.Hidden;
            this.valueLabels[index].Visibility = Visibility.Visible;
            ChildFormControl.getInstance().update();

        }
        void ByteInput_LostFocus(object sender, RoutedEventArgs e)
        {
            this.EnsureByteInput((TextBox)sender);
        }
        public void ShowInput(int index)
        {
            this.valueInputs[index].Text = (String)this.valueLabels[index].Content;
            this.valueInputs[index].Visibility = Visibility.Visible;
            this.valueLabels[index].Visibility = Visibility.Hidden;
            this.valueInputs[index].Focus();
            this.valueInputs[index].SelectAll();
        }
        void ByteLebel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int index = this.valueLabels.IndexOf((Label)sender);
            this.ShowInput(index);
            e.Handled = true;
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
                    this.valueLabels[i].Content = str[1 + i];
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
