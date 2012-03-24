using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Media.Imaging;

namespace Edit
{
    public enum ItemType
    {
        None, Project, CodeFile, BinFile, LinkFile, Folder
    };
	public partial class TreeItemPanel
	{
        public string Path;
        public bool IsFile;
		public TreeItemPanel(ItemType type,String text)
		{
			this.InitializeComponent();
            dealPicture(type);
            this.Text.Text = text;
			// 在此点之下插入创建对象所需的代码。
		}
        private void dealPicture(ItemType type)
        {
            if (type == ItemType.None)
            {
                for (int i = 0; i < this.Picture.Children.Count; i++)
                    ((Image)this.Picture.Children[i]).Visibility = Visibility.Hidden;
                this.Picture.Width = 0;
            }
            if (type == ItemType.Project)
            {
                for (int i = 0; i < this.Picture.Children.Count; i++)
                    ((Image)this.Picture.Children[i]).Visibility = Visibility.Hidden;
                this.ProjectPicture.Visibility = Visibility.Visible;
            }
            if (type == ItemType.LinkFile)
            {
                for (int i = 0; i < this.Picture.Children.Count; i++)
                    ((Image)this.Picture.Children[i]).Visibility = Visibility.Hidden;
                this.LinkFilePicture.Visibility = Visibility.Visible;
            }
            if (type == ItemType.CodeFile)
            {
                for (int i = 0; i < this.Picture.Children.Count; i++)
                    ((Image)this.Picture.Children[i]).Visibility = Visibility.Hidden;
                this.StringFilePicture.Visibility = Visibility.Visible;
            }
            if (type == ItemType.BinFile)
            {
                for (int i = 0; i < this.Picture.Children.Count; i++)
                    ((Image)this.Picture.Children[i]).Visibility = Visibility.Hidden;
                this.BinFilePicture.Visibility = Visibility.Visible;
            }
            if (type == ItemType.Folder)
            {
                for (int i = 0; i < this.Picture.Children.Count; i++)
                    ((Image)this.Picture.Children[i]).Visibility = Visibility.Hidden;
                this.FolderPicture.Visibility = Visibility.Visible;
            }
        }
	}
}