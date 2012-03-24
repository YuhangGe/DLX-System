using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abraham
{
    public class DLXException: Exception 
    {
        public  int Line;//错误所在行
        public  int Colum;//错误所在列
        private string msg;//错误信息
        public DLXException(string msg,int line,int colum )
        {
            this.Line = line;
            this.Colum = colum;
            this.msg = msg;
        }
       
        public override string Message
        {
            get
            {
                return this.msg ;
            }
        }
        public override string ToString()
        {
            return string.Format("错误出现在第{0}行{1}列：{2}", this.Line, this.Colum, this.msg);
        }
    }
}
