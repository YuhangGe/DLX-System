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
        /* 修改了此函数的实现.出于性能方面的考虑,避免在int型与String之间的转换.
         * Shore Ray */
        public Word GetWord(int addr)
        {
            UInt32 uWord = 0;
            UInt32 temp = 0;
            for (int i = 0; i < 4; i++)
            {
                temp = this[addr + i];
                temp = temp & 0xFF;
                uWord = uWord << 8;
                uWord = uWord | temp;
            }
            return new Word((int)uWord);
            /*
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                sb.Append(Computer.byte2str(this[addr + i]));
            }
            return new Word(sb.ToString());
             */
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
