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
using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Diagnostics;
using System.Globalization;

namespace Edit
{
    /// <summary>
    /// DLXTabControl.xaml 的交互逻辑
    /// </summary>
    public partial class DLXTabControl : UserControl
    {
        public delegate void eventdelegate(object sender, object[] args);
        public event eventdelegate SelectedChange;
        int Nodot = 0;
        public List<DlxTabItem> allItems;
        private DlxTabItem _selectedOne = null;
        private DlxTabItem selectedOne
        {
            set
            {
                if(_selectedOne!=null)
                    this._selectedOne.IsSelect=false;
                this._selectedOne = value;
                if (value != null)
                    value.IsSelect = true;
                if (this.SelectedChange != null)
                    this.SelectedChange.Invoke(this, new object[] { _selectedOne });
                this.AdjustHeader();
            }
            get
            {
                return this._selectedOne;
            }
        }
        private List<DlxTabItem> showItems;
        TipWindow tw;

        public DLXTabControl()
        {
            InitializeComponent();
            selectedOne = null;
            allItems = new List<DlxTabItem>();
            showItems = new List<DlxTabItem>();

            this.SizeChanged += new SizeChangedEventHandler(DLXTabControl_SizeChanged);
            this.closeShow.MouseDown += new MouseButtonEventHandler(closeShow_MouseDown);
            this.unfoldShow.MouseDown += new MouseButtonEventHandler(unfoldShow_MouseDown);
        }

        void unfoldShow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p1 = SmallTool.GetMousePosition();
            Point p2 = Mouse.GetPosition((IInputElement)sender);
            double x = p1.X - p2.X;
            double y = p1.Y - p2.Y;
            tw = new TipWindow();
            tw.Show();
            tw.AddItems(this.allItems, this.selectedOne);
            SetTipLocation(tw, x, y);
            tw.ChooseLabel += new TipWindow.eventdelegate(tw_ChooseLabel);
        }

        void tw_ChooseLabel(object sender, object[] args)
        {
            this.selectedOne = allItems[(int)args[0]];
            tw.AddItems(this.allItems, this.selectedOne);
        }
        private void SetTipLocation(TipWindow tp, double x, double y)
        {
            double swidth = tp.visualWidth;
            double sheight = tp.visualHeight;
            double lwidth = SystemParameters.WorkArea.Width;
            double lheight = SystemParameters.WorkArea.Height;

            if (y > sheight && lheight - y - 10 < sheight)
                tp.Top = y - sheight;
            else
                tp.Top = y + 10;

            if (lwidth - x + 10 < swidth)
                tp.Left = lwidth - swidth;
            else if (x > 10)
                tp.Left = x - 10;
            else
                tp.Left = 0;
        }


        void closeShow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.selectedOne != null)
                this.selectedOne.OnClose();
        }
        void DLXTabControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.AdjustHeader();
        }
        
        public void AdjustHeader()
        {
            if (selectedOne == null)
            {
                try
                {
                    this.HeaderPanel.Children.Clear();                    
                }
                catch (Exception e) { }
                try
                {
                    this.MainPanel.Children.Clear();
                }
                catch (Exception e) { }
                try
                {
                    this.showItems.Clear();
                }
                catch (Exception e) { }
                return;
            }
            double width = this.ActualWidth - 32;
            this.HeaderPanel.Children.Clear();
            this.HeaderPanel.Children.Add(selectedOne.Header);
            this.MainPanel.Children.Clear();
            this.MainPanel.Children.Add(selectedOne.Content);
            List<DlxTabItem> term = new List<DlxTabItem>();
            term.Add(this.selectedOne);
            if (this.showItems.IndexOf(this.selectedOne) != -1)
            {
                int index = this.showItems.IndexOf(this.selectedOne);
                double currentw = HeaderWidth(this.selectedOne);
                for (int i = 0; i < index; i++)
                {
                    if (currentw + HeaderWidth(showItems[i]) <= width)
                    {
                        this.HeaderPanel.Children.Insert(this.HeaderPanel.Children.Count - 1, showItems[i].Header);
                        term.Insert(term.Count - 1, showItems[i]);
                        currentw = currentw + HeaderWidth(showItems[i]);
                    }
                    else
                    {
                        currentw = currentw + HeaderWidth(showItems[i]);
                        break;
                    }
                }
                for (int i = index + 1; i < this.showItems.Count; i++)
                {
                    if (currentw + HeaderWidth(showItems[i]) <= width)
                    {
                        this.HeaderPanel.Children.Add(showItems[i].Header);
                        term.Add(showItems[i]);
                        currentw = currentw + HeaderWidth(showItems[i]);
                    }
                    else
                    {
                        currentw = currentw + HeaderWidth(showItems[i]);
                        break;
                    }
                }
                for (int i = 0; i < this.allItems.Count; i++)
                {
                    if (this.HeaderPanel.Children.IndexOf(allItems[i].Header) != -1)
                        continue;
                    if (currentw + HeaderWidth(allItems[i]) < width)
                    {
                        this.HeaderPanel.Children.Add(allItems[i].Header);
                        term.Add(allItems[i]);
                        currentw = currentw + HeaderWidth(allItems[i]);
                    }
                    else
                    {
                        break;
                    }
                }
                this.showItems.Clear();
                for (int i = 0; i < this.HeaderPanel.Children.Count; i++)
                    this.showItems.Add(term[i]);
                adjustTurn(this.showItems, this.allItems);
            }
            else
            {
                this.allItems.Remove(this.selectedOne);
                this.allItems.Insert(0, this.selectedOne);
                double currentw = HeaderWidth(this.selectedOne);
                for (int i = 1; i < this.allItems.Count; i++)
                {
                    if (currentw + HeaderWidth(allItems[i]) < width)
                    {
                        this.HeaderPanel.Children.Add(allItems[i].Header);
                        term.Add(allItems[i]);
                        currentw = currentw + HeaderWidth(allItems[i]);
                    }
                    else
                    {
                        break;
                    }
                }
                this.showItems.Clear();
                for (int i = 0; i < this.HeaderPanel.Children.Count; i++)
                    this.showItems.Add(term[i]);
            }
            //Debug.Write(width + " ");
            //for (int i = 0; i < showItems.Count; i++)
            //    Debug.Write(HeaderWidth(showItems[i]) + " ");
            //Debug.WriteLine("");
        }
        private void adjustTurn(List<DlxTabItem> showi, List<DlxTabItem> alli)
        {
            for (int i = 0; i < showi.Count; i++)
                alli.Remove(showi[i]);
            for (int i = showi.Count - 1; i >= 0; i--)
                alli.Insert(0, showi[i]);
        }
        public double HeaderWidth(DlxTabItem d)
        {
            double w = getStringLength(d.header, "微软雅黑", 12) + 43.5;
            //Debug.WriteLine(w + " " + d.Header.ActualWidth);
            return w;
            //return 100;
        }

        public int AddItem(DlxTabItem d)
        {
            this.allItems.Add(d);
            d.ControlIndex = this.Nodot;
            this.Nodot++;
            this.selectedOne = d;
            d.SelectClick+=new DlxTabItem.eventdelegate(d_SelectClick);
            d.CloseClick += new DlxTabItem.eventdelegate(d_CloseClick);
            return d.ControlIndex;
        }
        public void CloseItem(DlxTabItem d)
        {
            if (d.ControlIndex < 0)
                return;
            if (d != selectedOne)
            {
                this.showItems.Remove(d);
                this.allItems.Remove(d);
                this.AdjustHeader();
            }
            else
            {
                if (this.allItems.Count > 1)
                {
                    if (this.allItems.IndexOf(d) != 0)
                        this.selectedOne = this.allItems[this.allItems.IndexOf(d) - 1];
                    else
                        this.selectedOne = this.allItems[1];
                    this.showItems.Remove(d);
                    this.allItems.Remove(d);
                    this.AdjustHeader();
                }
                else
                {
                    this.selectedOne = null;
                    this.showItems.Remove(d);
                    this.allItems.Remove(d);
                    this.AdjustHeader();
                }
            }
            d.ControlIndex = -1;
        }

        void d_CloseClick(object sender, object[] args)
        {
            if (!((TabCloseEvent)args[1]).Cancel)
                this.CloseItem((DlxTabItem)sender);

        }
        void d_SelectClick(object sender, object[] args)
        {
            this.selectedOne = (DlxTabItem)sender;
        }
        public void ActiveIndex(int index)
        {
            for(int i=0;i<this.allItems.Count;i++)
                if (allItems[i].ControlIndex == index)
                {
                    this.selectedOne = allItems[i];
                    break;
                }
                    
        }
        public DlxTabItem GetItem(int index)
        {
            for (int i = 0; i < this.allItems.Count; i++)
                if (allItems[i].ControlIndex == index)
                    return allItems[i];
            return null;
        }
        public void HeaderDock(String d)
        {
            if (d.Equals("Buttom"))
            {
                this.HeadGrid.VerticalAlignment = VerticalAlignment.Bottom;
                this.HeadGrid.Margin = new Thickness(0, 0, 0, 0);
                this.MainPanel.Margin = new Thickness(0, 0, 0, 20);
            }
            else
            {
                this.HeadGrid.VerticalAlignment = VerticalAlignment.Top;
                this.HeadGrid.Margin = new Thickness(0, 0, 0, 0);
                this.MainPanel.Margin = new Thickness(0, 20, 0, 0);
            }
        }
        public void HiddenOperation(bool t)
        {
            if (t)
                this.OperationGrid.Visibility = Visibility.Hidden;
            else
                this.OperationGrid.Visibility = Visibility.Visible;
        }

        public DlxTabItem SelectedItem()
        {
            return this.selectedOne;
        }
        public double getStringLength(string content, string fontName, int fontSize)
        {
            FormattedText ft = new FormattedText(content, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(fontName), fontSize, Brushes.Black);
            return ft.Width;
        }
        public void CloseAll()
        {
            for (int i = 0; i < allItems.Count; i++)
                allItems[i].OnClose();
        }
       
    }
}
