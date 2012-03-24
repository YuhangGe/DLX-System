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
using System.Diagnostics;

namespace MultiLinker
{
    /// <summary>
    /// OutputPanel.xaml 的交互逻辑
    /// </summary>
    public partial class OutputPanel : UserControl
    {
        private OutputItem _ChooseItem = null;
        OutputItem ChooseItem
        {
            get { return _ChooseItem; }
            set
            {
                if (_ChooseItem != null && this.ItemPanel.Children.IndexOf(_ChooseItem) != -1)
                    _ChooseItem.isChoose = false;
                _ChooseItem = value;
                if (_ChooseItem != null)
                {
                    _ChooseItem.isChoose = true;
                    int address = this.ItemPanel.Children.IndexOf(this._ChooseItem) * 19;
                    if (!(this.Scroll.VerticalOffset < address && this.Scroll.VerticalOffset + this.Scroll.ActualHeight >= address))
                    {
                        this.Scroll.ScrollToVerticalOffset(address);
                    }                    
                }
            }
        }
       

        public OutputPanel()
        {
            InitializeComponent();
            this.InitTextBox.Text = "DLX Editor Single[1.0.0.0]\nAll Rights Reserved 2010(©) Team209\nSoftware Institute of Nanjing University";
            this.KeyDown += new KeyEventHandler(OutputPanel_KeyDown);
            //this.uiControl = uic;
            //this.uiControl.outputPanel = this;
        }

        void OutputPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.RightCtrl || e.Key == Key.LeftCtrl)
            {
                if (this.ChooseItem != null && this.ItemPanel.Children.IndexOf(this.ChooseItem) != -1)
                {
                    int baseindex = this.ItemPanel.Children.IndexOf(this.ChooseItem);
                    int index = this.ItemPanel.Children.IndexOf(this.ChooseItem) + 1;
                    index = index % this.ItemPanel.Children.Count;
                    while (!((OutputItem)this.ItemPanel.Children[index]).ChooseAble)
                    {
                        index = (index + 1) % this.ItemPanel.Children.Count;
                        if (index == baseindex)
                            return;
                    }
                    this.ChooseItem = (OutputItem)this.ItemPanel.Children[index];
                }
            }
            if (e.Key == Key.Enter)
            {
                if (this.ChooseItem != null)
                {
                    this.ChooseItem.OnDoubleClick();
                }
            }
        }
        //清除字符串

        private delegate void StringParaFun(String s);
        private delegate void NullParaFun();
        public void Clear()
        {
            this.Dispatcher.Invoke(new NullParaFun(this.clear));
        }
        public void clear()
        {
            this.ItemPanel.Children.Clear();
            this.ChooseItem = null;
        }
        //显示字符串
        public void Show(String str)
        {
            this.Dispatcher.Invoke(new StringParaFun(this.show), str);
        }
        public void show(String str)
        {            
            this.InitTextBox.Visibility = Visibility.Hidden;
            List<String> lines = this.Split(str, '\n');
            this.ItemPanel.Children.Clear();
            this.ChooseItem = null;
            for (int i = 0; i < lines.Count; i++)
            {
                OutputItem oi = this.MakeOutputItem(lines[i]);
                this.ItemPanel.Children.Add(oi);
            }
            this.Scroll.ScrollToBottom();
        }
        //添加字符串
        public void Append(String str)
        {
            this.Dispatcher.Invoke(new StringParaFun(this.append), str);
        }
        public void append(String str)
        {
            this.InitTextBox.Visibility = Visibility.Hidden;
            List<String> lines = this.Split(str, '\n');
            for (int i = 0; i < lines.Count; i++)
            {
                OutputItem oi = this.MakeOutputItem(lines[i]);
                this.ItemPanel.Children.Add(oi);
            }
            this.Scroll.ScrollToBottom();
        }
        //根据字符c切分字符串
        private List<String> Split(String str, char c)
        {
            List<String> lines = new List<String>();
            String s = str;
            while (s.IndexOf(c) != -1)
            {
                lines.Add(s.Substring(0, s.IndexOf(c)));
                s = s.Substring(s.IndexOf(c) + 1);
            }
            lines.Add(s);
            return lines;
        }
        //生成outputitem
        private OutputItem MakeOutputItem(String str)
        {
            OutputItem oi = new OutputItem();
            if (str.Length > 0 && str[0] == '{' && str.IndexOf('}') > 0)
            {
                oi.ChooseAble = true;
                oi.message = str.Substring(1, str.IndexOf('}') - 1);
                if (oi.message.Length != 0)
                    oi.DoubleClick += new OutputItem.DoubleClickHandle(OutputItem_DoubleClick);
                if (str.Length == str.IndexOf('}') + 1)
                    str = "";
                else
                    str = str.Substring(str.IndexOf('}') + 1);
            }
            oi.MouseDown += new MouseButtonEventHandler(OutputItem_MouseDown);
            oi.Show(str);
            return oi;
        }

        void OutputItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount==1)
                this.ChooseItem = (OutputItem)sender;
        }


        //双击显示区事件
        void OutputItem_DoubleClick(OutputItem sender, string message)
        {
            MessageParse.ParseMessage(message);
        }


        public void SetChoose(int s)
        {
            if (s < this.ItemPanel.Children.Count)
                this.ChooseItem = (OutputItem)this.ItemPanel.Children[s];
        }

        public void Fresh()
        {
            this.ChooseItem = null;
            this.ItemPanel.Children.Clear();
            this.InitTextBox.Visibility = Visibility.Visible;
        }
    }

    class MessageParse
    {
        public static void ParseMessage(String message)
        {
            Debug.WriteLine(message);
            char ins = message[0];
            switch (ins)
            {
                case 'F': FMessage(message); break;
                default: break;
            }
        }
        private static void FMessage(String message)
        {
            String name = message.Substring(2);
            String path = MainForm.ParsePath(name);
            Debug.WriteLine(path);
            OutProcess.CallExplorer(path);
        }
       
    }
}


