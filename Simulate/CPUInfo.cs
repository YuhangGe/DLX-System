using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using VM;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Windows.Controls;
using System.Windows;

namespace Simulate
{
    public class CPUInfo
    {
        bool test = false;
        
        private static CPUInfo instance;
        public delegate void eventdelegate(object sender, object[] args);
        public delegate bool runtestdelegate();
        public event eventdelegate ValueChangeEvent;
        public event eventdelegate HaltChangeEvent;
        private delegate void NullParameterDelegate();
        private runtestdelegate OperationTest = null;
        private bool isRun = false;
        public Computer computer;
        bool storeChange = false;
        List<UInt32> lightMemory = new List<UInt32>();
        List<int> lightRegister = new List<int>();
        Thread runThread;
        int instructionsNumber = 0;
        //public Button button = null;

        protected CPUInfo()
        {
            computer = new Computer();
            computer.memory.Modified += new Memory.deleModified(memory_Modified);
            computer.RegModified += new Computer.deleRegModified(computer_RegModified);
            //this.setMemoryValue(SmallTool.UinttoInt(4294901763), 1);
            //this.computer.memory[SmallTool.UinttoInt(4294901763)] |= 1;
            DeviceControl.getInstance().ChangeValue += new DlxExternalDevice.ValueEventDelegate(CPUInfo_ChangeValue);
        }

        void CPUInfo_ChangeValue(int v1, byte v2)
        {
            setMemoryValue(v1, v2);
        }

        void computer_RegModified(int n)
        {
            if (n == 31)
            {
                if (computer.R[31].Value == computer.PC.Value)
                {
                    this.stepoutFunLevel--;
                    this.inFunction = true;
                    //Debug.WriteLine("Register Changed" + this.stepoutFunLevel);
                }
            }
            if (storeChange)
                this.lightRegister.Add(n);
            else
            {
                this.lightRegister.Clear();
                this.lightMemory.Clear();
            }
        }
        void memory_Modified(object sender, object[] args)
        {
            if (storeChange)
                this.lightMemory.Add(SmallTool.InttoUint((int)args[0]));
            else
            {
                this.lightRegister.Clear();
                this.lightMemory.Clear();
            }
            //if (((int)args[0]) == SmallTool.UinttoInt(SmallTool.StringLocationParse("xFFFF00FB")))
            if (((int)args[0]) == SmallTool.UinttoInt(0xFFFF00FB))
            {
                if ((byte)args[1] == 1)
                {
                    this.instructionsNumber = 0;
                    //this.computer.memory[SmallTool.UinttoInt(4294901763)] &= 254;
                    //this.setMemoryValue(SmallTool.UinttoInt(4294901763), 0);
                }
                else
                {
                    //this.computer.memory[SmallTool.UinttoInt(4294901763)] |= 1;
                    //this.setMemoryValue(SmallTool.UinttoInt(4294901763), 1);
                }
                if (this.HaltChangeEvent != null)
                    this.HaltChangeEvent(this, args);
            }
            if (((int)args[0]) == SmallTool.UinttoInt(4294901763))
            {
                this.computer.KBSR.Value = this.computer.memory.GetWord(SmallTool.UinttoInt(4294901760)).Value;
                //this.computer.KBSR.Value = this.computer.memory[SmallTool.UinttoInt(4294901763)];
            }
            if (((int)args[0]) == SmallTool.UinttoInt(4294901771))
            {
                this.computer.DSR.Value = this.computer.memory.GetWord(SmallTool.UinttoInt(4294901768)).Value;
                //this.computer.DSR.Value = this.computer.memory[SmallTool.UinttoInt(4294901771)];
            }
            if (((int)args[0]) == SmallTool.UinttoInt(4294901779))
            {
                this.computer.TMCR.Value = this.computer.memory.GetWord(SmallTool.UinttoInt(4294901776)).Value;
            }
            DeviceControl.getInstance().deviceManage.ValueChanged((int)args[0], (byte)args[1]);
            if (this.ValueChangeEvent != null)
                this.ValueChangeEvent.Invoke(this, args);

        }

        public void ComputerMemoryInit()
        {
            this.computer.InitTrapTable();
            
        }
        public static CPUInfo getInstance()
        {
            if (instance == null)
                instance = new CPUInfo();
            return instance;
        }

        public void setPC(int value)
        {
            computer.PC.Value = value;
        }
        public int getPC()
        {
            return computer.PC.Value;
        }

        public ArrayList getBreakpoints()
        {
            return computer.breakpoints;
        }

        public byte getMemoryValue(UInt32 loc)
        {
            return this.computer.memory[SmallTool.UinttoInt(loc)];
        }
        public void setMemoryValue(int index, byte value)
        {
            this.computer.memory[index] = value;
        }

        public List<UInt32> getMemoryLight()
        {
            return this.lightMemory;
        }
        public List<int> getRegisterLight()
        {
            return this.lightRegister;
        }
        public void storeChangeOn()
        {
            this.storeChange = true;
        }
        public void storeChangeOff()
        {
            this.storeChange = false;
        }

        public int[] getRegisterValue()
        {
            int[] a = new int[40];
            a[0] = this.computer.PC.Value;
            a[1] = this.computer.IR.Value;
            for (int i = 0; i < 32; i++)
                a[i + 2] = this.computer.R[i].Value;
            a[34] = this.computer.CAUSE.Value;
            a[35] = this.computer.SR.Value;
            a[36] = this.computer.EPC.Value;
            a[37] = this.computer.KBSR.Value;
            a[38] = this.computer.DSR.Value;
            a[39] = this.computer.TMCR.Value;
            return a;
        }

        public bool isPowerOn()
        {
            return test;
        }

        public bool operationWay(string name)
        {
            if (isRun)
                return false;
            if (name.Equals("run"))
                this.OperationTest = new runtestdelegate(this.runTest);
            else if (name.Equals("debug"))
                this.OperationTest = new runtestdelegate(this.debugTest);
            else if (name.Equals("stepinto"))
                this.OperationTest = new runtestdelegate(this.stepintoTest);
            else if (name.Equals("stepout"))
            {
                this.OperationTest = new runtestdelegate(this.stepoutTest);
                this.stepoutFunLevel = 0;
            }
            else if (name.Equals("stepover"))
                this.OperationTest = new runtestdelegate(this.stepoverTest);
            else
                return false;
            return true;
        }

        public void RunProgram()
        {
            //lock (this)
            //{
            /* 对instructionCount函数进行过修改,Int32.MinValue的参数表示正在执行指令,
             * 界面上仅显示"Executing instructions..."
             * TODO:
             * 此修改仅是为了ucos操作系统移植所需达到的性能而进行的优化,对于软件设计的一致性
             * 具有一定破坏性.请做进一步修改.
             * Shore Ray */
            ChildFormControl.getInstance().getMemoryPanel().instructionCount(Int32.MinValue);

            //TODO:
            //以下几条语句是为了计算模拟器执行指令的速度
            //正式发布的产品中请去除
            int insNumStart = this.instructionsNumber;
            System.DateTime startTime = System.DateTime.Now;
                do
                {
                    /* 函数体进行了修改
                     * 1.去掉了不必要的delegate间接.这里我不知道用delegate的原因,如果是有原因的,请改回来,由此造成的
                     * 不便请见谅.
                     * 2.禁止了每条指令完成后刷新界面上的指令条数.目前暂时使用了一个简单的方法:仅在CPUInfo.RunProgram函数
                     * 的最后,即跳出执行指令的while循环后刷新指令数.执行指令过程中仅显示"Executing instructions..."
                     * Shore Ray */

                    //NullParameterDelegate fun = new NullParameterDelegate(this.computer.Execute);
                    //fun.Invoke();
                    this.computer.Execute();

                    this.instructionsNumber++;

                    
                    //ChildFormControl.getInstance().getMemoryPanel().instructionCount(this.instructionsNumber);

                    //Debug.WriteLine(this.stepoutFunLevel);
                    //button.Dispatcher.Invoke(fun);
                   // ChildFormControl.getInstance().getMemoryPanel().instructionCount(this.instructionsNumber);
                } while (this.OperationTest());

                
            //TODO:
            //以下语句是为了显示计算模拟器执行指令的速度
            //正式发布的产品中请进行修改
            System.TimeSpan elapsed= System.DateTime.Now - startTime;
            int exeSpeed = (int)((this.instructionsNumber - insNumStart) / elapsed.TotalSeconds);
            if (instructionsNumber - insNumStart > 1)
            {

                ChildFormControl.getInstance().getMemoryPanel().instructionCount(-exeSpeed);
            }
            else
            {
                ChildFormControl.getInstance().getMemoryPanel().instructionCount(instructionsNumber);
            }
            
        }
        public void UpdateViewer()
        {
            ChildFormControl.getInstance().threadUpdate();
        }
        public void RunProgramThreadFun()
        {            
            isRun = true;
            this.PowerOn();
            this.storeChangeOn();
            RunProgram();
            UpdateViewer();
            //NullParameterDelegate fun = new NullParameterDelegate(RunProgram);
            //fun.Invoke();
            //NullParameterDelegate fun1 = new NullParameterDelegate(UpdateViewer);
            
            //fun1.Invoke();
            //fun1.Invoke();
            this.storeChangeOff();
            //this.lightMemory.Clear();
            //this.lightRegister.Clear();
            isRun = false;
        }
        public void RunProgramThreadStart(String name)
        {
            if (runThread != null && (runThread.ThreadState == System.Threading.ThreadState.Aborted || runThread.ThreadState == System.Threading.ThreadState.Stopped))
                runThread = null;
            if (runThread != null)
                return;
            if (!operationWay(name))
                return;
            ChildFormControl.getInstance().getMemoryPanel().stateShowDirect((char)(name[0]-32)+name.Substring(1)+"...");
            runThread = new Thread(new ThreadStart(RunProgramThreadFun));
            runThread.IsBackground = true;
            runThread.Start();            
        }

        public void PowerOn()
        {
            this.computer.memory[SmallTool.UinttoInt(SmallTool.StringLocationParse("xFFFF00FB"))] = 1;
        }
        public void PowerOff()
        {
            this.computer.memory[SmallTool.UinttoInt(SmallTool.StringLocationParse("xFFFF00FB"))] = 0;
        }

        private bool runTest()
        {

            //if (this.computer.memory[SmallTool.UinttoInt(SmallTool.StringLocationParse("xFFFF00FB"))] == 1)
            if (this.computer.memory[SmallTool.UinttoInt(0xFFFF00FB)] == 1)
                return true;
            else
            {
                ChildFormControl.getInstance().getMemoryPanel().stateShowDirect("Halt.");
                return false;
            }
        }
        private bool debugTest()
        {
            //if (this.computer.memory[SmallTool.UinttoInt(SmallTool.StringLocationParse("xFFFF00FB"))] == 1)
            if (this.computer.memory[SmallTool.UinttoInt(0xFFFF00FB)] == 1)
            {
                if (this.computer.breakpoints.IndexOf(this.computer.PC.Value) != -1)
                {
                    String s = "Breakpoint: PC(" + SmallTool.intToHexString(this.computer.PC.Value) + ")";
                    ChildFormControl.getInstance().getMemoryPanel().stateShowDirect(s);
                    return false;
                }
                ConditionBreakpoint c = conditionBreakpointTest();
                if (c != null)
                {
                    String s = "Breakpoint: ";
                    s += c.toString();
                    ChildFormControl.getInstance().getMemoryPanel().stateShowDirect(s);
                    return false;
                }
                return true;
            }
            else
            {
                ChildFormControl.getInstance().getMemoryPanel().stateShowDirect("Halt.");
                return false;
            }
        }
        private ConditionBreakpoint conditionBreakpointTest()
        {
            for (int i = 0; i < this.computer.breakpoints.Count; i++)
            {
                if (this.computer.breakpoints[i] is ConditionBreakpoint)
                {
                    ConditionBreakpoint cbp = (ConditionBreakpoint)this.computer.breakpoints[i];
                    if (cbp.isRegister)
                    {
                        if (testRegister(cbp.register).Value == cbp.value)
                            return cbp;
                    }
                    else
                    {
                        if (cbp.size == 1)
                        {
                            if (computer.memory[cbp.location] == (byte)cbp.value)
                                return cbp;
                        }
                        else
                        {
                            double v = 0;
                            UInt32 l = SmallTool.InttoUint(cbp.location);
                            v = computer.memory[cbp.location];
                            v = v * 256 + computer.memory[SmallTool.UinttoInt(l + 1)];
                            v = v * 256 + computer.memory[SmallTool.UinttoInt(l + 2)];
                            v = v * 256 + computer.memory[SmallTool.UinttoInt(l + 3)];
                            if ((UInt32)v == SmallTool.InttoUint(cbp.value))
                                return cbp;
                        }
                    }
                }
            }
            return null;
        }
        public Word testRegister(string name)
        {
            if (name.Equals("pc"))
                return computer.PC;
            if (name.Equals("ir"))
                return computer.IR;
            if (name.Equals("cause"))
                return computer.CAUSE;
            if (name.Equals("sr"))
                return computer.SR;
            if (name.Equals("epc"))
                return computer.EPC;
            if (name.Equals("kbsr"))
                return computer.KBSR;
            if (name.Equals("dsr"))
                return computer.DSR;
            if (name.Equals("tmcr"))
                return computer.TMCR;
            return computer.R[int.Parse(name.Substring(1))];
        }

        private bool stepintoTest()
        {
            ChildFormControl.getInstance().getMemoryPanel().stateShowDirect("Ready");
            return false;
        }

        bool inFunction = false;
        private bool stepoverTest()
        {
            if (!this.inFunction)
            {
                ChildFormControl.getInstance().getMemoryPanel().stateShowDirect("Ready");
                return false;
            }
            else
            {
                if (this.computer.memory[SmallTool.UinttoInt(SmallTool.StringLocationParse("xFFFF00FB"))] == 1)
                {
                    if (this.computer.breakpoints.IndexOf(this.computer.PC.Value) != -1)
                    {
                        String s = "Breakpoint: PC(" + SmallTool.intToHexString(this.computer.PC.Value) + ")";
                        ChildFormControl.getInstance().getMemoryPanel().stateShowDirect(s);
                        return false;
                    }
                    ConditionBreakpoint c = conditionBreakpointTest();
                    if (c != null)
                    {
                        String s = "Breakpoint: ";
                        s += c.toString();
                        ChildFormControl.getInstance().getMemoryPanel().stateShowDirect(s);
                        return false;
                    }
                    if (computer.PC.Value == computer.R[31].Value)
                    {
                        this.stepoutFunLevel = this.stepoutFunLevel + 1;
                        //Debug.WriteLine(this.stepoutFunLevel + "");
                        if (this.stepoutFunLevel == 0)
                        {
                            ChildFormControl.getInstance().getMemoryPanel().stateShowDirect("Ready");
                            this.inFunction = false;
                            return false;
                        }
                        return true;
                    }
                    return true;
                }
                else
                {
                    ChildFormControl.getInstance().getMemoryPanel().stateShowDirect("Halt.");
                    return false;
                }
            }
        }

        int stepoutFunLevel = 0;
        private bool stepoutTest()
        {
            if (this.computer.memory[SmallTool.UinttoInt(SmallTool.StringLocationParse("xFFFF00FB"))] == 1)
            {
                if (this.computer.breakpoints.IndexOf(this.computer.PC.Value) != -1)
                {
                    String s = "Breakpoint: PC(" + SmallTool.intToHexString(this.computer.PC.Value) + ")";
                    ChildFormControl.getInstance().getMemoryPanel().stateShowDirect(s);
                    return false;
                }
                ConditionBreakpoint c = conditionBreakpointTest();
                if (c != null)
                {
                    String s = "Breakpoint: ";
                    s += c.toString();
                    ChildFormControl.getInstance().getMemoryPanel().stateShowDirect(s);
                    return false;
                }
                if (computer.PC.Value == computer.R[31].Value)
                {
                    this.stepoutFunLevel = this.stepoutFunLevel + 1;
                    //Debug.WriteLine(this.stepoutFunLevel + "");
                    if (this.stepoutFunLevel == 1)
                    {
                        ChildFormControl.getInstance().getMemoryPanel().stateShowDirect("Out of function");
                        return false;
                    }
                    return true;
                }
                return true;
            }
            else
            {
                ChildFormControl.getInstance().getMemoryPanel().stateShowDirect("Halt.");
                return false;
            }
        }
        
        //-----------------------------------------------------------
        private byte[] int_to_bytes(int num)
        {
            byte[] rtn = new byte[4];
            rtn[3] = (byte)(num & 0x000000ff);
            rtn[2] = (byte)((num & 0x0000ff00) >> 8);
            rtn[1] = (byte)((num & 0x00ff0000) >> 16);
            rtn[0] = (byte)((num & 0xff000000) >> 24);
            return rtn;
        }
        private int bytes_to_int(byte[] c)
        {
            return (c[0] << 24 | c[1] << 16 | c[2] << 8 | c[3]);
        }
        private int read_int(BinaryReader br)
        {
            byte[] b = new byte[4];
            for (int i = 0; i < 4; i++)
                b[i] = br.ReadByte();
            return bytes_to_int(b);
        }
        private string IntToBin32(int num)
        {
            string rtn = Convert.ToString(num, 2);
            if (rtn.Length < 32)
            {
                string l = "";
                int left = 32 - rtn.Length;
                for (int i = 0; i < left; i++)
                    l += '0';
                rtn = l + rtn;
            }
            return rtn;
        }

        public bool save(String path, int headaddr, int length)
        {
            /* length is the number of words rather than bytes 
             * headaddr % 4 == 0
             */
            if (headaddr % 4 > 0)
                return false;
            StreamWriter writer = new StreamWriter(path);
            writer.WriteLine(new Word(headaddr).ToString());
            for (int i = 0; i < length; i++)
            {
                writer.WriteLine(computer.memory.GetWord(headaddr + i * 4).ToString());
            }
            writer.Close();
            return true;
        }

        public List<Int32> load(String path)
        {
            if (path.Substring(path.Length - 4, 4).Equals(".bin"))
                return this.com_load(path);
            if (path.Substring(path.Length - 5, 5).Equals(".sbin"))
                return this.bin_load(path);
            else
                throw new Exception("InvalidFile:" + path);
        }
        public List<Int32> bin_load(String path)
        {
            List<Int32> ret = new List<Int32>();
            List<String> cache = new List<String>();
            StreamReader sr = new StreamReader(path, Encoding.GetEncoding("GB2312"));
            String line;
            line = sr.ReadLine();
            int addr = new Word(line).Value;
            ret.Add(0); ret.Add(new Word(line).Value); ret.Add(0);
            ret.Add(0); // The number of TRAP 0
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Length > 0)
                    cache.Add(line.Substring(0, 32));
            }
            if (_check(cache))
            {
                foreach (String s in cache)
                {
                    computer.memory.WriteWord(new Word(s), addr);
                    if (new Word(s).Value == unchecked((int)0xC0000000))
                    {
                        ret[3] += 1;
                        ret.Add(addr);
                    }
                    addr += 4;
                }
            }
            else
                throw new Exception("InvalidSBinFile");
            return ret;
        }
        private bool _check(List<String> code)
        {
            foreach (String s in code)
            {
                if (s.Length != 32)
                    return false;
                foreach (char c in s)
                    if (c != '0' && c != '1')
                        return false;
            }
            return true;
        }

        public List<Int32> com_load(String path)
        {
            List<Int32> ret = new List<int>();
            List<Int32> trap = new List<int>();
            BinaryReader br = new BinaryReader(new BufferedStream(File.OpenRead(path)));

            if (br.ReadByte() != (byte)'d' || br.ReadByte() != (byte)'l' || br.ReadByte() != (byte)'x' || br.ReadByte() != (byte)209)
                throw new Exception("InvalidDLXFile");
            int data_num = read_int(br);
            ret.Add(data_num);

            for (int i = 0; i < data_num; i++)
            {
                int data_init = read_int(br);
                int data_len = read_int(br);
                ret.Add(data_init);

                for (int j = 0; j < data_len; j++)
                {
                    //  int data = read_int(br);
                    byte bs = br.ReadByte();
                    computer.memory[data_init] = bs;
                    data_init++;

                }
            }
            int main_address = read_int(br);
            ret.Add(main_address);

            int text_num = read_int(br);


            ret.Add(text_num);

            for (int i = 0; i < text_num; i++)
            {
                int text_init = read_int(br);
                int text_len = read_int(br);
                ret.Add(text_init);

                int end = text_len / 4;
                //保证为4的倍数
                System.Diagnostics.Debug.Assert(text_len % 4 == 0);
                //ret.Add(0); // The number of TRAP 0
                for (int j = 0; j < end; j++)
                {
                    int text = read_int(br);
                    computer.memory.WriteWord(new Word(text), text_init);
                    if (new Word(text).Value == unchecked((int)0xC0000000))
                    {
                        //MessageBox.Show(ret.Count + " " + (1 + data_num + 1 + 1 + text_num));
                        //ret[1 + data_num + 1 + 1 + text_num] += 1;
                        //ret.Add(text_init);
                        trap.Add(text_init);
                    }
                    text_init += 4;

                }
            }
            ret.Add((Int32)trap.Count);
            foreach (Int32 v in trap)
            {
                ret.Add(v);
            }

            br.Close();
            return ret;
            //Console.WriteLine();
            //Console.WriteLine("Finish!");
            //Console.ReadKey();

        }
        private object Exception(string p)
        {
            throw new NotImplementedException();
        }
    }
}
