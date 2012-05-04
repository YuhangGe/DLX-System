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
using VM;
namespace Simulate
{
    /// <summary>
    /// RegisterPanelItem.xaml 的交互逻辑
    /// </summary>
    public partial class RegisterPanelItem : UserControl
    {
        public delegate void NextRegisterDel(RegisterPanelItem sender, int dire, object para);
        public event NextRegisterDel NextRegisterEvent;
        public int toplocation = 0;
        private bool _isLock = false;
        public bool IsLock
        {
            get
            {
                return _isLock;
            }
            set
            {
                _isLock = value;
                if (_isLock)
                {
                    this.Arrow.Style = (Style)this.FindResource("ArrowDown");
                    this.BackRectangle.Stroke = (Brush)this.FindResource("StrokeLight");
                    this.RegisterName.Foreground = (Brush)this.FindResource("RegisterNameBrush");
                }
                else
                {
                    this.Arrow.Style = (Style)this.FindResource("ArrowUp");
                    this.BackRectangle.Stroke = (Brush)this.FindResource("StrokeCommon");
                    this.RegisterName.Foreground=(Brush)this.FindResource("CommonBrush");
                }
            }
        }
        private String name;
        private List<Label> ValueLabels;
        private List<TextBox> ValueInputs;

        public void setName(String name)
        {
            this.name = name;
            this.RegisterName.Content = name;
        }        
        public RegisterPanelItem()
        {
            InitializeComponent();
            this.ValueLabels = new List<Label>();
            this.ValueLabels.Add(this.HexValue);
            this.ValueLabels.Add(this.DecValue);
            this.ValueInputs = new List<TextBox>();
            this.ValueInputs.Add(this.HexInput);
            this.ValueInputs.Add(this.DecInput);
            
            this.MouseDown += new MouseButtonEventHandler(RegisterPanelItem_MouseDown);
            


            this.HexValue.MouseDoubleClick += new MouseButtonEventHandler(ValueInput_MouseDoubleClick);
            this.DecValue.MouseDoubleClick+=new MouseButtonEventHandler(ValueInput_MouseDoubleClick);
            this.HexInput.LostFocus += new RoutedEventHandler(ValueInput_LostFocus);
            this.DecInput.LostFocus += new RoutedEventHandler(ValueInput_LostFocus);
            this.HexInput.KeyDown += new KeyEventHandler(ValueInput_KeyDown);
            this.HexInput.AddHandler(TextBox.KeyDownEvent, new KeyEventHandler(ValueInput_KeyDown), true);
            this.DecInput.AddHandler(TextBox.KeyDownEvent, new KeyEventHandler(ValueInput_KeyDown), true);
        }

        bool SetValue(String stri)
        {
            String str = SmallTool.StringStardard(stri);
            try
            {
                int value = SmallTool.UinttoInt(SmallTool.StringLocationParse(str));
                Word _r=CPUInfo.getInstance().testRegister(((String)this.RegisterName.Content).ToLower());
                _r.Value = value;
                
                //下面将带来Simulate走向崩溃的高潮……
                //{What the FUCK!!!!!!!!!!!!!!!
                if (_r == CPUInfo.getInstance().computer.KBSR)
                {
                    UInt32 _v = SmallTool.InttoUint(value);
                    byte _b = 0;
                    for (UInt32 i = 0; i < 4; i++)
                    { 
                        _b=(byte)(_v%256);
                        _v=_v/256;
                        CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901763 - i), _b);
                    }
                }
                else if (_r == CPUInfo.getInstance().computer.DSR)
                {
                    UInt32 _v = SmallTool.InttoUint(value);
                    byte _b = 0;
                    for (UInt32 i = 0; i < 4; i++)
                    {
                        _b = (byte)(_v % 256);
                        _v = _v / 256;
                        CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901771 - i), _b);
                    }
                }
                else if (_r == CPUInfo.getInstance().computer.TMCR)
                {
                    UInt32 _v = SmallTool.InttoUint(value);
                    byte _b = 0;
                    for (UInt32 i = 0; i < 4; i++)
                    {
                        _b = (byte)(_v % 256);
                        _v = _v / 256;
                        CPUInfo.getInstance().setMemoryValue(SmallTool.UinttoInt(4294901779 - i), _b);
                    }
                }
                //}What the FUCK!!!!!!!!!!!!!!!
            
            
            }
            catch (Exception ex)
            {
                ChildFormControl.getInstance().getMemoryPanel().breakPause("Register " + this.RegisterName.Content + "- Wrong Value!");
            }
            return true;
        }
        void EnsureInput(TextBox tb)
        {
            this.SetValue(tb.Text);

            int index = this.ValueInputs.IndexOf(tb);
            this.ValueInputs[index].Visibility = Visibility.Hidden;
            this.ValueLabels[index].Visibility = Visibility.Visible;
            ChildFormControl.getInstance().update();
        }
        void ValueInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.EnsureInput((TextBox)sender);
            if (e.Key == Key.Tab)
            {
                this.EnsureInput((TextBox)sender);
                int index = this.ValueInputs.IndexOf((TextBox)sender);
                index = (index + 1) % 2;
                this.ShowInput(index);
                e.Handled = true;
            }
            if (e.Key == Key.Up)
            {
                if (this.NextRegisterEvent != null)
                    this.NextRegisterEvent(this, -1, this.ValueInputs.IndexOf((TextBox)sender));
            }
            if (e.Key == Key.Down)
            {
                if (this.NextRegisterEvent != null)
                    this.NextRegisterEvent(this, 1, this.ValueInputs.IndexOf((TextBox)sender));
            }
        }
        void ValueInput_LostFocus(object sender, RoutedEventArgs e)
        {
            this.EnsureInput((TextBox)sender);
        }
        void RegisterPanelItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
        }

        public void ShowInput(int index)
        {
            this.ValueInputs[index].Text = (String)this.ValueLabels[index].Content;
            this.ValueInputs[index].Visibility = Visibility.Visible;
            this.ValueLabels[index].Visibility = Visibility.Hidden;
            this.ValueInputs[index].Focus();
            this.ValueInputs[index].SelectAll();
        }
        void ValueInput_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int index = this.ValueLabels.IndexOf((Label)sender);
            this.ShowInput(index);
            e.Handled = true;
        }
        
        public Path getArrow()
        {
            return this.Arrow;
        }
        public void setValue(int value)
        {
            this.HexValue.Content = SmallTool.intToHexString(value);
            this.DecValue.Content = "#" + value;
        }
        public void setLight(bool b)
        {
            if (b)
            {
                this.DecValue.Style = (Style)this.FindResource("LabelLight");
                this.HexValue.Style = (Style)this.FindResource("LabelLight");
            }
            else
            {
                this.DecValue.Style = (Style)this.FindResource("LabelCommon");
                this.HexValue.Style = (Style)this.FindResource("LabelCommon");
            }
        }
    }
}
