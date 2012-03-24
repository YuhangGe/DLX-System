//#define debug  //是否打印调试信息


#define meaning //通过宏meaning，表明当前代码段是语义检查
//如果没有定义宏meaning ,代码只执行词法检查


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
namespace DLXAssembler
{
    public partial class CompilerCore
    {
#if debug
        private void Debug(string msg)
        {
            Console.WriteLine("-debug out: " + msg);
        }
#endif
        //用于存储行列值的结构
        private struct row_colum
        {
            public int row;
            public int colum;
            public row_colum(int row, int colum)
            {
                this.row = row;
                this.colum = colum;
            }
        }
#if meaning
        private uint location_counter = 0;//地址记数器
#endif
        //返回的汇编结果
        private DLXObject result = null;

        //一个符号的最大长度
        private const int MAX_WORD_LEN = 50;

        //.Space最大数
        private const int MAX_SPACE_NUM = Int16.MaxValue;

        //最多检测的非致命错误，如果超过，停止编译
        public const int MAX_ERROR_ALLOW = 5;


        //错误链表的头指针
        private DLXException dlx_errors = null;
        //错误链表的尾指针
        private DLXException tailer_error = null;

        //16位整型的上界，用于16进制换算成10进制
        private const int UP_NUM = 65536;

        private Stream src_stream;

        //指令操作符类型
        private enum WORDTYPE
        {
            ALI,//I-类型算术/逻辑运算
            ALR,//R-类型算术/逻辑运算
            LHI,//LHI指令
            LOAD,//数据传送
            STORE,//数据传送
            CB,//条件分支
            TRAP,//TRAP指令
            JMPI,//使用立即数的跳，包括J和JAL
            JMPR,//使用寄存器的跳，包括JR和JALR
            RET,//从JUMP返回,等价于 JR R31
            RFE,//从异常返回
            LABEL,//    标记
            MOVE //传送指令
        }
        //指令操作符
        private string[] KEYWORDS = new string[]{
	         "ADD","ADDI","SUB","SUBI",
             "AND","ANDI","OR","ORI",
             "XOR","XORI","SLL","SLLI",
             "SRL","SRLI", "SRA","SRAI",
             "SLT","SLTI","SLE","SLEI",
             "SEQ","SEQI","SNE","SNEI",
              "LHI",
			 "LB","SB","LW","SW",
             "BEQZ","BNEZ","J","JAL",
             "JR","JALR","TRAP","RFE",
              "MOVI2S","MOVS2I",
             "RET"};

        //Trap向量表
        private Dictionary<string, int> trap_table = new Dictionary<string, int>() { 
               {"GETC",6},{"OUT",7},{"PUTS", 8},{"IN", 9},{"GETS", 10},{"HALT", 0}
        };
    //特殊寄存器
       private Dictionary<string, int> special_register = new Dictionary<string, int>()
       {
           {"SR",0xC},{"CAUSE",0xD},{"EPC",0xE}
       };

       private bool ignore_case = true;
        public CompilerCore()
        {

        }
        /// <summary>
        ///  得到符号在操作符表中的位置
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private int get_word_index(string word)
        {

            for (int i = 0; i < KEYWORDS.Length; i++)
                if (word.ToUpper() == KEYWORDS[i])
                    return i;
            return -1;
        }
        //返回符号的类型(如果是指令，返回指令类型；否则返回Label)
        private WORDTYPE get_word_type(string src_word)
        {
            int index = get_word_index(src_word);
            if (index >= 0 && index <= 23)
                if (index % 2 == 0)
                    return WORDTYPE.ALR;
                else
                    return WORDTYPE.ALI;
            else if (index == 24)
                return WORDTYPE.LHI;
            else if (index == 25 || index == 27)
                return WORDTYPE.LOAD;
            else if (index == 26 || index == 28)
                return WORDTYPE.STORE;
            else if (index == 29 || index == 30)
                return WORDTYPE.CB;
            else if (index == 31 || index == 32)
                return WORDTYPE.JMPI;
            else if (index == 33 || index == 34)
                return WORDTYPE.JMPR;
            else if (index == 35)
                return WORDTYPE.TRAP;
            else if (index == 36)
                return WORDTYPE.RFE;
            else if (index == 37 || index == 38)
                return WORDTYPE.MOVE;
            else if (index == 39)
                return WORDTYPE.RET;
            else
                return WORDTYPE.LABEL;
        }
        private void push_convert(string str)
        {
            result.textContent.Append(str.ToUpper());
        }
        /// <summary>
        /// 查找标记是否已经声明
        /// </summary>
        /// <param name="word">标记名称</param>
        /// <param name="index">标记后面的offset(通常只对data断的数组标记不为0)</param>
        /// <returns> </returns>
        private bool check_symbol(string word, int index)
        {
            return check_data_symbol(word, index) || check_text_symbol(word);
        }
        /// <summary>
        /// 只在Data段中查找标记是否已经声明
        /// </summary>
        /// <param name="word"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool check_data_symbol(string word, int index)
        {
            Symbol s = null;
            if ((s = result.dataTable.getSymbol(word)) != null)
            {
                if (!s.IsExtern && index > s.Value.Count - 1)
                    error(string.Format("标记偏移量{0}溢出，{1}的最大偏移为{2}", index, word, s.Value.Count - 1));
                return true;
            }
            return false;
        }
        private bool check_data_symbol(string word)
        {
            return result.dataTable.getSymbol(word) != null;
        }
        /// <summary>
        /// 只在text段中查找标记是否已经声明
        /// 用于跳转指令，这些指令后面跟的标记只能是程序段中声明的标记
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private bool check_text_symbol(string word)
        {
            if (result.textTable.getSymbol(word) != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 辅助函数，把32位int转为4个8位byte
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static byte[] uint_to_bytes(uint num)
        {
            byte[] rtn = new byte[4];
            rtn[3] = (byte)(num & 0x000000ff);
            rtn[2] = (byte)((num & 0x0000ff00) >> 8);
            rtn[1] = (byte)((num & 0x00ff0000) >> 16);
            rtn[0] = (byte)((num & 0xff000000) >> 24);
            return rtn;
        }
        /// <summary>
        /// 辅助函数，把32位int转为4个8位byte
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static byte[] int_to_bytes(int num)
        {
            byte[] rtn = new byte[4];
            rtn[3] = (byte)(num & 0x000000ff);
            rtn[2] = (byte)((num & 0x0000ff00) >> 8);
            rtn[1] = (byte)((num & 0x00ff0000) >> 16);
            rtn[0] = (byte)((num & 0xff000000) >> 24);
            return rtn;
        }
        /// <summary>
        /// 把4个8位char合并成一个32位int
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int char_to_int(char[] c)
        {
#if debug
            System.Diagnostics.Debug.Assert(c.Length == 4);
#endif
            return (int)((((byte)c[0]) << 24) | (((byte)c[1]) << 16) | (((byte)c[2]) << 8) | ((byte)c[3]));
        }
        private int line = 1;
        private int line_at = 0;
        private int token_index = -1;
        private char cur_token;
        /// <summary>
        /// 读取下一个字符
        /// </summary>
        /// <returns></returns>
        private char get_src_token()
        {
            token_index++;
            line_at++;

            int read = src_stream.ReadByte();// src_content[token_index];
            //Console.WriteLine(read);
            if (read == -1)
            {
                cur_token = Convert.ToChar(0);
            }
            else
            {
                cur_token = Convert.ToChar(read);
                if (cur_token == '\n')
                {
                    line++;
                    line_at = 0;
                }
            }
            return cur_token;
        }
        /// <summary>
        /// 回退一个字符
        /// </summary>
        /// <returns></returns>
        private void  back_src_token()
        {
            token_index--;
            line_at--;
            src_stream.Seek(-2, SeekOrigin.Current);
            int read = src_stream.ReadByte();
            cur_token = (char)read;
        }
        private int error_num = 0;
        private void delete_last_error()
        {
            tailer_error = tailer_error.PreException;
            if (tailer_error == null)
                dlx_errors = null;
            else
                tailer_error.NextException = null;
            error_num--;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg">错误信息</param>
        /// <param name="is_dead">是否是致命错误,如果是致命错误（如没有text段)，将不会继续汇编。</param>
        private void error(string msg, bool is_dead = false)
        {
            error(msg, line, line_at, is_dead);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg">错误信息</param>
        /// <param name="row">发生错误的行</param>
        /// <param name="colum">发生错误的列</param>
        /// <param name="is_dead">是否是致命错误,如果是致命错误（如没有text段)，将不会继续汇编。</param>
        private void error(string msg, int row, int colum, bool is_dead)
        {
            error_num++;
            if (dlx_errors == null)
            {
                dlx_errors = new DLXException(msg, row, colum, is_dead);
                tailer_error = dlx_errors;
            }
            else
            {

                tailer_error.NextException = new DLXException(msg, row, colum, is_dead);
                tailer_error.NextException.PreException = tailer_error;
                tailer_error = tailer_error.NextException;
                tailer_error.NextException = null;
            }
            throw dlx_errors;
        }
        /// <summary>
        /// 是否是合法的字符
        /// 合法字符包括字母、数字、_和$
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private bool is_validate_token(char ch)
        {
            if (char.IsLetterOrDigit(ch))
                return true;
            if (ch == '_' || ch == '$')
                return true;
            return false;
        }
        /// <summary>
        /// 得到一个字符串，可能是指令操作码（Add,Lw等),也可能是label,
        /// 但不能以字母开头
        /// </summary>
        /// <returns></returns>
        private string get_word()
        {
            string rtn = "";
            int i = 0;
            while (cur_token != ' ' && cur_token != '\t' && cur_token != 0 && cur_token != '\n' && cur_token != '\r')
            {
                if (is_validate_token(cur_token) == false)
                {
                    if (cur_token == ':' || cur_token == '[' || cur_token == '(' || cur_token == '\'' || cur_token == ';' || cur_token == '/')
                        break;
                    error(string.Format("标记中出现非法字符‘{0}'", cur_token));
                }
                else
                {
                    rtn += cur_token;
                    i++;
                    if (i >= MAX_WORD_LEN)
                    {
                        error("标记长度不能超过50！");
                        break;
                    }
                    get_src_token();
                }
            }
            if (string.IsNullOrEmpty(rtn) || Regex.IsMatch(rtn, @"^\s*$"))
                error("指令不完整！需要标记。");
            else if (char.IsDigit(rtn[0]))
                error("标记不能以字母开头");
            return rtn;
            //printf("got word %s\n\n\n",word);
        }
        /// <summary>
        /// 获取数字格式的字符串，
        ///字符串格式为#或x或b开头，分别代表十进制，十六进制和二进制
        /// 也可以直接数字，代表十进制
        /// </summary>
        /// <returns></returns>
        private string get_number_word()
        {
            string rtn = "";
            if (cur_token == 'x' || cur_token == 'X')
            {
                rtn += cur_token;
                get_src_token();
                if (!char.IsDigit(cur_token) &&
                    !(cur_token <= 'f' && cur_token >= 'a') &&
                    !(cur_token <= 'F' && cur_token >= 'A'))
                {
                    back_src_token();
                    error("错误的数字格式");
                }

                while (true)
                {
                    rtn += cur_token;
                    get_src_token();
                    if (!char.IsDigit(cur_token) &&
                    !(cur_token <= 'f' && cur_token >= 'a') &&
                    !(cur_token <= 'F' && cur_token >= 'A'))
                        break;
                }
                return rtn;
            }
            else if (cur_token == 'b' || cur_token == 'B')
            {

                rtn += cur_token;
                get_src_token();
                if (cur_token != '1' && cur_token != '0')
                {
                    back_src_token();
                    error("错误的数字格式");
                }

                while (true)
                {

                    rtn += cur_token;
                    get_src_token();
                    if (cur_token != '1' && cur_token != '0')
                        break;

                }
                return rtn;
            }
            else if (cur_token == '#')
                get_src_token();
            if (!char.IsDigit(cur_token) && cur_token != '-')
                if (cur_token == '\r' || cur_token == '\n')
                    error("指令不完整，需要数字");
                else
                    error("错误的数字格式");
            while (true)
            {
                rtn += cur_token;
                get_src_token();
                if (!char.IsDigit(cur_token))
                    break;
            }
            return rtn;
        }
        private byte parse_number_byte(string num_word)
        {
            try
            {
                char first = num_word[0];
                if (first == 'x' || first == 'X')
                {
                    return Convert.ToByte(num_word.Substring(1), 16);
                }
                else if (first == 'b' || first == 'B')
                {
                    return Convert.ToByte(num_word.Substring(1), 2);
                }
                else if (first == '#')
                {
                    return Convert.ToByte(num_word.Substring(1), 10);
                }
                else
                {
                    return Convert.ToByte(num_word, 10);
                }
            }
            catch
            {
                error("错误的数字范围，需要8位无符号Byte");
                return 0;
            }
        }

        /// <summary>
        /// 将字符串转换为16位整数
        /// 字符串格式为#或x或b开头，分别代表十进制，十六进制和二进制
        /// 也可以直接数字，代表十进制
        /// 字符串格式的正确由获取该字符串的函数 get_number_word保证
        /// </summary>
        /// <param name="num_word"></param>
        /// <returns></returns>
        private int parse_number_16(string num_word)//转换立即数,16
        {
            try
            {
                char first = num_word[0];
                if (first == 'x' || first == 'X')
                {
                    return Convert.ToInt16(num_word.Substring(1), 16);
                }
                else if (first == 'b' || first == 'B')
                {
                    return Convert.ToInt16(num_word.Substring(1), 2);
                }
                else if (first == '#')
                {
                    return Convert.ToInt16(num_word.Substring(1), 10);
                }
                else
                {
                    return Convert.ToInt16(num_word, 10);
                }
            }
            catch
            {
                error("错误的数字，请检查立即数的范围！");
                return 0;
            }

        }
        /// <summary>
        /// 将字符串转换为32位整数
        /// 字符串格式为#或x或b开头，分别代表十进制，十六进制和二进制
        /// 也可以直接数字，代表十进制
        /// 字符串格式的正确由获取该字符串的函数 get_number_word保证
        /// </summary>
        /// <param name="num_word"></param>
        /// <returns></returns>
        private int parse_number_32(string num_word)
        {
            try
            {
                char first = num_word[0];
                if (first == 'x' || first == 'X')
                {
                    return Convert.ToInt32(num_word.Substring(1), 16);
                }
                else if (first == 'b' || first == 'B')
                {
                    return Convert.ToInt32(num_word.Substring(1), 2);
                }
                else if (first == '#')
                {
                    return Convert.ToInt32(num_word.Substring(1), 10);
                }
                else
                {
                    return Convert.ToInt32(num_word, 10);
                }
            }
            catch
            {
                error("错误的数字，请检查数字的范围！");
                return 0;
            }
        }
        /// <summary>
        /// 跳过空白符，包括空格和制表符
        /// </summary>
        private void skip_space()
        {
            while (cur_token == ' ' || cur_token == '\t')
                get_src_token();
        }
        /// <summary>
        /// 出现错误时跳转到下一行语句
        /// </summary>
        private void jump_to_next_line()
        {
            while (cur_token != '\r' && cur_token != '\n' && cur_token != 0)
                get_src_token();
        }
        /// <summary>
        /// 唯一的公开函数，对源进行汇编
        /// </summary>
        /// <param name="source">要汇编的源，可以是继承自Stream的任何源，包括FileStream等</param>
        /// <param name="ignore_case">标识是否忽略大小写，默认忽略</param>
        /// <returns>返回汇编结果DLXObject，用于linker链接</returns>
        public DLXObject Complie(Stream source,bool ignore_case = true)
        {
            if (source == null)
                throw new Exception("the source stream stream can not be null!");
            this.ignore_case = ignore_case;
            src_stream = source;
            result = new DLXObject();


            get_src_token();
            Data();
            Text();
            if (dlx_errors != null)
                throw dlx_errors;
#if debug
            Debug("Success!\n\n");
#endif
            //Debug(cvt_content.ToString());

            return result;
        }
    }
}
