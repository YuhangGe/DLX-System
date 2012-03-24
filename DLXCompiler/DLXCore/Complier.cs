

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace DLXCore
{

    public class DLXCompiler
    {
        private  void Debug(string msg)
        {
           // System.Diagnostics.Debug.Print(msg);
            dest_writer.Write(msg);
        }
        private  const int MAX_WORD_LEN = 50;
        private  const int KEYWORDS_NUM = 36;
        private  const int UP_NUM = 65536;

        private StringBuilder src_content;
        private StringBuilder cvt_content;
        private Stream   src_stream;
        private TextWriter  dest_writer;
        private enum WORDTYPE
        {
            ALI,//I-类型算术/逻辑运算
            ALR,//R-类型算术/逻辑运算
            DM,//数据传送
            CB,//条件分支
            TRAP,//TRAP指令
            JMPI,//使用立即数的跳，包括J和JAL
            JMPR,//使用寄存器的跳，包括JR和JALR
            RFE,//从异常返回
            LABEL//    标示 
        }
        private  string[] KEYWORDS = new string[]{
	         "ADD","ADDI","SUB","SUBI","AND",
			 "ANDI","OR","ORI","XOR","XORI",
			 "",//由于LHI没有对应的LHR,此处留空方便统一判断
			 "LHI","SLL","SLLI","SRL","SRLI",
			 "SRA","SRAI","SLT","SLTI","SLE",
			 "SLEI","SEQ","SEQI","SNE","SNEI",
			 "LW","SW","BEQZ","BNEZ","J",
			 "JR","JAL","JALR","TRAP","XFE"};
        private WORDTYPE get_word_type(string src_word)
        {
            int index = -1;
            for (int i = 0; i < KEYWORDS_NUM; i++)
            {
                if (src_word.ToUpper() == KEYWORDS[i])
                {
                    index = i;
                    break;
                }
            }
            if (index >= 0 && index <= 25)
            {
                if (index % 2 == 0)
                    return WORDTYPE.ALR;
                else
                    return WORDTYPE.ALI;
            }
            else if (index == 26 || index == 27)
            {
                return WORDTYPE.DM;
            }
            else if (index == 28 || index == 29)
            {
                return WORDTYPE.CB;
            }
            else if (index == 30 || index == 32)
            {
                return WORDTYPE.JMPI;
            }
            else if (index == 31 || index == 33)
            {
                return WORDTYPE.JMPR;
            }
            else if (index == 34)
            {
                return WORDTYPE.TRAP;
            }
            else if (index == 35)
            {
                return WORDTYPE.RFE;
            }
            return WORDTYPE.LABEL;
        }
        private void push_convert(string str)
        {
            cvt_content.Append(str.ToUpper());
        }
        private int line = 1;
        private int line_at = 0;
        private int token_index = -1;
        private char cur_token;
        private int cvt_index = 0;
        private bool to_end = false;
        char get_src_token()
        {
            token_index++;
            line_at++;
      
                int read = src_stream.ReadByte ();// src_content[token_index];
               
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
        void error()
        {
            throw new Exception(string.Format("error at char \'{0}\'({1}), position {2}, line {3}", cur_token, cur_token, line_at, line));
        }
        private bool is_validate_token(char ch)
        {
            if (ch >= 'A' && ch <= 'Z')
                return true;
            if (ch >= 'a' && ch <= 'z')
                return true;
            if (ch >= '0' && ch <= '9')
                return true;
            if (ch == '_')
                return true;
            return false;
        }
        string get_word()
        {
            string rtn = "";
            int i = 0;
            while (cur_token != ' ' && cur_token != 0 && cur_token != '\n' && cur_token != '\r')
            {
                if (is_validate_token(cur_token) == false)
                {
                    if (cur_token == ':')
                        break;
                    error();
                }
                else
                {
                    rtn += cur_token;
                    i++;
                    if (i >= MAX_WORD_LEN)
                    {
                        error();
                        break;
                    }
                    get_src_token();
                }
            }
            return rtn;
            //printf("got word %s\n\n\n",word);
        }
        int get_number()
        {
            int rtn = 0;
            bool is_neg = false;
            if (cur_token == 'x')
            {
                get_src_token();
                while (true)
                {
                    if (cur_token >= '0' && cur_token <= '9')
                    {
                        rtn *= 16;
                        rtn += cur_token - '0';
                    }
                    else if (cur_token >= 'a' && cur_token <= 'f')
                    {
                        rtn *= 16;
                        rtn += cur_token - 'a' + 10;
                    }
                    else if (cur_token >= 'A' && cur_token <= 'F')
                    {
                        rtn *= 16;
                        rtn += cur_token - 'A' + 10;
                    }
                    else
                        break;
                    get_src_token();
                }
                if (rtn > 32767)
                    rtn -= 65536;
                return rtn;
            }
            if (cur_token == '-')
            {
                is_neg = true;
                get_src_token();
            }
            while (cur_token >= '0' && cur_token <= '9')
            {
                rtn *= 10;
                rtn += (cur_token - '0');
                get_src_token();
            }
            if (is_neg == true)
                rtn = -rtn;
            return rtn;
        }
        void skip_space()
        {
            while (cur_token == ' ' || cur_token == '\t')
                get_src_token();
        }

        void D()
        {//dlx programm
            while (cur_token != 0)
            {
                switch (cur_token)
                {
                    case '\'':
                        C();
                        continue;
                    case '/':
                        get_src_token();
                        if (cur_token != '/')
                            error();
                        else
                            C();
                        continue;
                    case ' ':
                    case '\t':
                        skip_space();
                        continue;
                    case '\n':
                    case '\r':
                        get_src_token();//ignore '\r', as each sentence end with "\n\r" in windows
                        break;
                    default:
                        if (is_validate_token(cur_token) == true)
                        {
                            I();
                            skip_space();
                            if (cur_token=='\r' || cur_token == '\n' || cur_token == '\'' || cur_token == '/')
                                break;
                            else
                                error();
                        }
                        else
                            error();
                        break;
                }


            }
        }
        /// <summary>
        ///instruction include comment
        /// </summary>
        void I()
        {
            string word = get_word();
            WORDTYPE w_type;

            //printf("got word %s\n\n",word);
            w_type = get_word_type(word);
            if (w_type == WORDTYPE.LABEL)
            {

                //do something...
                Debug("got label :"+word+'\n');
                push_convert(word);

                skip_space();

                if (cur_token != ':')
                    error();
                push_convert(":");

                get_src_token();
                skip_space();
                word = get_word();
                w_type = get_word_type(word);
                if (w_type == WORDTYPE.LABEL)
                    error();


            }

            push_convert(word);
            push_convert(" ");
            //dest_writer.WriteLine("got word" + word);
            switch (w_type)
            {
                case WORDTYPE.ALR:
                    I_ALR();
                    break;
                case WORDTYPE.ALI:
                    I_ALI();
                    break;
                case WORDTYPE.DM://数据传送

                    I_DM();
                    break;
                case WORDTYPE.CB:
                    I_CB();
                    break;
                case WORDTYPE.JMPI:
                    I_JMPI();
                    break;
                case WORDTYPE.TRAP:
                    I_TRAP();
                    break;
                case WORDTYPE.JMPR:
                    I_JMPR();
                    break;
                default:
                    error();
                    break;
            }
            push_convert("\n");

        }
        void I_ALR()
        {
           
            skip_space();
            R();
            skip_space();
            if (cur_token != ',')
                error();
            get_src_token();
            skip_space();
            R();
            skip_space();
            if (cur_token != ',')
                error();
            get_src_token();
            skip_space();
            R();
         
        }
        void I_ALI()
        {
            skip_space();
            R();
            skip_space();
            if (cur_token != ',')
                error();
            get_src_token();
            skip_space();
            R();
            skip_space();
            if (cur_token != ',')
                error();
            get_src_token();
            skip_space();
            int imm = get_number();
            Debug(String.Format("got {0}\n", imm));
        }
        void I_DM()
        {
            skip_space();
            R();
            skip_space();
            if (cur_token != ',')
                error();
            get_src_token();
            skip_space();
            R();
            skip_space();
            if (cur_token != ',')
                error();
            get_src_token();
            skip_space();
            int imm = get_number();
            Debug(String.Format("got offset {0}\n", imm));
        }
        void I_CB()
        {

            skip_space();
            R();
            skip_space();
            if (cur_token != ',')
                error();
            get_src_token();
            skip_space();
            string label = get_word();
            push_convert(label);
            Debug(String.Format("c b to {0}\n", label));
        }
        void I_JMPI()
        {
            skip_space();
            string label = get_word();
            push_convert(label);
        }
        void I_TRAP()
        {
            skip_space();
            int tindex = get_number();
            Debug(String.Format("trap {0}\n", tindex));
        }
        void I_JMPR()
        {
            skip_space();
            R();
        }
        void I_RFE()
        {

        }

        //registers
        void R()
        {
            int r_id;
            if (cur_token != 'r' && cur_token != 'R')
                error();
            get_src_token();
            r_id = get_number();
            if (r_id < 0 || r_id > 31)
                error();
            push_convert('R'+r_id.ToString()+' ');
            dest_writer.Write("get r" + r_id.ToString()+'\n');
            return;
        }
        void TI()
        {

        }
        private void C()
        {
            Debug("\nComment:");
            char ch = get_src_token();
            Debug(ch.ToString());
            while (ch != '\n' && ch != 0)
            {
                ch = get_src_token();
                Debug(ch.ToString());
            }
            Debug("\n");
            return;
        }

        public void Complie(Stream   source, TextWriter  destination)
        {
            if(source ==null || destination==null)
                throw new Exception ("the source stream or destination stream can not be null!");
            src_stream  = source;
            dest_writer = destination;
            cvt_content = new StringBuilder();
            get_src_token();
            D();
            Debug("Success!\n\n");
            Debug(cvt_content.ToString());
        }
    }
}
