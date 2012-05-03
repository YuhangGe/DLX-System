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
        public delegate void deleRegModified(int n);
        public event deleRegModified RegModified;
        public ArrayList breakpoints;
        #region OS
        /* Base Register */
        private Word BR = new Word(0);
        /* Length Register */
        private Word LR = new Word(100);
        #endregion
        private static String byte2str(Byte b)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 7; i >= 0; i--)
            {
                sb.Append((b >> i & 1).ToString());
            }
            return sb.ToString();
        }
        public void WriteWord(Word w, int addr)
        {
            int v = w.Value;
            for (int i = 24; i >= 0; i -= 8)
            {
                this.memory[addr + (24 - i) / 8] = (byte)(v >> i & 0xFF);
            }
        }
        public Word GetWord(int addr)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                sb.Append(byte2str(memory[addr + i]));
            }
            return new Word(sb.ToString());
        }

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
                    m.Invoke(this, new Object[]{});
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
        public void Handle_R_000001()
        {
            int SR1 = new Word("0" + IR.ToString().Substring(6, 5)).Value;
            int SR2 = new Word("0" + IR.ToString().Substring(11, 5)).Value;
            int DR = new Word("0" + IR.ToString().Substring(16, 5)).Value;
            R[DR].Value = (R[SR1] + R[SR2]).Value;
        }
        #endregion
        public Word[] R = new Word[32];
        public Memory memory = new Memory();
        public Word PC = new Word(0);
        public Word IR = new Word(0);
        public Computer()
        {
            breakpoints = new ArrayList();
            for (int i = 0; i < 32; i++)
            {
                R[i] = new Word(0);
                R[i].Modified += new Word.deleModified(word_Modified);
            }
            //INTR = new Word(0);
            this.memory.Modified += new Memory.deleModified(memory_Modified);
            this.RegModified += new deleRegModified(Computer_RegModified);
            InitTrapTable();
        }
        private void InitTrapTable()
        {
            int ROUTING_START = 0x1000;
            int ROUTING_DELTA = 0x400;
            WriteWord(new Word("11111111111111100000000000000000"), 0);
            for (int i = 1; i < 512; i++)
            {
                int value = ROUTING_START + (i - 1) * ROUTING_DELTA;
                WriteWord(new Word(value), i * 4);
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
        public void Execute()
        {
            //Check();
            Fetch();
            Handle();
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
                Execute();
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

    }
}
