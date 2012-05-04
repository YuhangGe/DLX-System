using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Diagnostics;

namespace VM
{
    public class Computer
    {
        /* 已经添加时钟中断，中断号同8086处理器一致，为0x1C。
         * 1000条指令中断一次（暂定），但还没有测试，需要配合。
         * 暂时还无法进行关中断，因为我们不能另外添加寄存器，否则
         * 现有的指令集将无法访问该寄存器（除非做寄存器映射）。鉴于
         * 通用寄存器有32个，可以从尾部开始挪用，作为保留寄存器，因此
         * 可以添加诸如FLAG之类的寄存器。
         */
        public delegate void deleRegModified(int n);
        public event deleRegModified RegModified;
        public int timer = 1000;
        public ArrayList breakpoints;
        #region OS
        /* Base Register */
        private Word BR = new Word(0);
        /* Length Register */
        private Word LR = new Word(100);
        #endregion
        public static String byte2str(Byte b)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 7; i >= 0; i--)
            {
                sb.Append((b >> i & 1).ToString());
            }
            return sb.ToString();
        }
        private static byte str2byte(String s)
        {
            byte result = 0;
            for (int i = 7; i >= 0; i--)
            {
                result = (byte)(Math.Pow(2, i) * (s[7 - i] - '0') + result);
            }
            return result;
        }
        public void Load(String[] code, int start)  //load the program into the memory. The start must be 4n!!!
        {
            for (int i = 0; i < code.Count(); i++)
            {
                memory[start + i * 4] = str2byte(code[i].Substring(0, 8));
                memory[start + i * 4 + 1] = str2byte(code[i].Substring(8, 8));
                memory[start + i * 4 + 2] = str2byte(code[i].Substring(16, 8));
                memory[start + i * 4 + 3] = str2byte(code[i].Substring(24, 8));
            }
        }
        public void Load(byte[] code, int start)
        {
            for (int i = 0; i < code.Count(); i++)
            {
                memory[start + i] = code[i];
            }
        }
        //public void WriteWord(Word w, int addr)
        //{
        //    int v = w.Value;
        //    for (int i = 24; i >= 0; i -= 8)
        //    {
        //        this.memory[addr + (24 - i) / 8] = (byte)(v >> i & 0xFF);
        //    }
        //}
        //public Word GetWord(int addr)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    for (int i = 0; i < 4; i++)
        //    {
        //        sb.Append(byte2str(memory[addr + i]));
        //    }
        //    return new Word(sb.ToString());
        //}

        private bool DEBUG = false;
        //public Word INTR;
        private void Fetch()
        {
            int pc = PC.Value;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                sb.Append(byte2str(memory[pc + i]));
            }
            PC.Value += 4;
            IR = new Word(sb.ToString());
        }
        //private void Check()
        //{
        //    Type t = this.GetType();
        //    if (INTR.Value != 0)
        //    {
        //        MethodInfo m = t.GetMethod("intr_" + INTR.Value.ToString());
        //        try
        //        {
        //            m.Invoke(this, new Object[] { });
        //        }
        //        catch (NullReferenceException nre)
        //        {
        //        }
        //        catch (TargetInvocationException tie)
        //        {
        //            INTR.Value = 0;
        //            throw tie.InnerException;
        //        }
        //        finally
        //        {
        //            INTR.Value = 0;
        //        }
        //    }
        //}
        //public void intr_1()
        //{
        //    /* STOP */
        //    for (int i = 0; i < 32; i++)
        //    {
        //        R[i].Value = 0;
        //    }
        //    INTR.Value = 0;
        //    throw new Exception("STOP");
        //}
        #region Handlers
        private void Handle()
        {
            Type t = this.GetType();
            if (IR.ToString().Substring(0, 6) == "000000")
            {
                String m_name = "Handle_R_" + IR.ToString().Substring(26);
                MethodInfo m = t.GetMethod(m_name);
                try
                {
                    m.Invoke(this, new Object[] { });
                }
                catch (NullReferenceException nre)
                {
                    Debug.WriteLine(nre.Message);
                }
            }
            else
            {
                MethodInfo m = t.GetMethod("Handle_IJ_" + IR.ToString().Substring(0, 6));
                try
                {
                    m.Invoke(this, new Object[] { });
                }
                catch (NullReferenceException nre)
                {
                    Debug.WriteLine(nre.Message);
                }
            }
        }
        public void Handle_R_000001()   //ADD
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int SR2 = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            int DR = new Word("0" + IR.ToString().Substring(16, 5)).Value;
            R[DR].Value = (R[SR1] + R[SR2]).Value;
        }
        public void Handle_IJ_000001()  //ADDI
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int imm = new Word(IR.ToString().Substring(16, 16)).Value;
            int DR = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            R[DR].Value = R[SR1].Value + imm;
        }
        public void Handle_R_000011()  //Sub
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int SR2 = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            int DR = new Word("0" + IR.ToString().Substring(16, 5)).Value;
            R[DR].Value = (R[SR1] - R[SR2]).Value;
        }
        public void Handle_IJ_000011()  //SubI
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int imm = new Word(IR.ToString().Substring(16, 16)).Value;
            int DR = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            R[DR].Value = R[SR1].Value - imm;
        }
        public void Handle_R_001001()  //AND
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int SR2 = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            int DR = new Word("0" + IR.ToString().Substring(16, 5)).Value;
            R[DR].Value = (R[SR1] & R[SR2]).Value;
        }
        public void Handle_IJ_001001()  //ANDI
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int imm = new Word(IR.ToString().Substring(16, 16)).Value;
            int DR = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            R[DR].Value = R[SR1].Value & imm;
        }
        public void Handle_R_001010()  //OR
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int SR2 = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            int DR = new Word("0" + IR.ToString().Substring(16, 5)).Value;
            R[DR].Value = (R[SR1] | R[SR2]).Value;
        }
        public void Handle_IJ_001010()  //ORI
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int imm = new Word(IR.ToString().Substring(16, 16)).Value;
            int DR = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            R[DR].Value = R[SR1].Value | imm;
        }
        public void Handle_R_001011() //XOR
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int SR2 = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            int DR = new Word("0" + IR.ToString().Substring(16, 5)).Value;
            R[DR].Value = ((R[SR1] | R[SR2]) - (R[SR1] & R[SR2])).Value;
        }
        public void Handle_IJ_001011()  //XORI
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int imm = new Word(IR.ToString().Substring(16, 16)).Value;
            int DR = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            R[DR].Value = (R[SR1].Value | imm) - (R[SR1].Value & imm);
        }
        public void Handle_IJ_001100()  //LHI
        {
            int DR = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            int imm = new Word(IR.ToString().Substring(16, 16)).Value;
            R[DR].Value = new Word(IR.ToString().Substring(16, 16) + "0000000000000000").Value;
        }
        public void Handle_R_001101()  //SLL
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int SR2 = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            int DR = new Word("0" + IR.ToString().Substring(16, 5)).Value;
            R[DR].Value = R[SR1].Value * (int)(Math.Pow(2, R[SR2].Value));

        }
        public void Handle_IJ_001101()  //SLLI
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int imm = new Word(IR.ToString().Substring(16, 16)).Value;
            int DR = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            R[DR].Value = R[SR1].Value * (int)(Math.Pow(2, imm));
            //Console.WriteLine(R[SR1]);
            //Console.WriteLine(R[DR]);
        }
        public void Handle_R_001110()  //SRL
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int SR2 = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            int DR = new Word("0" + IR.ToString().Substring(16, 5)).Value;
            R[DR].Value = R[SR1].Value / (int)(Math.Pow(2, R[SR2].Value));
            String zero = "";
            for (int i = 0; i < R[SR2].Value; i++)
            {
                zero = zero + "0";
            }
            R[DR].Value = new Word(zero + R[DR].ToString().Substring(R[SR2].Value, 32 - R[SR2].Value)).Value;
        }
        public void Handle_IJ_001110()  //SRLI
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int imm = new Word(IR.ToString().Substring(16, 16)).Value;
            int DR = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            String zero = "";
            for (int i = 0; i < imm; i++)
            {
                zero = zero + "0";
            }
            R[DR] = new Word(zero+R[SR1].ToString().Substring(0, 32 - imm));
            //Console.WriteLine(R[SR1]);
            //Console.WriteLine(R[DR]);
        }
        public void Handle_R_001111()  //SRA
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int SR2 = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            int DR = new Word("0" + IR.ToString().Substring(16, 5)).Value;
            int imm = R[SR2].Value;
            String zero = "";
            if (R[SR1].Value >= 0)
            {
                for (int i = 0; i < imm; i++)
                {
                    zero = zero + "0";
                }
            }
            else
            {
                for (int i = 0; i < imm; i++)
                {
                    zero = zero + "1";
                }
            }
            R[DR] = new Word(zero + R[SR1].ToString().Substring(0, 32 - imm));
            Console.WriteLine("S"+R[SR1]);
            Console.WriteLine("R"+R[DR]);
        }
        public void Handle_IJ_001111()  //SRAI
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int imm = new Word(IR.ToString().Substring(16, 16)).Value;
            int DR = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            String zero = "";
            if (R[SR1].Value >= 0)
            {
                for (int i = 0; i < imm; i++)
                {
                    zero = zero + "0";
                }
            }
            else
            {
                for (int i = 0; i < imm; i++)
                {
                    zero = zero + "1";
                }
            }
            R[DR] = new Word(zero + R[SR1].ToString().Substring(0, 32 - imm));
            //Console.WriteLine("S"+R[SR1]);
            //Console.WriteLine("R"+R[DR]);
        }
        public void Handle_R_010000() //SLT
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int SR2 = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            int DR = new Word("0" + IR.ToString().Substring(16, 5)).Value;
            if (R[SR1].Value < R[SR2].Value)
            {
                R[DR].Value = 1;
            }
            else
            {
                R[DR].Value = 0;
            }
        }
        public void Handle_IJ_010000()  //SLTI
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int imm = new Word(IR.ToString().Substring(16, 16)).Value;
            int DR = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            if (R[SR1].Value < imm)
            {
                R[DR].Value = 1;
            }
            else
            {
                R[DR].Value = 0;
            }
        }
        public void Handle_R_010010() //SLE
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int SR2 = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            int DR = new Word("0" + IR.ToString().Substring(16, 5)).Value;
            if (R[SR1].Value <= R[SR2].Value)
            {
                R[DR].Value = 1;
            }
            else
            {
                R[DR].Value = 0;
            }
        }
        public void Handle_IJ_010010()  //SLEI
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int imm = new Word(IR.ToString().Substring(16, 16)).Value;
            int DR = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            if (R[SR1].Value <= imm)
            {
                R[DR].Value = 1;
            }
            else
            {
                R[DR].Value = 0;
            }
        }
        public void Handle_R_010100()  //SEQ
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int SR2 = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            int DR = new Word("0" + IR.ToString().Substring(16, 5)).Value;
            if (R[SR1].Value == R[SR2].Value)
            {
                R[DR].Value = 1;
            }
            else
            {
                R[DR].Value = 0;
            }
        }
        public void Handle_IJ_010100()  //SEQI
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int imm = new Word(IR.ToString().Substring(16, 16)).Value;
            int DR = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            if (R[SR1].Value == imm)
            {
                R[DR].Value = 1;
            }
            else
            {
                R[DR].Value = 0;
            }
        }
        public void Handle_R_010101()  //SNE
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int SR2 = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            int DR = new Word("0" + IR.ToString().Substring(16, 5)).Value;
            if (R[SR1].Value != R[SR2].Value)
            {
                R[DR].Value = 1;
            }
            else
            {
                R[DR].Value = 0;
            }
        }
        public void Handle_IJ_010101()  //SNEI
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int imm = new Word(IR.ToString().Substring(16, 16)).Value;
            int DR = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            if (R[SR1].Value != imm)
            {
                R[DR].Value = 1;
            }
            else
            {
                R[DR].Value = 0;
            }
        }
        public void Handle_IJ_010110()  //LB
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int imm = new Word(IR.ToString().Substring(16, 16)).Value;
            int DR = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            int address = R[SR1].Value + imm;
            R[DR].Value = (new Word(byte2str(memory[address]))).Value;
            /*
                 * 由于hash表的访问有锁机制,内存访问指令可能会卡.
                 * sleep 1毫秒是为了解决这个问题.
                 * 在lw, sw, lb, sb 这四条指令中都需要这个操作.
                 * */
            System.Threading.Thread.Sleep(1);
        }
        public void Handle_IJ_010111()  //SB
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int imm = new Word(IR.ToString().Substring(16, 16)).Value;
            int DR = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            int address = R[SR1].Value + imm;
            memory[address] = str2byte(R[DR].ToString().Substring(24, 8));
            /*
                * 由于hash表的访问有锁机制,内存访问指令可能会卡.
                * sleep 1毫秒是为了解决这个问题.
                * 在lw, sw, lb, sb 这四条指令中都需要这个操作.
                * */
          //  System.Threading.Thread.Sleep(1);
        }
        public void Handle_IJ_011100()  //LW
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int imm = new Word(IR.ToString().Substring(16, 16)).Value;
            int DR = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            int address = R[SR1].Value + imm;
            if (address % 4 == 0)
            {
                R[DR]=memory.GetWord(address);
                //R[DR].Value = (new Word(byte2str(memory[address]) + byte2str(memory[address + 1]) + byte2str(memory[address + 2]) + byte2str(memory[address + 3]))).Value;
            }
            else
            {
                /*
                 * 异常处理
                 * */
            }
            /*
             * 由于hash表的访问有锁机制,内存访问指令可能会卡.
             * sleep 1毫秒是为了解决这个问题.
             * 在lw, sw, lb, sb 这四条指令中都需要这个操作.
             * */
            System.Threading.Thread.Sleep(1);
        }
        public void Handle_IJ_011101()  //SW
        {
            int DR = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            int imm = new Word(IR.ToString().Substring(16, 16)).Value;
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int address = R[SR1].Value + imm;
            if (address % 4 == 0)
            {
                memory.WriteWord(R[DR],address);
                //memory[address] = str2byte(R[DR].ToString().Substring(0, 8));
                //memory[address + 1] = str2byte(R[DR].ToString().Substring(8, 8));
                //memory[address + 2] = str2byte(R[DR].ToString().Substring(16, 8));
                //memory[address + 3] = str2byte(R[DR].ToString().Substring(24, 8));
            }
            else
            {
                /*
                 * 异常处理
                 * */
            }
            /*
            * 由于hash表的访问有锁机制,内存访问指令可能会卡.
            * sleep 1毫秒是为了解决这个问题.
            * 在lw, sw, lb, sb 这四条指令中都需要这个操作.
            * */
         //   System.Threading.Thread.Sleep(1);
        }
        public void Handle_IJ_101000()  //BEQZ
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int imm = new Word(IR.ToString().Substring(16, 16)).Value;
            if (R[SR1].Value == 0)
            {
                PC.Value = PC.Value + imm;
                /*
                 * 关于内存4的倍数的问题
                 * */
            }
           // System.Threading.Thread.Sleep(1);
        }
        public void Handle_IJ_101001()  //BNEZ
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int imm = new Word(IR.ToString().Substring(16, 16)).Value;
            if (R[SR1].Value != 0)
            {
                PC.Value = PC.Value + imm;
                /*
                 * 关于内存4的倍数的问题
                 * */
            }
        }
        public void Handle_IJ_101100()  //J
        {
            int imm = new Word(IR.ToString().Substring(6, 26)).Value;
            PC.Value = PC.Value + imm;
            /*
                 * 关于内存4的倍数的问题
                 * */
        }
        public void Handle_IJ_101101()  //JR
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            PC.Value = R[SR1].Value;
            /*
                 * 关于内存4的倍数的问题
                 * */
        }
        public void Handle_IJ_101110() //JAL
        {
            int imm = new Word(IR.ToString().Substring(6, 26)).Value;
            R[31].Value = PC.Value;
            PC.Value = PC.Value + imm;
        }
        public void Handle_IJ_101111() //JALR
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            R[31].Value = PC.Value;
            PC.Value = R[SR1].Value;
        }
        public void Handle_IJ_110000() //TRAP
        {
            R[31].Value = PC.Value;
            int imm = new Word(IR.ToString().Substring(6, 26)).Value;
            imm <<= 2;
            // TRAP向量表中存的地址是数据段的起始地址，代码段需要加上0x100
            PC.Value = this.memory.GetWord(imm).Value + 0x100;
        }
        public void Handle_IJ_100010() //MOVI2S
        {
            //Console.WriteLine("asfd");
            int SR = new Word("0" + IR.ToString().Substring(16, 5)).Value;
            int Special_R = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            Console.WriteLine(Special_R);
            switch (SR)
            { 
                case 12:
                    this.SR = new Word(this.R[Special_R].Value);
                    break;
                case 13:
                    this.CAUSE = new Word(this.R[Special_R].Value);
                    break;
                case 14:
                    this.EPC = new Word(this.R[Special_R].Value);
                    break;
            }
        }
        public void Handle_IJ_100011() //MOVS2I
        {
            int SR = new Word("0" + IR.ToString().Substring(16, 5)).Value;
            int Special_R = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            switch (SR)
            {
                case 12:
                    this.R[Special_R] = new Word(this.SR.Value);
                    break;
                case 13:
                    this.R[Special_R] = new Word(this.CAUSE.Value);
                    break;
                case 14:
                    this.R[Special_R] = new Word(this.EPC.Value);
                    break;
            }
        }
        public void Handle_IJ_110001() //RFE
        {
            this.SR[0] = this.SR[2];
            this.PC = new Word(this.EPC.Value);
        }
        #endregion
        public Word[] R = new Word[32];
        public Memory memory = new Memory();
        public Word PC = new Word(0);
        public Word IR = new Word(0);
        public Word CAUSE = new Word(0);
        public Word SR = new Word(0);
        public Word EPC = new Word(0);
        public Word KBSR = new Word(0);
        public Word DSR = new Word(0);
        public Word TMCR = new Word(0);
        public int INT = 0;
        public Computer()
        {
            breakpoints = new ArrayList();
            for (int i = 0; i < 32; i++)
            {
                R[i] = new Word(0);
                R[i].Tag = i;
                R[i].Modified += new Word.deleModified(word_Modified);
            }
            //INTR = new Word(0);
            this.memory.Modified += new Memory.deleModified(memory_Modified);
            this.RegModified += new deleRegModified(Computer_RegModified);
            //InitTrapTable();
            //Enable all the interrupts
            this.SR[0]= 1;
        }

        //reset the memory to null
        public void reset()
        {
            this.memory.reset();
            this.PC = new Word(0);
            this.IR = new Word(0);
            this.CAUSE = new Word(0);
            this.SR = new Word(0);
            this.EPC = new Word(0);
            this.KBSR = new Word(0);
            this.DSR = new Word(0);
            this.TMCR = new Word(0);
            for (int i = 0; i < 32; i++)
            {
                this.R[i] = new Word(0);
            }
            //Enable all the interrupts
            this.SR[0] = 1;
        }

        /* InitTrapTable should be called
         * after memory event being registered
         */
        public void InitTrapTable()
        {
            int ROUTING_START = 0x1000;
            int ROUTING_DELTA = 0x400;
            memory.WriteWord(new Word("11111111111111100000000000000000"), 0);
            for (int i = 1; i < 512; i++)
            {
                int value = ROUTING_START + (i - 1) * ROUTING_DELTA;
                memory.WriteWord(new Word(value), i * 4);
            }   
        }
        void Computer_RegModified(int n)
        {
            Debug.WriteLine(n);
        }

        void word_Modified(object sender, object[] args)
        {
            Word r = (Word)sender;
            if (this.RegModified != null)
            {
                this.RegModified(r.Tag);
            }
        }

        void memory_Modified(object sender, object[] args)
        {
            Debug.WriteLine(string.Format("Memory {0} is modified to {1}", args[0], args[1]));
        }
        public void Interrupt()
        {
            timer--;
            if (timer <= 0)
            {
                //System.Threading.Thread.Sleep(1);
                //R[31].Value = PC.Value;
                // TRAP向量表中存的地址是数据段的起始地址，代码段需要加上0x100
                //PC.Value = this.memory.GetWord(0x1c << 2).Value + 0x100;
                //timer = 1000;
                this.TMCR[0] = 1;
                timer = memory.GetWord(unchecked((int)0xFFFF0014)).Value;
                if (timer <= 0)
                {
                    timer = 1000;
                }
            }

            //this.CAUSE[10] = (this.memory[unchecked((int)0xFFFF0008)] & 1) & (this.memory[unchecked((int)0xFFFF0008)] >> 1 & 1);
            //this.CAUSE[11] = (this.memory[unchecked((int)0xFFFF0000)] & 1) & (this.memory[unchecked((int)0xFFFF0000)] >> 1 & 1);
            //this.CAUSE[15] = (this.memory[unchecked((int)0xFFFF0010)] & 1) & (this.memory[unchecked((int)0xFFFF0010)] >> 1 & 1) & (this.memory[unchecked((int)0xFFFF0010)] >> 2 & 1);
            this.CAUSE[10] = this.DSR[0] & this.DSR[1];
            this.CAUSE[11] = this.KBSR[0] & this.KBSR[1];
            this.CAUSE[15] = this.TMCR[0] & this.TMCR[1] & this.TMCR[2];
            this.INT = this.CAUSE[8] | this.CAUSE[9] | this.CAUSE[10] | this.CAUSE[11] | this.CAUSE[12] | this.CAUSE[13] | this.CAUSE[14] | this.CAUSE[15];
            this.INT = this.INT & this.SR[0];

            if (this.INT>0)
            {
                this.EPC = new Word(this.PC.Value);
                this.SR[2] = this.SR[0];
                this.SR[0] = 0;
                this.PC = new Word(unchecked((int)0x80001000));
            }
        }

        public void Execute()
        {
            //Debug.WriteLine("aaaa");
            //Check();
            Fetch();
            Handle();
            Interrupt();
        }
        #region debugger
        public void ToggleBreakpoint(int addr)
        {
            if (breakpoints.IndexOf(addr) == -1)
            {
                breakpoints.Add(addr);
            }
        }
        public void RemoveBreakpoint(int addr)
        {
            breakpoints.Remove(addr);
        }
        public void Continue()
        {
            while (breakpoints.IndexOf(PC.Value) == -1)
            {
                Execute();
            }
        }
        public void StepOver()
        {
            int target = PC.Value + 4;
            while (PC.Value != target)
            {
                Execute();
            }
        }
        public void StepIn()
        {
            Execute();
        }
        public void StepOut()
        {
            while (PC.Value != R[31].Value)
            {
                Execute();
            }
        }
        //public void Stop()
        //{
        //    /* run in another thread */
        //    this.INTR.Value = 1;
        //}
        #endregion
        public void onInterrupt()
        {
            this.SR[0] = 1;
        }
        public void offInterrupt()
        {
            this.SR[0] = 0;
        }
    }
}
