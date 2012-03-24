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
            this.setMemoryValue(SmallTool.UinttoInt(4294901763), 1);
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
                    Debug.WriteLine("Register Changed" + this.stepoutFunLevel);
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
        public void aaa()
        {
            this.computer.memory.WriteWord(new Word(10), 0);
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
            if (((int)args[0]) == SmallTool.UinttoInt(SmallTool.StringLocationParse("xFFFF00FB")))
            {
                if ((byte)args[1] == 1)
                {
                    this.instructionsNumber = 0;
                    this.setMemoryValue(SmallTool.UinttoInt(4294901763), 0);
                }
                else
                {
                    this.setMemoryValue(SmallTool.UinttoInt(4294901763), 1);
                }
                if (this.HaltChangeEvent != null)
                    this.HaltChangeEvent(this, args);
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
            int[] a = new int[34];
            a[0] = this.computer.PC.Value;
            a[1] = this.computer.IR.Value;
            for (int i = 0; i < 32; i++)
                a[i + 2] = this.computer.R[i].Value;
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
            lock (this)
            {
                do
                {

                    NullParameterDelegate fun = new NullParameterDelegate(this.computer.Execute);
                    fun.Invoke();
                    this.instructionsNumber++;
                    Debug.WriteLine(this.stepoutFunLevel);
                    //button.Dispatcher.Invoke(fun);
                    ChildFormControl.getInstance().getMemoryPanel().instructionCount(this.instructionsNumber);
                } while (this.OperationTest());
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
            if (this.computer.memory[SmallTool.UinttoInt(SmallTool.StringLocationParse("xFFFF00FB"))] == 1)
                return true;
            else
            {
                ChildFormControl.getInstance().getMemoryPanel().stateShowDirect("Halt.");
                return false;
            }
        }
        private bool debugTest()
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
                        Debug.WriteLine(this.stepoutFunLevel + "");
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
                    Debug.WriteLine(this.stepoutFunLevel + "");
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
            StreamReader sr = new StreamReader(path,Encoding.GetEncoding("GB2312"));
            String line;
            line = sr.ReadLine();
            int addr = new Word(line).Value;
            ret.Add(0);  ret.Add(new Word(line).Value); ret.Add(0); 
            while((line=sr.ReadLine())!=null)
            {
                if (line.Length > 0)
                    cache.Add(line.Substring(0, 32));
            }
            if (_check(cache))
            {
                foreach (String s in cache)
                {
                    computer.memory.WriteWord(new Word(s), addr);
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
            BinaryReader br = new BinaryReader(File.OpenRead(path));
           
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
                    computer.memory[data_init]=bs;
                    data_init ++;

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
                for (int j = 0; j < end; j++)
                {
                    int text = read_int(br);
                    computer.memory.WriteWord(new Word(text), text_init);
                    text_init += 4;

                }
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
