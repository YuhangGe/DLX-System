using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM
{
    public class Word
    {
        #region 8.17
        public delegate void deleModified(object sender, object[] args);
        public event deleModified Modified;
        private int value;
        public int Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
                
                if (Modified != null)
                {
                    Modified(this, new object[] { });
                }
                
            }
        }
        private int tag;
        public int Tag
        {
            get
            {
                return tag;
            }
            set
            {
                tag = value;
            }
        }
        #endregion
        public Word(String value)
        {
            int s;
            int l = value.Length;
            s = (int)(-(int)(value[0] - '0') * Math.Pow(2, (l - 1)));
            for (int i = 1; i < value.Length; i++)
            {
                s += (int)((value[i] - '0') * Math.Pow(2, (l - 1 - i)));
            }
            this.value = s;
        }
        public Word(int value)
        {
            this.value = value;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 31; i >= 0; i--)
            {
                sb.Append(((this.value >> i) & 1).ToString());
            }
            return sb.ToString();
        }

        #region operator overriding
        public int this[int n]
        {
            get
            {
                return (this.value >> n) & 1;
            }
            set
            {
                if (value > 0)
                { 
                    this.value = this.value|(1<<(n));
                }
                else
                {
                    this.value = this.value & ~(1<<(n));
                }
            }
        }
        public static Word operator +(Word word1, Word word2)
        {
            return new Word(word1.value + word2.value);
        }
        public static Word operator -(Word w1, Word w2)
        {
            return new Word(w1.value - w2.value);
        }
        public static Word operator *(Word w1, Word w2)
        {
            return new Word(w1.value * w2.value);
        }
        public static Word operator &(Word w1, Word w2)
        {
            return new Word(w1.value & w2.value);
        }
        public static Word operator |(Word w1, Word w2)
        {
            return new Word(w1.value | w2.value);
        }
        #endregion
    }
}
