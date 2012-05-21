using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows;
using System.Threading;

namespace Simulate
{

    public class ConditionBreakpoint
    {
        public bool isRegister;
        public int location;
        public String register;
        public int value;
        public int size;
        public ConditionBreakpoint(bool i, int l, String r, int v,int s)
        {
            this.isRegister = i;
            this.location = l;
            this.register = r;
            this.value = v;
            this.size = s;
        }
        public string toString()
        {
            String str="";
            if (isRegister)
                str = this.register.ToUpper();
            else
                str = SmallTool.LocationParse(SmallTool.InttoUint(this.location));
            str += '(';
            if (this.size == 1)
                str += 'x' + SmallTool.BytetoHexString((byte)this.value).ToUpper();
            else
                str += SmallTool.intToHexString(this.value);
            str += ')';
            return str;
        }
    }
    public class StringItem
    {
        public String str;
        public double inf;
        public StringItem() { }
        public StringItem(String s, double i)
        {
            this.str = s;
            this.inf = i;
        }
    }

    public class MemoryInformation
    {
        public UInt32 loc;
        public StringItem loction;
        public StringItem[] byteValue;
        public StringItem hexValue;
        public StringItem insValue;

        public MemoryInformation(UInt32 l, StringItem lc, StringItem[] b, StringItem h, StringItem i)
        {
            this.loc = l;
            this.loction = lc;
            this.byteValue = b;
            this.hexValue = h;
            this.insValue = i;
        }
        public MemoryInformation(UInt32 l, StringItem lc, StringItem b1, StringItem b2, StringItem b3, StringItem b4, StringItem h, StringItem i)
        {
            this.loc = l;
            this.loction = lc;
            this.byteValue = new StringItem[4];
            this.byteValue[0] = b1;
            this.byteValue[1] = b2;
            this.byteValue[2] = b3;
            this.byteValue[3] = b4;
            this.hexValue = h;
            this.insValue = i;
        }
        public MemoryInformation()
        {
            this.byteValue = new StringItem[4];
        }
        public String[] toStrings()
        {
            String[] strs = new String[7];
            if (this.loction == null)
                strs[0] = null;
            else
                strs[0]=this.loction.str;
            for (int i = 0; i < 4; i++)
                if (this.byteValue[i] == null)
                    strs[1 + i] = "00000000";
                else
                    strs[1 + i] = byteValue[i].str;
            if (this.hexValue == null)
                strs[5] = "x00000000";
            else
                strs[5] = this.hexValue.str;
            if (this.insValue == null)
                strs[6] = "NOP";
            else
                strs[6] = this.insValue.str;
            return strs;
        }
    }

    public class MemoryTree
    {
        public MemoryTreeNode root;
        CPUInfo cpu;
        string path = Application.ResourceAssembly.Location;
        /* 增加这个集合对象记录那些已经更新,但界面的内存缓冲树尚未重取的内存地址
         * 确切地说,其中包含的地址都是实际地址/4*4(4字节对齐,向下)的结果
         * 这样的实现方式是为了提高此模块提供给VM模块回调的函数的性能,尽量不影响VM的性能
         * 选用这个数据结构是由于unsigned int很便于排序,这个数据结构对数据的插入,删除和访问
         * 都提供O(log(n))的时间上限.
         * Shore Ray */
        private System.Collections.Generic.SortedDictionary<UInt32,Object> invalidMemAddr=new System.Collections.Generic.SortedDictionary<UInt32,Object>();
       
        /* Optimization attempt 2. No effect
       EventWaitHandle memoryChangeEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
       System.Collections.Queue memoryBufferRequests = new System.Collections.Queue();
       System.Threading.Thread memoryBufferThread;
        */
       public MemoryTree()
       {
           root = null;
           cpu = CPUInfo.getInstance();
           /* 这个回调函数进行了修改.原本的实现导致两个问题:
            * 1.加载较大文件时,一次性创建了过多的MemoryTreeNode对象,造成长时间等待
            * 2.指令执行时,此回调操作较影响指令执行速度
            * 原函数的功能并没有弃置,而是转移到search函数中调用
            * Shore Ray */
           this.cpu.ValueChangeEvent += new CPUInfo.eventdelegate(cpu_ValueChangeEvent);
           this.cpu.ComputerMemoryInit();
           path = path.Substring(0, path.LastIndexOf('\\')) + "\\";
           this.cpu.load(path + "trap.bin");
            
       }
       public void ReSet()
       {
           root = null;
           this.cpu.computer.reset();
           this.cpu.ComputerMemoryInit();
           this.cpu.load(path+"trap.bin");
       }
       public static int height(MemoryTreeNode node)
       {
           return node == null ? -1 : node.height;
       }

       public MemoryTreeNode insert(MemoryTreeNode node)
       {
           this.root = insert(node, this.root);
           return this.root;
       }
       private MemoryTreeNode insert(MemoryTreeNode node, MemoryTreeNode root)
       {
            
           if (root == null)
           {
               root = node;
           }
           else if (node.info.loc < root.info.loc)
           {
               root.left = insert(node, root.left);
               if (height(root.left) - height(root.right) == 2)
                   if (node.info.loc < root.left.info.loc)
                       root = rotateWithLeftChild(root);
                   else
                       root = doubleWithLeftChild(root);
           }
           else if (node.info.loc > root.info.loc)
           {
               root.right = insert(node, root.right);
               if (height(root.right) - height(root.left) == 2)
                   if (node.info.loc > root.right.info.loc)
                       root = rotateWithRightChild(root);
                   else
                       root = doubleWithRightChild(root);
           }
           else
           {
               root.info = node.info;
           }
           root.height = Math.Max(height(root.left), height(root.right)) + 1;
           return root;
       }

       private static MemoryTreeNode rotateWithLeftChild(MemoryTreeNode k2)
       {
           MemoryTreeNode k1 = k2.left;
           k2.left = k1.right;
           k1.right = k2;
           k2.height = Math.Max(height(k2.left), height(k2.right)) + 1;
           k1.height = Math.Max(height(k1.left), k2.height) + 1;
           return k1;
       }
       private static MemoryTreeNode doubleWithLeftChild(MemoryTreeNode k3)
       {
           k3.left = rotateWithRightChild(k3.left);
           return rotateWithLeftChild(k3);
       }
       private static MemoryTreeNode rotateWithRightChild(MemoryTreeNode k2)
       {
           MemoryTreeNode k1 = k2.right;
           k2.right = k1.left;
           k1.left = k2;
           k2.height = Math.Max(height(k2.left), height(k2.right)) + 1;
           k1.height = Math.Max(k2.height, height(k1.right)) + 1;
           return k1;
       }
       private static MemoryTreeNode doubleWithRightChild(MemoryTreeNode k3)
       {
           k3.right = rotateWithLeftChild(k3.right);
           return rotateWithRightChild(k3);
       }

        /* 为此函数增加了职责.
         * 现在它不但负责返回对应地址内存节点的信息,还负责更新需要显示的且已被修改了的内存节点
         * Shore Ray */ 
       public MemoryTreeNode search(UInt32 location)
       {
           
           if (invalidMemAddr.ContainsKey(location))
           {
               AddMomery(location);
               invalidMemAddr.Remove(location);
           }
           return search(location, root);
       }
       private MemoryTreeNode search(UInt32 location, MemoryTreeNode node)
       {
           if (node == null)
           {
               MemoryInformation m = new MemoryInformation(location,
               new StringItem(SmallTool.LocationParse(location), location),
               new StringItem("00000000", 0),
               new StringItem("00000000", 0),
               new StringItem("00000000", 0),
               new StringItem("00000000", 0),
               new StringItem("x00000000", 0),
               new StringItem("NOP", 0));
               return new MemoryTreeNode(m);
           }
           if (location == node.info.loc)
               return node;
           if (location < node.info.loc)
               return search(location, node.left);
           if (location > node.info.loc)
               return search(location, node.right);
           return null;
       }
       /* Optimization attempt 2. No effect
       private void processMemoryBufferRequests()
       {
           while (true)
           {
               memoryChangeEvent.WaitOne();
               while (memoryBufferRequests.Count > 0)
               {
                   Object addr = null;
                   lock (memoryBufferRequests)
                   {
                       addr = memoryBufferRequests.Dequeue();
                   }
                   if (addr != null)
                   {
                       this.AddMomery(SmallTool.InttoUint((int)addr));
                   }
               }
           }
       }
       */


       
        /* 修改了此函数的工作机制.
         * 由于此函数是性能攸关的函数,现在此函数不再亲自处理内存节点树的更新,而只是将该内存地址记录下来.
         * 实际对内存节点数的更新由search函数在需要取这个内存节点的值时进行.
         * Shore Ray */
        void cpu_ValueChangeEvent(object sender, object[] args)
        {
            
            if (!invalidMemAddr.ContainsKey(SmallTool.InttoUint((int)args[0]) / 4 * 4))
            {
                invalidMemAddr.Add(SmallTool.InttoUint((int)args[0])/4*4, null);
            }
            //Original version
            //Debug.WriteLine(args[0]);
            //AddMomery(SmallTool.InttoUint((int)args[0]));

            /* Optimization attempt 2. No effect
           if (memoryBufferThread == null)
           {
               memoryBufferThread = new Thread(
                   this.processMemoryBufferRequests
               );
               memoryBufferThread.Start();
           }

           if (args[0] == null)
           {
               return;
           }
           lock (memoryBufferRequests)
           {
               memoryBufferRequests.Enqueue(args[0]);
           }
           memoryChangeEvent.Set();
           */
           /* 这段代码希望将维护内存缓存的职责交给界面线程,但是效果似乎不好
            * Optimization attempt 1
           MainForm mf=ChildFormControl.getInstance().mw;
           if (mf != null)
           {

               mf.Dispatcher.BeginInvoke(
                   new Action(delegate()
                       {
                           AddMomery(SmallTool.InttoUint((int)args[0]));
                       }
                   )
               );
           }
           else
           {
               AddMomery(SmallTool.InttoUint((int)args[0]));
           }
           */
        }
        public void AddMomery(UInt32 location)
        {
            UInt32 loc = location / 4 * 4;
            byte [] value=new byte[4];
            for (UInt32 i = 0; i < 4; i++)
            {
                value[i] = cpu.getMemoryValue(loc + i);
            }
            this.insert(makeNode(loc, value));
        }
        public MemoryTreeNode makeNode(UInt32 loc, byte[] value)
        {
            StringItem l = new StringItem(SmallTool.LocationParse(loc), loc);
            StringItem[] bs = new StringItem[4];
            string s = SmallTool.BytestoHexString(value);
            double v = value[0] * 16777216 + value[1] * 65536 + value[2] * 256 + value[3];
            StringItem h = new StringItem("x"+s, v);
            string[] ss = SmallTool.HexStringtoByteString(s);
            for (int i = 0; i < 4; i++)
                bs[i] = new StringItem(ss[i], value[i]);
            StringItem ins=new StringItem(SmallTool.InstructionParse(value),v);
            MemoryInformation mi = new MemoryInformation(loc, l, bs, h, ins);
            return new MemoryTreeNode(mi);
        }
    }

    public class MemoryTreeNode
    {
        public MemoryInformation info;
        public int height;
        public MemoryTreeNode left, right;

        public MemoryTreeNode()
        {
            info = new MemoryInformation();
            info.loc = 0;
            info.loction = new StringItem("x00000000", 0);
            info.byteValue[0] = new StringItem("00000000", 0);
            info.byteValue[1] = new StringItem("00000000", 0);
            info.byteValue[2] = new StringItem("00000000", 0);
            info.byteValue[3] = new StringItem("00000000", 0);
            info.hexValue = new StringItem("x00000000", 0);
            info.insValue = new StringItem("x00000000", 0);

            height = 0;
            left = null;
            right = null;
        }
        public MemoryTreeNode(UInt32 _l, StringItem l, StringItem[] b, StringItem h, StringItem i)
        {
            this.info.loc = _l;
            this.info.loction = l;
            this.info.byteValue = b;
            this.info.hexValue = h;
            this.info.insValue = i;

            this.left = null;
            this.right = null;
            this.height = 0;
        }
        public MemoryTreeNode(MemoryInformation m)
        {
            this.info = m;
            height = 0;
            left = null;
            right = null;
        }
    }

    public class SmallTool
    {
        public static string[] ZeroString ={"",
                                           "0",
                                           "00",
                                           "000",
                                           "0000",
                                           "00000",
                                           "000000",
                                           "0000000",
                                           "00000000"};

        public static string[] RegisterNames ={
                                                 "PC","IR",
                                                 "R0","R1","R2","R3","R4","R5","R6","R7","R8","R9",
                                                 "R10","R11","R12","R13","R14","R15","R16","R17","R18","R19",
                                                 "R20","R21","R22","R23","R24","R25","R26","R27","R28","R29",
                                                 "R30","R31",
                                                 "CAUSE","SR","EPC","KBSR","DSR","TMCR"
                                             };

        public static void PerformClick(Button button)
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

        public static string LocationParse(UInt32 l)
        {
            string s = String.Format("{0:X}", l);
            s = "x" + ZeroString[8 - s.Length] + s;
            return s;
        }
        public static string intToHexString(int v)
        {
            string s = String.Format("{0:X}", v);
            s = "x" + ZeroString[8 - s.Length] + s;
            return s;
        }
        public static string BytestoHexString(byte[] value)
        {
            string s = "";
            for (int i = 0; i < 4; i++)
                s += BytetoHexString(value[i]);
            return s;
        }
        public static string BytetoHexString(byte value)
        {
            string s = String.Format("{0:X}", value);
            s = ZeroString[2 - s.Length] + s;
            return s;
        }
        public static string[] HexStringtoByteString(string s)
        {
            string[] strs = new string[4];
            for (int i = 0; i < 4; i++)
                strs[i] = HexChartoByteChar(s[i * 2]) + HexChartoByteChar(s[i * 2 + 1]);
            return strs;
        }

        public static String StringStardard(string s)
        {
            String s1 = s.ToLower();
            String s2 = "";
            for (int i = 0; i < s1.Length; i++)
                if (s1[i] != ' ' && s1[i] != '\t')
                    s2 += s1[i];
            return s2;
        }
        public static bool isRegisterName(string s)
        {
            if (s.Equals("pc"))
                return true;
            if (s.Equals("ir"))
                return true;
            for (int i = 1; i < 32; i++)
                if (s.Equals("r" + i))
                    return true;
            return false;
        }

        public static string HexChartoByteChar(char c)
        {
            switch (c)
            {
                case '0': return "0000";
                case '1': return "0001";
                case '2': return "0010";
                case '3': return "0011";
                case '4': return "0100";
                case '5': return "0101";
                case '6': return "0110";
                case '7': return "0111";
                case '8': return "1000";
                case '9': return "1001";
                case 'A': return "1010";
                case 'B': return "1011";
                case 'C': return "1100";
                case 'D': return "1101";
                case 'E': return "1110";
                case 'F': return "1111";
                default: return "0000";
            }
        }
        public static String byte2str(Byte b)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 7; i >= 0; i--)
            {
                sb.Append((b >> i & 1).ToString());
            }
            return sb.ToString();
        }
        public static byte str2byte(String s)
        {
            byte result = 0;
            int length = s.Length;
            for (int i = 0; i < length; i++)
                if (s[i] != '0' && s[i] != '1')
                    throw new FormatException();
            for (int i = length-1; i >= 0; i--)
            {
                result = (byte)(Math.Pow(2, i) * (s[length -1 - i] - '0') + result);
            }
            return result;
        }
        private static byte str8tobyte(String s)
        {
            byte result = 0;
            int length = s.Length;
            if (length != 8)
                throw new FormatException();
            for (int i = 0; i < length; i++)
                if (s[i] != '0' && s[i] != '1')
                    throw new FormatException();
            for (int i = length - 1; i >= 0; i--)
            {
                result = (byte)(Math.Pow(2, i) * (s[length - 1 - i] - '0') + result);
            }
            return result;
        }
        private static int str2int(String s)
        {
            int result = 0;
            if (s[0]=='0')
            {
                int length = s.Length;
                for (int i = length - 1; i >= 0; i--)
                {
                    result = (int)(Math.Pow(2, i) * (s[length - 1 - i] - '0') + result);
                }
            }
            else
            {
                int length = s.Length;
                for (int i = length - 2; i >= 0; i--)
                {
                    result = (int)(Math.Pow(2, i) * (s[length - 1 - i] - '0') + result);
                }
                result = 0 - (int)((Math.Pow(2, length - 1)) - result);
            }
            return result;
        }    
        
                   
        //**********
        //反汇编函数
        //**********
        public static string InstructionParse(byte[] value)
        {
            String machine = byte2str(value[0]) + byte2str(value[1]) + byte2str(value[2]) + byte2str(value[3]);
            String execute = machine.Substring(0, 6);
            bool back = false;
            if (execute.Equals("000000"))
            {
                execute = machine.Substring(26, 6);
                back = true;
            }
            byte r_1 = str2byte(machine.Substring(6, 5));
            byte r_2 = str2byte(machine.Substring(11, 5));
            String result = "";
            int imm;
            int pcoffset;
            if (back == false)
            {
                switch (execute)
                {
                    case "000001":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "ADDI\tr" + r_1.ToString() + " r" + r_2.ToString() + " " + imm.ToString();
                        break;
                    case "000011":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "SUBI\tr" + r_1.ToString() + " r" + r_2.ToString() + " " + imm.ToString();
                        break;
                    case "001001":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "ANDI\tr" + r_1.ToString() + " r" + r_2.ToString() + " " + imm.ToString();
                        break;
                    case "001010":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "ORI\tr" + r_1.ToString() + " r" + r_2.ToString() + " " + imm.ToString();
                        break;
                    case "001011":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "XORI\tr" + r_1.ToString() + " r" + r_2.ToString() + " " + imm.ToString();
                        break;
                    case "001100":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "LHI\tr" + r_2.ToString() + " " + imm.ToString();
                        break;
                    case "001101":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "SLLI\tr" + r_1.ToString() + " r" + r_2.ToString() + " " + imm.ToString();
                        break;
                    case "001110":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "SRLI\tr" + r_1.ToString() + " r" + r_2.ToString() + " " + imm.ToString();
                        break;
                    case "001111":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "SRAI\tr" + r_1.ToString() + " r" + r_2.ToString() + " " + imm.ToString();
                        break;
                    case "010000":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "SLTI\tr" + r_1.ToString() + " r" + r_2.ToString() + " " + imm.ToString();
                        break;
                    case "010010":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "SLEI\tr" + r_1.ToString() + " r" + r_2.ToString() + " " + imm.ToString();
                        break;
                    case "010100":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "SEQI\tr" + r_1.ToString() + " r" + r_2.ToString() + " " + imm.ToString();
                        break;
                    case "010110":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "LB\tr" + r_1.ToString() + " r" + r_2.ToString() + " " + imm.ToString();
                        break;
                    case "010111":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "SB\tr" + r_1.ToString() + " r" + r_2.ToString() + " " + imm.ToString();
                        break;
                    case "011100":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "LW\tr" + r_1.ToString() + " r" + r_2.ToString() + " " + imm.ToString();
                        break;
                    case "011101":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "SW\tr" + r_1.ToString() + " r" + r_2.ToString() + " " + imm.ToString();
                        break;
                    case "101000":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "BEQZ\tr" + r_1.ToString() + " " + imm.ToString();
                        break;
                    case "101001":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "BNEZ\tr" + r_1.ToString() + " " + imm.ToString();
                        break;
                    case "101100":
                        pcoffset = str2int(machine.Substring(6, 26));
                        result = result + "J\t" + pcoffset.ToString();
                        break;
                    case "101101":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "JR\tr" + r_1.ToString() + " " + imm.ToString();
                        break;
                    case "101110":
                        pcoffset = str2int(machine.Substring(6, 26));
                        result = result + "JAL\t" + pcoffset.ToString();
                        break;
                    case "101111":
                        imm = str2int(machine.Substring(16, 16));
                        result = result + "JALR\tr" + r_1.ToString() + " r" + r_2.ToString() + " " + imm.ToString();
                        break;
                    case "110000":
                        int vector = str2int(machine.Substring(6, 26));
                        result = result + "TRAP\t" + vector.ToString();
                        break;
                    case "100010":
                        r_1 = str2byte(machine.Substring(11, 5));
                        r_2 = str2byte(machine.Substring(16, 5));
                        String special_R = "";
                        switch ((int)r_2)
                        {
                            case 12:
                                special_R = "SR";
                                break;
                            case 13:
                                special_R = "CAUSE";
                                break;
                            case 14:
                                special_R = "EPC";
                                break;
                        }
                        result = result + "MOVI2S\tr" + r_1.ToString() + " " + special_R;
                        break;
                    case "100011":
                        r_1 = str2byte(machine.Substring(11, 5));
                        r_2 = str2byte(machine.Substring(16, 5));
                        special_R = "";
                        switch ((int)r_2)
                        {
                            case 12:
                                special_R = "SR";
                                break;
                            case 13:
                                special_R = "CAUSE";
                                break;
                            case 14:
                                special_R = "EPC";
                                break;
                        }
                        result = result + "MOVS2I\tr" + r_1.ToString() + " " + special_R;
                        break;
                    case "110001":
                        result = result + "RFE";
                        break;
                    default:
                        result = "NOP";
                        break;
                }
            }
            else
            {
                byte r_3 = str2byte(machine.Substring(16, 5));
                switch (execute)
                {
                    case "000001":
                        result = result + "ADD\tr" + r_1.ToString() + " r" + r_2.ToString() + " r" + r_3.ToString();
                        break;
                    case "000011":
                        result = result + "SUB\tr" + r_1.ToString() + " r" + r_2.ToString() + " r" + r_3.ToString();
                        break;
                    case "001001":
                        result = result + "AND\tr" + r_1.ToString() + " r" + r_2.ToString() + " r" + r_3.ToString();
                        break;
                    case "001010":
                        result = result + "OR\tr" + r_1.ToString() + " r" + r_2.ToString() + " r" + r_3.ToString();
                        break;
                    case "001011":
                        result = result + "XOR\tr" + r_1.ToString() + " r" + r_2.ToString() + " r" + r_3.ToString();
                        break;
                    case "001101":
                        result = result + "SLL\tr" + r_1.ToString() + " r" + r_2.ToString() + " r" + r_3.ToString();
                        break;
                    case "001110":
                        result = result + "SRL\tr" + r_1.ToString() + " r" + r_2.ToString() + " r" + r_3.ToString();
                        break;
                    case "001111":
                        result = result + "SRA\tr" + r_1.ToString() + " r" + r_2.ToString() + " r" + r_3.ToString();
                        break;
                    case "010000":
                        result = result + "SLT\tr" + r_1.ToString() + " r" + r_2.ToString() + " r" + r_3.ToString();
                        break;
                    case "010010":
                        result = result + "SLE\tr" + r_1.ToString() + " r" + r_2.ToString() + " r" + r_3.ToString();
                        break;
                    case "010100":
                        result = result + "SEQ\tr" + r_1.ToString() + " r" + r_2.ToString() + " r" + r_3.ToString();
                        break;
                    default:
                        result = "NOP";
                        break;
                }
            }
            return result;

        }
        public static UInt32 StringLocationParse(String str)
        {
            try
            {
                String strs = "";
                for (int i = 0; i < str.Length; i++)
                    if (str[i] != ' ' && str[i] != '\t')
                        strs += str[i];
                if (strs[0] != '#' && strs[0] != 'x' && strs[0] != 'X')
                {
                    double a = double.Parse(strs);
                    if (a >= 0 && a <= UInt32.MaxValue)
                        return (UInt32)a;
                    else if (a < 0 && a >= int.MinValue)
                        return (UInt32)((double)UInt32.MaxValue + 1 + a);
                    else
                        throw new FormatException();
                }
                else if (strs[0] == '#')
                {
                    double a = double.Parse(strs.Substring(1));
                    if (a >= 0 && a <= UInt32.MaxValue)
                        return (UInt32)a;
                    else if (a < 0 && a >= int.MinValue)
                        return (UInt32)((double)UInt32.MaxValue + 1 + a);
                    else
                        throw new FormatException();
                }
                else
                    return UInt32.Parse(strs.Substring(1), System.Globalization.NumberStyles.HexNumber);

            }
            catch (Exception ex)
            {

                throw new FormatException();
            }
        }
        public static int StringValue32Parse(String str)
        {
            try
            {
                String strs = "";
                for (int i = 0; i < str.Length; i++)
                    if (str[i] != ' ' && str[i] != '\t')
                        strs += str[i];
                if (strs[0] == '#')
                {
                    double a = double.Parse(strs.Substring(1));
                    if (a >= int.MinValue && a <= int.MaxValue)
                        return (int)a;
                    else
                        throw new FormatException();
                }
                else if (strs[0] == 'x' || strs[0] == 'X')
                    return int.Parse(strs.Substring(1), System.Globalization.NumberStyles.HexNumber);
                else
                    throw new FormatException();
            }
            catch (Exception ex)
            {

                throw new FormatException();
            }
        }
        public static byte StringValue8Parse(String str)
        {
            try
            {
                if (str[0] != '#' && str[0] != 'x' && str[0] != 'X')
                {
                    return str2byte(str);
                }
                else if (str[0] == '#')
                {
                    double a = double.Parse(str.Substring(1));
                    if (a < 0 && a >= -128)
                        a = 256 + a;
                    if (a >= byte.MinValue && a <= byte.MaxValue)
                        return (byte)a;
                    else
                        throw new FormatException();
                }
                else
                {
                    if (str.Length != 3)
                        throw new FormatException();
                    return byte.Parse(str.Substring(1), System.Globalization.NumberStyles.HexNumber);
                }
            }
            catch (Exception ex)
            {
                throw new FormatException();
            }
        }
        /* 这个函数修改为直接进行类型转换.
         * 没有看出需要进行判断和计算的原因.如果确有原因,请改回,对此造成的不便请谅解.
         * Shore Ray */
        public static UInt32 InttoUint(int value)
        {
            /*
            UInt32 v;
            if (value >= 0)
                v = Convert.ToUInt32(value);
            else
                v = (UInt32)((double)UInt32.MaxValue + value + 1);
            return v;
             */
            return (UInt32)value;
        }
        /* 这个函数修改为直接进行类型转换.
        * 没有看出需要进行判断和计算的原因.如果确有原因,请改回,对此造成的不便请谅解.
        * Shore Ray */
        public static int UinttoInt(UInt32 value)
        {
            /*
            int v;
            if (value <= int.MaxValue)
                v = (int)value;
            else
                v = (int)((double)value - (double)UInt32.MaxValue - 1);
            return v;
             */
            return (Int32)value;
        }
    }
}
