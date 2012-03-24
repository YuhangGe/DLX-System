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
using System.Globalization;
using System.Diagnostics;
using System.Windows.Threading;
namespace Simulate
{
    public class IPoint
    {
        public int X;
        public int Y;
        public IPoint(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        public override string ToString()
        {
            return string.Format("Point{{{0},{1}}}", X, Y);
        }
    }
    public class ConsoleCanvas : Canvas
    {

        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return new RectangleGeometry(new Rect(base.RenderSize));
        }
        private ContextMenu cmenu = new ContextMenu();
        private DispatcherTimer dt = new DispatcherTimer();
        private bool flash = true;
        public bool Flash
        {
            get
            {
                return flash;
            }
            set
            {
                flash = value;
            }
        }
        public ConsoleCanvas()
        {

            SetFont("Consolas");
            this.cursor = new Line();
            this.cursor.Stroke = Brushes.White; ;
            this.cursor.StrokeThickness = 2.5;
            this.Children.Add(this.cursor);

            highlightBrush.Opacity = 0.7;

            this.Loaded += new RoutedEventHandler(DealLoaded);
            this.MouseMove += new MouseEventHandler(DealMouseMove);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(DealMouseDown);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(DealMouseUp);

            item = new MenuItem();
            item.Header = "复制";
            item.InputGestureText = "Ctrl+C";
            item.ToolTip = "复制选中文本";
            item.Click += new RoutedEventHandler(DealCopy);
            item.IsEnabled = false;
            cmenu.Items.Add(item);
            this.ContextMenu = cmenu;
            MenuItem item2 = new MenuItem();
            item2.Header = "清空";
            item2.InputGestureText = "Ctrl+L";
            item2.ToolTip = "清空控制台";
            item2.Click += new RoutedEventHandler(DealClear);
            cmenu.Items.Add(item2);
            this.ContextMenu = cmenu;


        }
        private void DealClear(object sender, RoutedEventArgs e)
        {
            this.Clear();
        }

        private void DealLoaded(object sender, RoutedEventArgs e)
        {
            this.parent = (ScrollViewer)this.Parent;
            dt.Interval = new TimeSpan(5000000);
            dt.Tick += new EventHandler(FlashCursor);
            dt.Start();
            this.Cursor = Cursors.IBeam;

        }
        private void DealCopy(object sender, RoutedEventArgs e)
        {
            DoCopy();
        }
        public void DoCopy()
        {
            if (start.X == -1)
                return;
            IPoint top, bottom;
            if (start.Y > end.Y)
            {
                top = end; bottom = start;
            }
            else
            {
                top = start; bottom = end;
            }
            int row_start = top.Y;
            int row_end = bottom.Y;
            int line_each_num = (int)(this.RenderSize.Width / fontWidth);
            int col_start = top.X;
            int col_end = bottom.X;
            if (start.Y == end.Y)
                if (start.X > end.X)
                {
                    col_start = end.X; col_end = start.X;
                }
                else
                {
                    col_start = start.X; col_end = end.X;
                }


            int line_index = 1;
            int col_index = 0;
            int len = _Text.Length;

            StringBuilder toCopy = new StringBuilder();
            bool doCopy = false;
            for (int i = 0; i < len; i++)
            {
                char c = _Text[i];
                if (c == '\n')
                {
                    line_index++;
                    col_index = 0;

                }
                else
                {
                    col_index++;
                    if (col_index == line_each_num - 1)
                    {
                        line_index++;
                        col_index = 0;
                    }
                }
                if (line_index == row_start && col_index == col_start)
                {
                    doCopy = true;
                }
                if (line_index == row_end && col_index == col_end)
                {
                    if (c == '\n')
                        toCopy.Append('\r');
                    toCopy.Append(c);
                    doCopy = false;
                }
                if (doCopy)
                {
                    if (c == '\n')
                        toCopy.Append('\r');
                    toCopy.Append(c);
                }

            }
            System.Diagnostics.Debug.Print(toCopy.ToString());
            Clipboard.SetText(toCopy.ToString());
            start.X = -1;
            end.X = -1;
            this.InvalidateVisual();
            item.IsEnabled = false;
        }
        private void FlashCursor(object sender, EventArgs e)
        {
            if (flash)
                if (cursor.Visibility == Visibility.Hidden)
                    cursor.Visibility = Visibility.Visible;
                else
                    cursor.Visibility = Visibility.Hidden;
            else
                cursor.Visibility = Visibility.Hidden;
        }
        private Typeface _TypeFace = null;
        private double fontHeight = 0;
        private double fontWidth = 0;
        private double fontSize = 20;
        private string fontName = "";
        private Brush foreground = Brushes.White;

        private Line cursor = null;
        public double FontSize
        {
            get { return fontSize; }
            set
            {
                fontSize = value;
                CalcFont();
            }
        }
        public Brush Foreground
        {
            get { return foreground; }
            set { foreground = value; cursor.Stroke = foreground; }
        }
        private SolidColorBrush highlightBrush = new SolidColorBrush(Colors.Yellow);
        public Color HighLightColor
        {
            get { return highlightBrush.Color; }
            set { highlightBrush.Color = value; }
        }
        public double HighLightOpacity
        {
            get { return highlightBrush.Opacity; }
            set { highlightBrush.Opacity = value; }
        }

        private MenuItem item = null;

        private bool mouse_down = false;
        private IPoint start = new IPoint(-1, -1);
        private IPoint end = new IPoint(-1, -1);
        private Point prePoint;
        private void DealMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point tmp = e.GetPosition(this);
            prePoint = tmp;
            int row = (int)(tmp.Y / fontHeight) + 1;
            int col = (int)((tmp.X - PADDINGLEFT) / fontWidth) + 1;
            start.Y = row;
            start.X = col;
            mouse_down = true;
        }
        private void DealMouseUp(object sender, MouseButtonEventArgs e)
        {
            mouse_down = false;
            Point tmp = e.GetPosition(this);
            if (prePoint.X == tmp.X)
            {
                start.X = -1;
                end.X = -1;
                item.IsEnabled = false;
                this.InvalidateVisual();
                return;
            }
            item.IsEnabled = true;
            int row = (int)(tmp.Y / fontHeight) + 1;
            int col = (int)((tmp.X - PADDINGLEFT) / fontWidth) + 1;
            end.Y = row;
            end.X = col;
            this.InvalidateVisual();
        }
        private void DealMouseMove(object sender, MouseEventArgs e)
        {
            if (mouse_down)
            {
                Point tmp = e.GetPosition(this);
                int row = (int)(tmp.Y / fontHeight) + 1;
                int col = (int)((tmp.X - PADDINGLEFT) / fontWidth) + 1;
                end.Y = row;
                end.X = col;
                this.InvalidateVisual();
            }
        }

        public double CursorThickness
        {
            get { return cursor.StrokeThickness; }
            set { cursor.StrokeThickness = value; }
        }
        public void ReSize()
        {
            parent.ScrollToVerticalOffset(this.Height - BOTTOMNUM * fontHeight - parent.ViewportHeight + PADDINGBOTTOM + PADDINGBOTTOM);
        }
        private void SetCursorLocation(Point p)
        {
            cursor.X1 = p.X;
            cursor.X2 = cursor.X1 + fontWidth;
            cursor.Y1 = p.Y;
            cursor.Y2 = p.Y;
        }
        public void SetFont(string fontName)
        {
            _TypeFace = new Typeface(fontName);
            CalcFont();
        }
        public string FontName
        {
            get { return fontName; }
            set { fontName = value; SetFont(fontName); }

        }
        private void CalcFont()
        {
            FormattedText ft = new FormattedText("i", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _TypeFace, fontSize, Foreground);
            fontHeight = ft.Height;
            fontWidth = ft.Width;
        }
        public string Text
        {
            get { return _Text.ToString(); }
            set { _Text = new StringBuilder(value); }
        }
        private ScrollViewer parent = null;
        private delegate void NoParaFun();
        public void AppendChar(char c)
        {
            _Text.Append(c);
            checkAppend = true;
            this.Dispatcher.Invoke(new NoParaFun(this.InvalidateVisual));
        }
        public void Clear()
        {
            _Text.Length = 0;
            start.X = -1;
            end.X = -1;
            this.Dispatcher.Invoke(new NoParaFun(this.InvalidateVisual));
        }
        private const double PADDINGLEFT = 5;
        private const double PADDINGRIGHT = 5;
        private const double PADDINGBOTTOM = 2;
        private const int BOTTOMNUM = 40;

        private double textTotalHeight = 0;
        private StringBuilder _Text = new StringBuilder();
        bool checkAppend = false;
        private void RenderText(DrawingContext drawingContext)
        {
            double totalRow = fontHeight;
            int num_each_line = (int)(this.RenderSize.Width / fontWidth);

            char[] line = new char[num_each_line];

            int line_count = 0;
            int len = _Text.Length;
            for (int i = 0; i < len; i++)
            {
                char c = _Text[i];
                if (c == '\n')
                {
                    OutputLine(drawingContext, line, totalRow);
                    totalRow += fontHeight;
                    line_count = 0;
                    if (i == len - 1)
                    {
                        textTotalHeight = totalRow;
                    }
                    continue;
                }
                line[line_count] = c;
                line_count++;
                if (line_count == num_each_line - 1)
                {
                    OutputLine(drawingContext, line, totalRow);
                    totalRow += fontHeight;
                    line_count = 0;
                    if (i == len - 1)
                    {
                        textTotalHeight = totalRow + fontHeight;
                    }
                }
                else
                    if (i == len - 1)
                    {

                        OutputLine(drawingContext, line, totalRow);
                        textTotalHeight = totalRow;
                    }


            }
            SetCursorLocation(new Point(line_count * fontWidth + PADDINGLEFT, totalRow));


            for (int i = 0; i < BOTTOMNUM; i++)
            {
                OutputLine(drawingContext, line, totalRow);
                totalRow += fontHeight;
            }
            this.Height = totalRow;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(Background, null, new Rect(new Point(0, 0), this.RenderSize));

            RenderHighlight(drawingContext);

            RenderText(drawingContext);

            if (checkAppend)
            {
                if (textTotalHeight > parent.VerticalOffset + parent.ViewportHeight - PADDINGBOTTOM)
                {
                    parent.ScrollToVerticalOffset(parent.VerticalOffset + fontHeight);
                }
                checkAppend = false;
            }
        }
        private void RenderHighlight(DrawingContext drawingContext)
        {
            if (start.X == -1)
                return;

            int cross = start.Y - end.Y;
            if (cross < 0)
                cross = -cross;

            if (cross == 0)
            {
                double x = Math.Min(start.X, end.X);
                double width = Math.Abs(start.X - end.X) + 1;
                drawingContext.DrawRectangle(highlightBrush, null, new Rect(x * fontWidth - PADDINGLEFT, (start.Y - 1) * fontHeight, width * fontWidth, fontHeight));
            }
            else
            {
                IPoint top, bottom;
                if (start.Y > end.Y)
                {
                    top = end; bottom = start;
                }
                else
                {
                    top = start; bottom = end;
                }
                double x = top.X * fontWidth - PADDINGLEFT;
                double y = (top.Y - 1) * fontHeight;
                drawingContext.DrawRectangle(highlightBrush, null, new Rect(x, y, this.RenderSize.Width - PADDINGRIGHT - x, fontHeight));
                for (int i = 1; i < cross; i++)
                {
                    drawingContext.DrawRectangle(highlightBrush, null, new Rect(PADDINGLEFT, y + i * fontHeight, this.RenderSize.Width - PADDINGRIGHT - PADDINGLEFT, fontHeight));
                }
                drawingContext.DrawRectangle(highlightBrush, null, new Rect(PADDINGLEFT, (bottom.Y - 1) * fontHeight, bottom.X * fontWidth, fontHeight));
            }

        }

        private void OutputLine(DrawingContext drawingContext, char[] line, double row)
        {
            FormattedText ft = new FormattedText(new string(line), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _TypeFace, fontSize, Foreground);

            drawingContext.DrawText(ft, new Point(5, row - fontHeight));

            Array.Clear(line, 0, line.Length);
        }

    }
}
