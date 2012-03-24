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

namespace MultiLinker
{
    /// <summary>
    /// FileItem.xaml 的交互逻辑
    /// </summary>
    public partial class FileItem : UserControl
    {
        private String _name;
        public String FileName { get { return _name; } }
        
        private String _path;
        public String FilePath { get { return _path; } }

        private bool _isLink;
        public bool IsLink { get { return _isLink; } }

        public delegate void CloseItemHandle(FileItem fi);
        public event CloseItemHandle CloseItem;

        public FileItem()
        {
            InitializeComponent();
        }

        public void FileModel(String name, String path, bool isLink,bool isDlx)
        {
            this._name = name;
            this._path = path;
            this._isLink = isLink;

            this.FileNameText.Text = name;
            this.FilePathText.Text = path;
            this.FileNameText.Focusable = true;
            this.FilePathText.Focusable = true;
            this.path3.Visibility = Visibility.Visible;
            this.image2.Visibility = Visibility.Hidden;
            this.image0.Visibility=Visibility.Hidden;
            this.image1.Visibility=Visibility.Hidden;
            this.image3.Visibility = Visibility.Hidden;
            if (isLink)
                this.image1.Visibility = Visibility.Visible;
            else
                if (isDlx)
                    this.image0.Visibility = Visibility.Visible;
                else
                    this.image3.Visibility = Visibility.Visible;
            this.rectGrid.Visibility = Visibility.Visible;
            this.TitleRect.Visibility = Visibility.Hidden;
            this.CloseGrid.Visibility = Visibility.Visible;
            this.ellipse.MouseEnter += new MouseEventHandler(ellipse_MouseEnter);
            this.ellipse.MouseLeave += new MouseEventHandler(ellipse_MouseLeave);
            this.path1.MouseEnter += new MouseEventHandler(ellipse_MouseEnter);
            this.path1.MouseLeave += new MouseEventHandler(ellipse_MouseLeave);
            this.path2.MouseEnter += new MouseEventHandler(ellipse_MouseEnter);
            this.path2.MouseLeave += new MouseEventHandler(ellipse_MouseLeave);
            this.CloseCanvas.MouseDown += new MouseButtonEventHandler(CloseGrid_MouseDown);

            this.ContextMenu = new ContextMenu();
            MenuItem mi = new MenuItem();
            mi.Header = "Goto The Folder";
            mi.Click += new RoutedEventHandler(mi_Click);
            this.ContextMenu.Items.Add(mi);
        }

        void mi_Click(object sender, RoutedEventArgs e)
        {
            String path = MainForm.ParsePath(this.FilePath);
            OutProcess.CallExplorer(path);
        }

        void CloseGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.CloseItem != null)
                this.CloseItem.Invoke(this);
        }

        public void OnClose()
        {
            if (this.CloseItem != null)
                this.CloseItem.Invoke(this);
        }

        void ellipse_MouseLeave(object sender, MouseEventArgs e)
        {
            this.ellipse.Fill = new SolidColorBrush(Color.FromArgb(0, 180, 0, 0));
            this.path1.Stroke = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
            this.path2.Stroke = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
        }
        void ellipse_MouseEnter(object sender, MouseEventArgs e)
        {
            this.ellipse.Fill = new SolidColorBrush(Color.FromArgb(255, 180, 0, 0));
            this.path1.Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            this.path2.Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        }

        public void SetClose(bool b)
        {
            this.CloseCanvas.IsEnabled = b;
        }
    }
}
