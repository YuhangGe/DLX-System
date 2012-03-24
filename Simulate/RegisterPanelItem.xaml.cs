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

namespace Simulate
{
    /// <summary>
    /// RegisterPanelItem.xaml 的交互逻辑
    /// </summary>
    public partial class RegisterPanelItem : UserControl
    {
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
        public void setName(String name)
        {
            this.name = name;
            this.RegisterName.Content = name;
        }
        public RegisterPanelItem()
        {
            InitializeComponent();
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
