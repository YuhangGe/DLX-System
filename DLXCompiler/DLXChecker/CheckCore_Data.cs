//#define debug


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Abraham
{
    /// <summary>
    /// 编译器核心的数据段汇编模块
    /// </summary>
    partial class DLXChecker
    {
        bool run_to_text = false;//当前编译是否到达text段
        bool run_to_data_content = false;//当前是否到达data 的数据段


        //临时储存.global和.extern声明的符号，用于查找符号是否在声明段
        //被声明为.global或.extern
        Dictionary<string, row_colum> global_symbol_tmp = new Dictionary<string, row_colum>();
        Dictionary<string, row_colum> extern_symbol_tmp = new Dictionary<string, row_colum>();


        //data分为全局声明段和数据段，前者包括.global和.extern
        //数据段
        private void Data()
        {

            run_to_text = false;
            run_to_data_content = false;
            try
            {
                DFirst();//首先保证第一行代码是.data
            }
            catch
            {
                jump_to_next_line();
            }
         
            skip_space();
            if (!(cur_token == '\r' || cur_token == '\n' || cur_token == '\'' || cur_token == '/' || cur_token == ';'))
                error(string.Format(".data伪指令结束后出现不支持的符号{0}", cur_token));

            while (cur_token != 0 && !run_to_text)
            {
                switch (cur_token)
                {
                    case ';':
                        C();
                        continue;
                    case ' ':
                    case '\t':
                        skip_space();
                        continue;
                    case '\n':
                        AppendContent();
                        get_src_token();
                        continue;
                    case '\r':
                        get_src_token();//ignore '\r', as each sentence end with "\n\r" in windows
                        continue;
                    default:
                        try
                        {
                            DP();//数据段伪指令
                            skip_space();
                            if (cur_token == '\r' || cur_token == '\n' ||  cur_token == ';')
                                break;
                            else
                                error(string.Format("当前伪指令结束后出现不支持的符号{0}", cur_token));
                            break;
                        }
                        catch
                        {
                            jump_to_next_line();
                            break;
                        }

                }
            }
            if (cur_token == 0)
                if (format_line.Length > 0 && src_line.Length > 0)
                    AppendContent(); 

            if (!run_to_text)
            {
                error("没有找到程序段!");
            }

            //检查是否有声明但没有定义的global标记
            if (global_symbol_tmp.Count > 0)
            {
                foreach (KeyValuePair<string, row_colum> kvp in global_symbol_tmp)
                {
                    try
                    {
                        error(string.Format("global声明的标记{0}没有定义", kvp.Key), kvp.Value.row, kvp.Value.colum);
                    }
                    catch (DLXException)
                    {
                    }
                }

            }

        }
        void DFirst()//data段第一行有效代码必须是.data
        {
            bool find_dot = false;
            //首先查找第一行有效代码，看是否是.data
            while (cur_token != 0 && !find_dot)
            {
                switch (cur_token)
                {
                    case ';':
                        C();
                        continue;
                    case ' ':
                    case '\t':
                        skip_space();
                        continue;
                    case '\n':
                        AppendContent();
                        get_src_token();
                        continue;
                    case '\r':
                        get_src_token();//ignore '\r', as each sentence end with "\n\r" in windows
                        continue;
                    default:
                        if (cur_token != '.')
                        {
                            error("第一行有效代码必须是.data!");                            
                        }
                        else
                        {
                            get_src_token();
                            find_dot = true;
                        }
                        break;

                }
            }
            if (cur_token != 0)
            {
                string word = get_word();
                if (word.ToUpper() != "DATA")
                {
                    error("第一行有效代码必须是.data!");
                }
                skip_space();
                format_line.Append(AppendSpace()).Append(".DATA");
                bool user_address = true;
                int d_base = -1;
                try
                {
                    string num_word = get_number_word();
                    d_base = parse_number_32(num_word);
                    format_line.AppendFormat("\t{0}", num_word);
                }
                catch (DLXException)
                {
                    //用户没有自定义初始地址，删除上面代码抛出的异常
                    delete_last_error();
                    user_address = false;
                }
                if (user_address && d_base % 4 != 0)
                    error("data段初始地址必须为4的倍数");
            }
            else//文件尾部检查
            {
                if(format_line.Length >0 && src_line.Length >0)
                    AppendContent();                   
            }
           
               

        }
        private void AppendContent()
        {
            if (!line_error)
                result.Content.AppendLine(format_line.ToString());
            else
                result.Content.AppendLine(src_line.ToString());
            format_line.Length=0;
            src_line.Length =0;
            line_error = false;
        }
        void DP()//Pseudo,数据段数据区
        {
            string word = null;

            if (cur_token != '.')
            {
                run_to_data_content = true;//如果是标签，表明到达了数据区
                word = get_word();

                if (char.IsDigit(word[0]))
                {
                    error("标签只能以$或_或字母开头");
                }
#if debug
                Debug("got label :" + word);
#endif
                skip_space();
                if (cur_token != ':')
                {
                    error("语法错误，期望分号(:)");
                }

                string upper_word = word.ToUpper();
                if (extern_symbol_tmp.ContainsKey(upper_word))
                    error(string.Format("符号{0}已被声明为Extern，不能被定义", word));

                if (dataTable.Contains(upper_word))
                    error(string.Format("符号{0}出现重定义", word));
                dataTable.Add(upper_word);

                format_line.Append(AppendSpace(upper_word + " :"));

                if (global_symbol_tmp.ContainsKey(upper_word))
                {
                    global_symbol_tmp.Remove(upper_word);
                }


                get_src_token();
                skip_space();
                if (cur_token != '.')
                    error("语法错误，期望点号(.)");
            }
            else
            {
                format_line.Append(AppendSpace());
            }
            get_src_token();
            word = get_word();
            format_line.AppendFormat(".{0}\t", word.ToUpper());
            skip_space();
            switch (word.ToLower())
            {
                case "global":
                    PGlobal();
                    break;
                case "extern":
                    PExtern();
                    break;
                case "word":
                    PWord();
                    break;
                case "byte":
                    PByte();
                    break;
                case "space":
                    PSpace();
                    break;
                case "align":
                    PAlign();
                    break;
                case "ascii":
                    PAscii();
                    break;
                case "asciiz":
                    PAsciiz();
                    break;
                case "text":
                    PText();
                    break;
                default:
                    error("不支持的伪指令：" + word);
                    break;
            }
        }
        void PText()
        {
            run_to_text = true;//设置run_to_text为真，跳到汇编text段函数Text()
            skip_space();
            bool user_address = true;
             int t_base=-1;
            try
            {
                string num_word = get_number_word();
                t_base = parse_number_32(num_word);
                format_line.Append(num_word);
            }
            catch (DLXException)
            {
                user_address = false;
                delete_last_error();
            }
            if(user_address && t_base % 4 != 0)
                    error("text段初始地址必须为4的倍数");
        }
        void PByte()
        {
            while (true)
            {
                string n_word = get_number_word();
                byte b = parse_number_byte (n_word);
                format_line.Append(n_word);   
#if debug
                Debug("got byte: " + b.ToString());
#endif


                skip_space();
                if (cur_token != ',')
                    break;
                result.Content.Append(", ");
                get_src_token();
                skip_space();
            }
        }
        void PAlign()
        {
            string num_word = get_number_word();
           
            int num = parse_number_16(num_word);
            if (num <= 0)
            {
                error(".align伪指令后只能跟非负数");
            }
         format_line.Append(num_word);

        }
        void PGlobal()
        {
            if (run_to_data_content)
                error(".global伪指令必须出现在数据段头");
            string word = get_word();

            if (char.IsDigit(word[0]))
                error("标示不能以字母开头");
            if (string.IsNullOrEmpty(word) || Regex.IsMatch(word, @"^\s*$"))
                error("指令不完整，.global指令后需要标签名");
#if debug
            Debug("global: " + word);
#endif

            format_line.Append(word);


            //添加word至.global声明过的符号列表中
            if (global_symbol_tmp.ContainsKey(word))
                error(string.Format("符号{0}已经被声明过Global,不能重复声明"));
            global_symbol_tmp.Add(word, new row_colum(line, line_at));

        }
        void PExtern()
        {
            if (run_to_data_content)
                error(".extern伪指令必须出现在数据段头");
            string word = get_word();
            if (char.IsDigit(word[0]))
                error("标示不能以字母开头");
            if (string.IsNullOrEmpty(word) || Regex.IsMatch(word, @"^\s*$"))
                error("指令不完整，.extern指令后需要标签名");
#if debug
            Debug("extern: " + word);
#endif
            format_line.Append(word.ToUpper());
            //添加word至.extern声明过的符号列表中
            if (extern_symbol_tmp.ContainsKey(word))
                error(string.Format("符号{0}已经被声明过Extern,不能重复声明"));
            extern_symbol_tmp.Add(word, new row_colum(line, line_at));
         

        }
        void PWord()
        {
            run_to_data_content = true;
         
            int a;
            while (true)
            {
                string n_word = get_number_word();
                a = parse_number_32(n_word);
#if debug
                Debug("got word: " + a.ToString());
#endif
              format_line.Append(n_word);
                skip_space();
                if (cur_token != ',')
                    break;
                format_line.Append(',');
                get_src_token();
                skip_space();
            }
        }
        void PSpace()
        {
            run_to_data_content = true;
            string num_word = get_number_word();
            int num = parse_number_32(num_word);
            if (num > MAX_SPACE_NUM)
                error(".space 要求空间太大了！");
#if debug
            Debug("got space: " + num.ToString());
#endif
            format_line.Append(num_word);
        }
        void PAscii( bool add_zero = false)//.ascii
        {
            run_to_data_content = true;
            while (true)
            {
                skip_space();
                string str = PAsciiEach();
                format_line.AppendFormat("\"{0}\"",str);
                skip_space();
                if (cur_token != ',')
                    break;
                else
                {
                    format_line.Append(", ");
                    get_src_token();
                }
            }


        }
        string PAsciiEach()//.ascii的每一个
        {
            //List<byte> l_b = new List<byte>();
            if (cur_token != '\"')
                error("字符串必需用双引号(\")引入");
            string rtn = "";
            get_src_token();
            bool find_back_quote = false;//是否找到后引号
            while (!find_back_quote)
            {
                if (cur_token == '\r' || cur_token == '\n' || cur_token==0)
                    break;
                if (cur_token != '\"')
                    rtn += cur_token;
                else
                    find_back_quote = true;
                get_src_token();
            }
            if (!find_back_quote)
                error("字符串必需用双引号(\")引入");
            return rtn;// System.Text.Encoding.GetEncoding("gbk").GetString(l_b.ToArray());
        }
        void PAsciiz()//.asciiz
        {
            run_to_data_content = true;
            PAscii( true);
        }

    }
}
