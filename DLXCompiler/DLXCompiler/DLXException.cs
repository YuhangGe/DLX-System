using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DLXAssembler
{
    /// <summary>
    /// 双向链表，用于处理编译过程中出现的错误
    /// </summary>
    class DLXException: Exception 
    {
        public  int Line;//错误所在行
        public  int Colum;//错误所在列
        private string msg;//错误信息
        private DLXException next;//下一个错误
        private DLXException previous;//上一个错误
        private bool is_dead = false;//是否是致命错误
        public DLXException(string msg,int line,int colum,bool is_dead=false )
        {
            this.Line = line;
            this.Colum = colum;
            this.msg = msg;
            this.next = null;
            this.previous = null;
            this.is_dead = is_dead;
        }
        public DLXException NextException
        {
            get { return this.next; }
            set { this.next = value; }
        }
        public DLXException PreException
        {
            get { return this.previous; }
            set { this.previous = value; }
        }
        public  bool IsDead
        {
            get { return this.is_dead; }
            set { this.is_dead = value; }
        }
        public override string Message
        {
            get
            {
                return this.msg ;
            }
        }
    }
}
