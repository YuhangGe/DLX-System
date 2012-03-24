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
using System.Windows.Media.Animation;

namespace Simulate
{
    /// <summary>
    /// RegisterPanel.xaml 的交互逻辑
    /// </summary>
    public partial class RegisterPanel : UserControl
    {
        private int ItemNumber;
        List<RegisterPanelItem> RegisterItems;
        private RegisterPanelControl rpc;

        private int videoIndex;             
        private List<RegisterPanelItem> videoTem;
        List<RegisterPanelItem> UnlockedItems;
        List<RegisterPanelItem> LockedItems;
        public RegisterPanel()
        {
            InitializeComponent();
            this.init(SmallTool.RegisterNames);
            this.update();
        }
        private void init(String [] name)
        {
            this.rpc = RegisterPanelControl.getInstance();

            this.ItemNumber = name.Length;
            this.Height = 19 * name.Length;
            RegisterItems = new List<RegisterPanelItem>();
            UnlockedItems = new List<RegisterPanelItem>();
            LockedItems = new List<RegisterPanelItem>();
            for (int i = 0; i < name.Length; i++)
            {
                RegisterPanelItem r = new RegisterPanelItem();
                r.setName(name[i]);
                this.MainPanel.Children.Add(r);
                Canvas.SetTop(r, i * 19);
                r.toplocation = i * 19;
                Canvas.SetLeft(r, 0);
                r.getArrow().MouseDown += new MouseButtonEventHandler(Arrow_MouseDown);
                this.RegisterItems.Add(r);
                this.UnlockedItems.Add(r);              
            }
            
        }

        public void update()
        {
            this.updateValue(this.rpc.getValue());
            this.updateLight(this.rpc.getLight());
        }
        public void updateValue(int[] values)
        {
            try
            {
                for (int i = 0; i < this.ItemNumber; i++)
                    RegisterItems[i].setValue(values[i]);
            }
            catch (Exception ex) { }
        }
        public void updateLight(bool[] lights)
        {
            try
            {
                for (int i = 0; i < this.ItemNumber; i++)
                    RegisterItems[i].setLight(lights[i]);
            }
            catch (Exception ex) { }
        }

        private int CheckArrow(Path p)
        {
            for (int i = 0; i < RegisterItems.Count; i++)
                if (RegisterItems[i].getArrow() == p)
                    return i;
            return -1;
        }
        private void Arrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int index = CheckArrow((Path)sender);
            RegisterPanelItem r = this.RegisterItems[index];
            if (r.IsLock)
            {
                ItemDown(index);
            }
            else
            {
                ItemUp(index);
            }
        }
        private void ItemUp(int index)
        {
            int index1 = this.UnlockedItems.IndexOf(this.RegisterItems[index]);
            List<RegisterPanelItem> tem = new List<RegisterPanelItem>();
            for (int i = 0; i < index1; i++)
                tem.Add(this.UnlockedItems[i]);
            int location = this.LockedItems.Count * 19;
            this.UnlockedItems.Remove(this.RegisterItems[index]);
            this.LockedItems.Add(this.RegisterItems[index]);
            ItemMove(index, location, tem, true);
        }
        private void ItemDown(int index)
        {
            int index1 = this.LockedItems.IndexOf(this.RegisterItems[index]);
            int index2 = 0;
            while (index2 < this.UnlockedItems.Count)
            {
                if (index < this.RegisterItems.IndexOf(this.UnlockedItems[index2]))
                {
                    
                    break;
                }
                index2++;
            }
            List<RegisterPanelItem> tem = new List<RegisterPanelItem>();
            for (int i = index1 + 1; i < this.LockedItems.Count; i++)
                tem.Add(this.LockedItems[i]);
            for (int i = 0; i < index2; i++)
                tem.Add(this.UnlockedItems[i]);
            int location = this.RegisterItems[index].toplocation + tem.Count * 19;
            this.LockedItems.Remove(this.RegisterItems[index]);
            this.UnlockedItems.Insert(index2, this.RegisterItems[index]);
            ItemMove(index, location, tem, false);
        }
        private void ItemMove(int index, int location, List<RegisterPanelItem> tem, bool d)
        {
            this.videoTem = tem;
            this.videoIndex = index;
            if (d)
            {
                this.RegisterItems[index].IsLock = true;
                int tl = this.RegisterItems[index].toplocation;
                this.RegisterItems[index].toplocation = location;
                //Canvas.SetTop(this.RegisterItems[index], location);
                for (int i = 0; i < tem.Count; i++)
                {
                    tem[i].toplocation += 19;
                    Canvas.SetTop(tem[i], tem[i].toplocation);
                }
                Storyboard b = MakeVideo(index, location,tl, tem, d);                
                b.Begin(this);
            }
            else
            {
                this.RegisterItems[index].IsLock = false;
                int tl = this.RegisterItems[index].toplocation;
                this.RegisterItems[index].toplocation = location;
                //Canvas.SetTop(this.RegisterItems[index], location);
                for (int i = 0; i < tem.Count; i++)
                {
                    tem[i].toplocation -= 19;
                    Canvas.SetTop(tem[i], tem[i].toplocation);
                }
                Storyboard b = MakeVideo(index, location, tl, tem, d);
                b.Begin(this);
            }
            
        }
        private Storyboard MakeVideo(int index, int location,int toplocation, List<RegisterPanelItem> tem, bool d)
        {
            Storyboard video = new Storyboard();
            TranslateTransform mainTra = new TranslateTransform();
            this.RegisterItems[index].RenderTransform = mainTra;
            String targetName = "mainTranslateTransform" +Math.Abs(DateTime.Now.ToBinary());
            this.RegisterName(targetName, mainTra);
            DoubleAnimationUsingKeyFrames mainVideo = new DoubleAnimationUsingKeyFrames();
            mainVideo.Duration = TimeSpan.FromSeconds(0.1);
            mainVideo.KeyFrames.Add(
                new LinearDoubleKeyFrame(
                    location-toplocation, TimeSpan.FromSeconds(0.1)));
            Storyboard.SetTargetName(mainVideo, targetName);
            Storyboard.SetTargetProperty(mainVideo, new PropertyPath(TranslateTransform.YProperty));
            video.Children.Add(mainVideo);
            video.Completed += new EventHandler(video_Completed);
            return video;
        }
        void video_Completed(object sender, EventArgs e)
        {
            try
            {
                this.RegisterItems[this.videoIndex].RenderTransform = null;
                Canvas.SetTop(this.RegisterItems[this.videoIndex], this.RegisterItems[this.videoIndex].toplocation);
                for (int i = 0; i < this.videoTem.Count; i++)
                    Canvas.SetTop(this.videoTem[i], this.videoTem[i].toplocation);
            }
            catch (Exception ex)
            {
               
            }
        }
    }
}
