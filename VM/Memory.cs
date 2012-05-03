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
        public delegate void deleModified(object sender, object[] args);
        public event deleModified Modified;
        public byte this[int n]
        {
            get
            {
                try
                {
                    return (byte)memory[n];
                }
                catch (NullReferenceException nfe)
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
    }
}
