using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Text.RegularExpressions;

namespace Edit_Single
{
    public class UIControl
    {
        public OutputPanel outputPanel;
        public EditPanel editPanel;
        public MainForm mainForm;
        public FileControl fileControl;
        public ProgramControl programControl;
        public static String path = null;
        public UIControl()
        {
            this.fileControl = new FileControl(this);
            this.programControl = new ProgramControl(this);
        }

        //当输出区的提示信息被激活时的处理函数
        public void MessageFocus(String str)
        {
            MessageParse.ParseMessage(this, str);
        }

        public void FormattedErrorShow(String s)
        {
            this.outputPanel.Show(s);
        }

        public Deploy getDeploy()
        {
            return Deploy.GetDeploy();
        }

        public void FindText(String regstr,String txt)
        {
            Regex reg = new Regex(regstr);
            Match mat = reg.Match(txt);
            while (mat.Success)
            {
                MessageBox.Show(mat.Index.ToString());//位置
                mat = reg.Match(txt, mat.Index + mat.Length);
            }
        }

        public static void PerformButtonClick(Button button)
        {
            if (button.IsEnabled)
            {
                var peer = new ButtonAutomationPeer(button);

                var invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;

                if (invokeProv != null)
                {
                    invokeProv.Invoke();
                }
            }
        }
        public static void PerformMenuItemClick(MenuItem button)
        {
            if (button.IsEnabled)
            {
                var peer = new MenuItemAutomationPeer(button);

                var invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;

                if (invokeProv != null)
                {
                    invokeProv.Invoke();
                }
            }
        }
    }



    class MessageParse
    {
        public static void ParseMessage(UIControl uic, String message)
        {
            Debug.WriteLine(message);
            char ins = message[0];
            switch (ins)
            {
                case 'E': EMessage(uic, message); break;
                case 'O': OMessage(uic, message); break;
                case 'F': FMessage(uic, message); break;
                case 'H': HMessage(uic, message); break;
                default: break;
            }
        }
        private static void EMessage(UIControl uic, String message)
        {
            try
            {
                String str = message.Substring(2);
                int index = str.IndexOf(',');
                int num1 = int.Parse(str.Substring(0, index));
                str = str.Substring(index + 1);
                index = str.IndexOf(',');
                int num2 = int.Parse(str.Substring(0, index));
                str = str.Substring(index + 1);
                uic.editPanel.ErrorTip(num1, num2);
                uic.editPanel.FocusOn();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        private static void OMessage(UIControl uic, String message)
        {
            String mes = message.Substring(2);
            int address = int.Parse(mes);
            uic.outputPanel.SetChoose(address);
        }
        private static void FMessage(UIControl uic, String message)
        {
            String name = message.Substring(2);
            String path = FileControl.ParsePath(name);
            Debug.WriteLine(path);
            OutProcess.CallExplorer(path);
        }
        private static void HMessage(UIControl uic, String message)
        {
            try
            {
                String[] names = new String[] { "llk.exe", "cjml.exe" };
                String name;
                if ((new Random()).NextDouble() <= 0.5)
                    name = names[0];
                else
                    name = names[1];
                OutProcess.CallOutProcessSample(FileControl.CombineName(UIControl.path, name), "");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("HMessage: " + ex.ToString());
                MessageBox.Show("程序居然打不开？？\n我擦嘞，不能忍……");
                uic.fileControl.SetChange(false);
                uic.mainForm.Close();
            }
        }
    }

    public class Deploy
    {
        public bool PromptSwitch;
        public bool NumberSwitch;
        public int FontSize;
        public string CharEncode;
        public string HLFile;
        public string LinkExt;
        public string BinExt;
        public string DlxExt;
        public string DLXAssemblerName;
        public string DLXLinkerName;
        public string DLXSimulateName;
        public string DLXMultiLinkerName;
        public static Deploy GetDeploy()
        {
            Deploy d = new Deploy();
            d.PromptSwitch = true;
            d.NumberSwitch = true;
            d.FontSize = 24;
            d.CharEncode = "gb2312";
            d.DlxExt = ".dlx";
            d.LinkExt = ".link";
            d.BinExt = ".bin";
            d.DLXAssemblerName = "DLXAssembler.exe";
            d.DLXLinkerName = "DLXLinker.exe";
            d.DLXSimulateName = "Simulate.exe";
            d.DLXMultiLinkerName = "MultiLinker.exe";
            d.HLFile = FileControl.CombineName(UIControl.path, "highlight.xshd");
            return d;
        }

        public static String h
        {
            get
            {
                return
                "完善Edit-Single，烦躁……\n" +
                "DLXEditor有偶然数组索引越界，找不到原因\n" +
                "调试不知道为什么有时候.vshost会崩溃\n" +
                "图片要P，界面要拖，写个程序要开vs，blend，ps……\n" +
                "Simulate要重构，Editor完整版要重写\n" +
                "没钱发T-T\n" +
                "自定义控件神马的最讨厌了！复杂的类关系or无止尽的事件，让我情何以堪啊！\n"+
                "决定写注释的，只写了5行就坚持不了了，绝对不能再来重构！\n"+
                "我擦！葛羽航没把DocumentChanged事件传给我\n" +
                "我擦！怎么强制调用控件Command啊\n" +
                "我擦！葛羽航又不知道跑哪去了，他妈的快改DLXEditor啊！！！\n" +
                "我擦！宝石迷阵居然被黄力为超了……TMD\n" +
                "嗯？我擦嘞，写这段话的时候居然发现一个隐蔽的bug\n" +
                "先去改了再来写……FUCK……\n" +
                "……\n" +
                "……\n" +
                "改好了……\n" +
                "不知道还有多少bug没找到……大一小朋友们对不起你们……\n" +
                "Edit-Single基本功能实现，纪念一下\n" +
                "首先感谢国家，感谢党\n" +
                "感谢葛羽航提供的各个版本各个bug的DLXEditor控件\n" +
                "感谢我的GF——外院貌美如花球技无敌的天下第一大天才 —— 王欢对开发进度的极大促进作用\n" +
                "感谢……感谢我妹的- - |||……\n" +
                "\t\tHL\t2011/1/8\n" +
                "{H}真是写得蛋疼，点这里放松下……"
                ;
            }
        }
    }
}
