using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace VM
{
    public class Memory
    {
        private Hashtable memory = new Hashtable();
        private static int[] important_address = { unchecked((int)0xffff0007)};
        public delegate void deleModified(object sender, object[] args);
        public delegate void deleAccessed(int addr);
        public event deleModified Modified;
        public event deleAccessed Accessed;
        //reset the memory to null
        public void reset()
        {
            this.memory.Clear();
        }
        public byte this[int n]
        {
            get
            {
                if (Accessed != null)
                    for (int i = 0; i < important_address.Length; i++)
                        if (n == important_address[i])
                            Accessed(n);
                /*
                try
                {
                    return (byte)memory[n];
                }
                catch (NullReferenceException nfe)
                {
                    return (byte)0;
                }
                 */
                if (memory.ContainsKey(n))
                {
                    return (byte)memory[n];
                }
                else
                {
                    return (byte)0;
                }
            }
            set
            {
                memory[n] = (byte)value;
                if (Modified != null)
                    Modified(this, new object[] { n, (byte)value });
            }
        }
        public Word GetWord(int addr)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                sb.Append(Computer.byte2str(this[addr + i]));
            }
            return new Word(sb.ToString());
        }
        public void WriteWord(Word w, int addr)
        {
            int v = w.Value;
            for (int i = 24; i >= 0; i -= 8)
            {
                this[addr + (24 - i) / 8] = (byte)(v >> i & 0xFF);
            }
        }
        public void SetByte(int address, byte v)
        {
            memory[address] = (byte)v;
        }
        public byte getByte(int address)
        {
            return (byte)memory[address];
        }
    }
}
