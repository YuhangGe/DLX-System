//#define debug
#define meaning

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DLXAssembler
{
    /// <summary>
    /// 编译器核心的数据段汇编模块
    /// </summary>
    partial class CompilerCore
    {
        bool run_to_text = false;//当前编译是否到达text段
        bool run_to_data_content = false;//当前是否到达data 的数据段

#if meaning
        //临时储存.global和.extern声明的符号，用于查找符号是否在声明段
        //被声明为.global或.extern
        Dictionary<string, row_colum> global_symbol_tmp = new Dictionary<string, row_colum>();
        Dictionary<string, row_colum> extern_symbol_tmp = new Dictionary<string, row_colum>();
#endif

        //data分为全局声明段和数据段，前者包括.global和.extern
        //数据段
        private void Data()
        {
            location_counter = 0;
            run_to_text = false;
            run_to_data_content = false;
            DFirst();//首先保证第一行代码是.data
            skip_space();
            if (!(cur_token == '\r' || cur_token == '\n' || cur_token == '\'' || cur_token == '/' || cur_token == ';'))
                error(string.Format(".data伪指令结束后出现不支持的符号{0}", cur_token));

            while (cur_token != 0 && !run_to_text)
            {
                switch (cur_token)
                {
                    case '\'':
                    case ';':
                        C();
                        continue;
                    case '/':
                        get_src_token();
                        if (cur_token != '/')
                            error(string.Format("不支持的符号{0},必须用一对/(//)表示注释", cur_token));
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
                        continue;
                    default:
                        try
                        {
                            DP();//数据段伪指令
                            skip_space();
                            if (cur_token == '\r' || cur_token == '\n' || cur_token == '\'' || cur_token == '/' || cur_token == ';')
                                break;
                            else
                                error(string.Format("当前伪指令结束后出现不支持的符号{0}", cur_token));
                            break;
                        }
                        catch
                        {
                            if (tailer_error.IsDead == true || error_num >= MAX_ERROR_ALLOW)
                                throw dlx_errors;
                            else
                            {
                                jump_to_next_line();
                            }
                            break;
                        }

                }
            }


            if (!run_to_text)
            {
                error("没有找到程序段!", true);
            }

            //检查是否有声明但没有定义的global标记
            if (global_symbol_tmp.Count > 0)
            {
                foreach (KeyValuePair<string, row_colum> kvp in global_symbol_tmp)
                {
                    try
                    {
                        error(string.Format("global声明的标记{0}没有定义", kvp.Key), kvp.Value.row, kvp.Value.colum, false);
                    }
                    catch
                    {
                        if (error_num >= MAX_ERROR_ALLOW)
                            throw dlx_errors;
                    }
                }

            }
         
            //如果用户没有自定义data段地址，那么填充data段长度为4的倍数
            //，因为多文件没有自定义地址链接时链接器会把data段直接连接起来，
            //如果不填充为4的倍数，连接起来后各个data段的初始地址不为4的倍数会出错。
            if (result.dataTable.TableBase == uint.MaxValue)
            {
                uint tmp = ~(0xffffffff << 2);
                while ((location_counter & tmp) != 0)
                {
                    result.dataContent.Append((char)0);
                    location_counter++;
                }
            }
            result.dataTable.Length = location_counter;
        }
        void DFirst()//data段第一行有效代码必须是.data
        {
            bool find_dot = false;
            //首先查找第一行有效代码，看是否是.data
            while (cur_token != 0 && !find_dot)
            {
                switch (cur_token)
                {
                    case '\'':
                    case ';':
                        C();
                        continue;
                    case '/':
                        get_src_token();
                        if (cur_token != '/')
                            error(string.Format("不支持的符号{0},必须用一对/(//)表示注释", cur_token));
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
                        continue;
                    default:
                        if (cur_token != '.')
                        {
                            error("第一行有效代码必须是.data!", true);
                        }
                        else
                        {
                            get_src_token();
                            find_dot = true;
                        }
                        break;

                }
            }
            string word = get_word();
            if (word.ToUpper() != "DATA")
            {
                error("第一行有效代码必须是.data!", true);
            }
            skip_space();
            try
            {
                //如果用户没有自定义初始地址，get_number_word会抛出异常，因为没有number可以get
                //这个异常会被捕获，从而知道用户是否自定义了初始地址。
                string num_word = get_number_word();
              
                int d_base = parse_number_32(num_word);
                if (d_base % 4 != 0)
                    error("data段初始地址必须为4的倍数");
                //用户自定义了初始地址
                result.dataTable.TableBase = (uint)d_base;
            }
            catch (DLXException)
            {
                //用户没有自定义初始地址，删除上面get_number_word抛出的异常
                delete_last_error();
                result.dataTable.TableBase = uint.MaxValue ;
            }
        }

        void DP()//Pseudo,数据段数据区
        {
            string word = null;
            Symbol symbol = null;
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

				string upper_word=this.ignore_case ?word.ToUpper():word;
                if (extern_symbol_tmp.ContainsKey(upper_word))
                    error(string.Format("符号{0}已被声明为Extern，不能被定义", word));
                symbol = result.dataTable.insertSymbol(upper_word);
                if (symbol == null)
                    error(string.Format("符号{0}出现重定义", upper_word));
                if (global_symbol_tmp.ContainsKey(upper_word))
                {
                    symbol.IsGlobal = true;
                    global_symbol_tmp.Remove(upper_word);
                }


                get_src_token();
                skip_space();
                if (cur_token != '.')
                    error("语法错误，期望点号(.)");
            }
            get_src_token();
            word = get_word();
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
                    PWord(symbol);
                    break;
                case "byte":
                    PByte(symbol);
                    break;
                case "space":
                    PSpace(symbol);
                    break;
                case "align":
                    PAlign();
                    break;
                case "ascii":
                    PAscii(symbol);
                    break;
                case "asciiz":
                    PAsciiz(symbol);
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
            try
            {
                string num_word = get_number_word();
                int t_base = parse_number_32(num_word);
                if (t_base % 4 != 0)
                    error("text段初始地址必须为4的倍数");
                //用户自定义了程序段初始地址
                result.textTable.TableBase = (uint)t_base;
            }
            catch (DLXException)
            {
                delete_last_error();
                result.textTable.TableBase = uint.MaxValue ;
            }
        }
        void PByte(Symbol symbol)
        {
            while (true)
            {
                string n_word = get_number_word();
                byte b = parse_number_byte (n_word);
                
#if debug
                Debug("got byte: " + b.ToString());
#endif
#if meaning
              
                result.dataContent.Append((char)b);

                if (symbol != null)
                    symbol.Value.Add(location_counter);
                location_counter ++;
#endif
                skip_space();
                if (cur_token != ',')
                    break;
                get_src_token();
                skip_space();
            }
        }
        void PAlign()
        {
            string num_word = get_number_word();
            int num = parse_number_16(num_word);
            if (num < 0)
            {
                error(".align伪指令后不能跟负数");
            }
            uint  tmp = ~(0xffffffff << num);
            while ((location_counter & tmp) != 0)
            {
                result.dataContent.Append((char)0);
                location_counter++;
            }
        }
        void PGlobal()
        {
            if (run_to_data_content)
                error(".global伪指令必须出现在数据段头");
            string word =get_word();
            word = this.ignore_case?word.ToUpper():word;
            if (char.IsDigit(word[0]))
                error("标示不能以字母开头");
            if (string.IsNullOrEmpty(word) || Regex.IsMatch(word, @"^\s*$"))
                error("指令不完整，.global指令后需要标签名");
#if debug
            Debug("global: " + word);
#endif



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
            word = this.ignore_case ? word.ToUpper() : word;
            if (char.IsDigit(word[0]))
                error("标示不能以字母开头");
            if (string.IsNullOrEmpty(word) || Regex.IsMatch(word, @"^\s*$"))
                error("指令不完整，.extern指令后需要标签名");
#if debug
            Debug("extern: " + word);
#endif
            //添加word至.extern声明过的符号列表中
            if (extern_symbol_tmp.ContainsKey(word))
                error(string.Format("符号{0}已经被声明过Extern,不能重复声明"));
            extern_symbol_tmp.Add(word, new row_colum(line, line_at));
            result.dataTable.insertSymbol(word, false, true);

        }
        void PWord(Symbol symbol)
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
#if meaning
                byte[] b = int_to_bytes(a);
                for (int i = 0; i < 4;i++ )
                {
                    result.dataContent.Append((char)b[i]);
                }
                if (symbol != null)
                    symbol.Value.Add(location_counter);
                location_counter += 4;
#endif
                skip_space();
                if (cur_token != ',')
                    break;
                get_src_token();
                skip_space();
            }
        }
        void PSpace(Symbol symbol)
        {
            run_to_data_content = true;
            string num_word = get_number_word();
            int num = parse_number_32(num_word);
            if (num > MAX_SPACE_NUM)
                error(".space 要求空间太大了！");
#if debug
            Debug("got space: " + num.ToString());
#endif
#if meaning

            for (int i = 0; i < num; i++)
            {
                result.dataContent.Append((char)0);
                if (symbol != null)
                    symbol.Value.Add(location_counter);
                location_counter ++;
            }

#endif
        }
        void PAscii(Symbol symbol, bool add_zero = false)//.ascii
        {
            run_to_data_content = true;
            while (true)
            {
                skip_space();
                string str = PAsciiEach(symbol);
#if debug
                if (add_zero)
                    Debug("got asciiz :" + str);
                else
                    Debug("got ascii :" + str);
#endif
#if meaning
                result.dataContent.Append(str);
                int len = str.Length;
                if (add_zero)
                {
                    result.dataContent.Append((char)0);
                    len++;
                }
                if (symbol != null)
                    symbol.Value.Add(location_counter);
                location_counter += (uint)len;
#endif
                skip_space();
                if (cur_token != ',')
                    break;
                else
                    get_src_token();
            }


        }
        string PAsciiEach(Symbol symbol)//.ascii的每一个
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
        void PAsciiz(Symbol symbol)//.asciiz
        {
            run_to_data_content = true;
            PAscii(symbol, true);
        }

    }
}
