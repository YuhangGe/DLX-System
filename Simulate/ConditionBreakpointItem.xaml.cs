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

namespace Simulate
{
    /// <summary>
    /// ConditionBreakpointItem.xaml 的交互逻辑
    /// </summary>
    public partial class ConditionBreakpointItem : UserControl
    {
        ConditionBreakpoint cbp;
        ConditionForm cf = null;
        public ConditionBreakpointItem(ConditionForm c)
        {
            InitializeComponent();
            this.cf = c;
            this.ellipse.MouseEnter += new MouseEventHandler(ellipse_MouseEnter);
            this.ellipse.MouseLeave += new MouseEventHandler(ellipse_MouseLeave);
            this.path1.MouseEnter += new MouseEventHandler(ellipse_MouseEnter);
            this.path1.MouseLeave += new MouseEventHandler(ellipse_MouseLeave);
            this.path2.MouseEnter += new MouseEventHandler(ellipse_MouseEnter);
            this.path2.MouseLeave += new MouseEventHandler(ellipse_MouseLeave);
            this.ellipse.MouseDown += new MouseButtonEventHandler(ellipse_MouseDown);
            this.path1.MouseDown += new MouseButtonEventHandler(ellipse_MouseDown);
            this.path2.MouseDown += new MouseButtonEventHandler(ellipse_MouseDown);
        }

        void ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CPUInfo.getInstance().getBreakpoints().Remove(this.cbp);
            this.cf.RefreshItem();
        }

        public void AddCBP(ConditionBreakpoint c)
        {
            this.cbp = c;
            this.ShowCBP();
        }
        public ConditionBreakpoint AddCBP(string l, string v)
        {
            this.cbp = this.makeBreakpoint(l, v);
            if (this.cbp == null)
                throw new Exception();
            this.ShowCBP();
            return this.cbp;
        }
        public void ShowCBP()
        {
            if (this.cbp == null)
                return;
            if (cbp.isRegister)
                this.RegisterName.Content = cbp.register.ToUpper();
            else
                this.RegisterName.Content = SmallTool.LocationParse(SmallTool.InttoUint(cbp.location));
            if (cbp.size == 1)
                this.HexValue.Content = 'x' + SmallTool.BytetoHexString((byte)cbp.value).ToUpper();
            else
                this.HexValue.Content = SmallTool.intToHexString(cbp.value);
            this.DecValue.Content = "#" + cbp.value + "";
        }
        void ellipse_MouseLeave(object sender, MouseEventArgs e)
        {
            this.ellipse.Fill = new SolidColorBrush(Color.FromArgb(0, 255, 0, 0));
            this.path1.Stroke = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
            this.path2.Stroke = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
        }
        void ellipse_MouseEnter(object sender, MouseEventArgs e)
        {
            this.ellipse.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            this.path1.Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            this.path2.Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        }

        private ConditionBreakpoint makeBreakpoint(String l, String v)
        {
            ConditionBreakpoint cbk;
            try
            {
                String ls = SmallTool.StringStardard(l);
                String vs = SmallTool.StringStardard(v);
                if (SmallTool.isRegisterName(ls))
                {
                    int value = SmallTool.StringValue32Parse(vs);
                    cbk = new ConditionBreakpoint(true, 0, ls, value, 4);
                    return cbk;
                }
                else
                {
                    UInt32 loc = SmallTool.StringLocationParse(ls);
                    if (vs[0] != '4')
                    {
                        byte value = SmallTool.StringValue8Parse(vs);
                        cbk = new ConditionBreakpoint(false, SmallTool.UinttoInt(loc), "", value, 1);
                        return cbk;
                    }
                    else
                    {
                        vs = vs.Substring(1);
                        int value1 = SmallTool.StringValue32Parse(vs);
                        cbk = new ConditionBreakpoint(false, SmallTool.UinttoInt(loc / 4 * 4), "", value1, 4);
                        return cbk;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
