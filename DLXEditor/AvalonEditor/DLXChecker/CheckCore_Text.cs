//#define debug

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abraham
{
    /// <summary>
    /// 编译器核心的程序段汇编模块
    /// </summary>
    public partial class DLXChecker
    {
        //等待检查是否有效的标记
        //当汇编识别到一个标记被使用，会查找标记是否已经声明，如果没有，
        //则加入wati_to_check,当以后识别到一个新声明的标记时，查找wait_to_check,
        //找到就删除。汇编结束看wait_to_check是否为空，不为空则报错.
        private List<string> wait_to_check = new List<string>();
       
        private Dictionary<string, List<row_colum>> wait_list = new Dictionary<string, List<row_colum>>();
        private void add_to_wait(string word, int row, int colum)
        {
            if (!wait_to_check.Contains(word))
                wait_to_check.Add(word);
            if (!wait_list.ContainsKey(word))
            {
                wait_list.Add(word, new List<row_colum>());
            }
            List<row_colum> l = wait_list[word];
            l.Add(new row_colum(row, colum));
        }
        private void remove_from_wait(string word)
        {
            if (!wait_to_check.Contains(word))
                return;
            wait_to_check.Remove(word);
            wait_list.Remove(word);
        }
        //程序段
        private void Text()
        {
            global_symbol_tmp.Clear();
            extern_symbol_tmp.Clear();

            TPseudo();//Text段的伪指令区
            TContent();//Text段的指令区

            //检查是否有没有声明但使用了的标记
            if (wait_to_check.Count > 0)
            {
                foreach (string word in wait_to_check)
                {
                    List<row_colum> lr = wait_list[word];
                    foreach (row_colum rc in lr)
                    {
                        try
                        {
                            error("未声明的标记"+word , rc.row, rc.colum);
                        }
                        catch
                        {
                           
                        }
                    }
                }
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
                    catch
                    {
                        
                    }
                }

            }
           
        }
        private void TPseudo()//程序段的伪指令区，即位于程序段顶端的.global和.extern
        {
            bool run_to_content = false;//是否汇编到程序段指令区
            while (cur_token != 0 && !run_to_content)
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
                        continue ;
                    default:
                        if (cur_token != '.')
                        {
                            run_to_content = true;
                            break;
                        }
                        else
                        {
                            try
                            {
                                get_src_token();
                                string word = get_word();
                                format_line.AppendFormat("{0}.{1}  ",AppendSpace(),word.ToUpper());
                                switch (word.ToLower())
                                {
                                    case "global":
                                        TGlobal();
                                        break;
                                    case "extern":
                                        TExtern();
                                        break;
                                    default:
                                        error("不能识别的伪指令" + word);
                                        break;
                                }
                                skip_space();
                                if (cur_token == '\r' || cur_token == '\n' || cur_token == ';')
                                    break;
                                else
                                    error(string.Format("当前伪指令结束后出现不支持的符号{0}", cur_token));
                            }
                            catch
                            {
                                jump_to_next_line();
                            }

                        }
                        break;
                }


            }
            if (cur_token == 0)//文件尾部检查
                if (format_line.Length > 0 && src_line.Length > 0)
                    AppendContent(); 
        }
        private void TGlobal()
        {
            skip_space();
            string word = get_word();

#if debug
            Debug("got tglobal: " + word);
#endif


            //添加word至.global声明过的符号列表中
            if (dataTable.Contains(word.ToUpper() ) )
                error(string.Format("符号{0}已经在data段声明过，不能重复！", word));
            if (global_symbol_tmp.ContainsKey(word.ToUpper()))
                error(string.Format("符号{0}已经被声明过Global,不能重复声明"));
            global_symbol_tmp.Add(word.ToUpper(),new row_colum(line,line_at ));
            format_line.Append(word.ToUpper());
        }
        private void TExtern()
        {
            skip_space();
            string word = get_word();

#if debug
            Debug("got textern: " + word);
#endif

            //添加word至.extern声明过的符号列表中
            string upper_word = word.ToUpper();
            if (dataTable.Contains(upper_word) == true)
                error(string.Format("符号{0}已经在data段声明过，不能重复！", word));
            if (extern_symbol_tmp.ContainsKey(upper_word))
                error(string.Format("符号{0}已经被声明过Extern,不能重复声明"));
            extern_symbol_tmp.Add(upper_word, new row_colum(line, line_at));
            textTable.Add(upper_word);
            format_line.Append(upper_word);
            //result.textTable.insertstring(word, false, true);

        }

        //程序段的实际指令区
        private void TContent()
        {
            while (cur_token != 0)
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
                        break;
                    default:
                        try
                        {
                            if (is_validate_token(cur_token) == true)
                            {
                                I();
                                skip_space();
                                if (cur_token == '\r' || cur_token == '\n' || cur_token == ';' || cur_token==0)
                                    break;
                                else
                                    error(string.Format("当前指令结束后出现不支持的符号{0}", cur_token));
                            }
                            else if (cur_token == '.')
                                error("伪指令只能出现在程序段的开头");
                            else
                                error(string.Format("不支持的符号{0}", cur_token));
                        }
                        catch
#if debug 
                            (Exception e)
#endif
                        {
#if debug
                            System.Diagnostics.Debug.Print(e.StackTrace.ToString());
#endif
                            jump_to_next_line();
                        }

                        break;
                }


            }
            if (cur_token == 0)//文件尾部检查
            {
                if (format_line.Length > 0 && src_line.Length > 0)
                    AppendContent(); 
            }
        }
        /// <summary>
        /// instruction include comment
        /// </summary>
        private void I()
        {
            string word = get_word();
            WORDTYPE w_type;

            //printf("got word %s\n\n",word);
            w_type = get_word_type(word);
            if (w_type == WORDTYPE.LABEL)
            {

                //do something...
#if debug
                Debug("got label :" + word + '\n');
#endif
                //push_convert(word);

                skip_space();

                if (cur_token != ':')
                    error("指令不完整，期望分号':'");
                // push_convert(":");
                string upper_word = word.ToUpper();
                if (extern_symbol_tmp.ContainsKey(upper_word))
                    error(string.Format("标记{0}已被声明为Extern，不能被定义", word));
                if (dataTable.Contains(upper_word) == true)
                    error(string.Format("符号{0}已经在data段声明过，不能重复！", word));
                if (textTable.Contains(upper_word))
                    error(string.Format("标记{0}出现重定义", word));
                else
                    textTable.Add(upper_word);
                if (global_symbol_tmp.ContainsKey(upper_word))
                {
                    global_symbol_tmp.Remove(upper_word);
                }

                //把标记从等待确认标记是否存在的列表出去除
                remove_from_wait(upper_word);

                format_line.Append(AppendSpace(upper_word + " :"));


                get_src_token();
                skip_space();
                word = get_word();
                w_type = get_word_type(word);
                if (w_type == WORDTYPE.LABEL)
                    error("不能识别的指令操作符\'" + word + '\'');


            }
            else
            {
                format_line.Append(AppendSpace());
            }

            //得到操作码在表KEYWORDS中的具体索引，
            //该索引会记录到DLXObject中，linker阶段通过索引
            //进行表驱动
            format_line.Append(AppendSpace(word.ToUpper(), SPACENUM2));
            switch (w_type)
            {
                case WORDTYPE.ALR://I-类型算术/逻辑运算
                    I_ALR(word);
                    break;
                case WORDTYPE.ALI://R-类型算术/逻辑运算
                    I_ALI(word);
                    break;
                case WORDTYPE.LHI://LHI指令
                    I_LHI(word);
                    break;
                case WORDTYPE.LOAD ://数据传送加载指令
                    I_LOAD(word);
                    break;
                case WORDTYPE.STORE ://数据传送储存指令
                    I_STORE(word);
                    break;
                case WORDTYPE.CB://条件分支
                    I_CB(word);
                    break;
                case WORDTYPE.JMPI://使用立即数的跳，包括J和JAL
                    I_JMPI(word);
                    break;
                case WORDTYPE.TRAP://TRAP指令
                    I_TRAP(word);
                    break;
                case WORDTYPE.JMPR://使用寄存器的跳，包括JR和JALR
                    I_JMPR(word);
                    break;
                case WORDTYPE.RFE:
                    I_RFE(word);
                    break;
                case WORDTYPE.MOVE:
                    I_MOVE(word);
                    break;
                case WORDTYPE.RET://从跳转（函数）返回 ，等价于JR R31
                    I_RET(word);
                    break;
                default:
                    error("不能识别的指令码" + word);
                    break;
            }
            //push_convert("\n");

        }
        private void I_MOVE(string  cmd)
        {
            skip_space();
            int gpr = R();
            skip_space();
            if (cur_token != ',')
                error("指令不完整，期望逗号(,)");
            get_src_token();
            skip_space();
            string spr_name = "";
            while (cur_token != ' ' && cur_token != '\t' && cur_token != 0 && cur_token != '\n' && cur_token != '\r')
            {
                spr_name += char.ToUpper(cur_token);
                get_src_token();
            }
            if (!special_register.ContainsKey(spr_name))
            {
                error("错误的特殊寄存器代号,请查阅手册.");
            }
            format_line.AppendFormat("R{0}, {1} ", gpr, spr_name);
          
        }
        /// <summary>
        /// 从跳转（函数）返回 ，等价于JR R31
        /// </summary>
        /// <param name="cmd_index"></param>
        private void I_RET(string cmd)
        {
           // format_line.Append("RET");
         }
        /// <summary>
        /// R-类型算术/逻辑运算，格式为Add r1,r2,r3
        /// </summary>
        private void I_ALR(string cmd)
        {

            skip_space();
            int r1 = R();
            skip_space();
            if (cur_token != ',')
                error("指令不完整，期望逗号(,)");
            get_src_token();
            skip_space();
            int r2 = R();
            skip_space();
            if (cur_token != ',')
                error("指令不完整，期望逗号(,)");
            get_src_token();
            skip_space();
            int r3 = R();
           format_line.AppendFormat ("R{0}, R{1}, R{2}", r1, r2, r3);


        }
        /// <summary>
        /// I-类型算术/逻辑运算，格式为Addi r1,r2,#100或addi r1,r2,lable_1或addi r1,r2,str[2]
        /// </summary>
        private void I_ALI(string cmd)
        {
            skip_space();
            int dest_r = R();
            skip_space();
            if (cur_token != ',')
                error("指令不完整，期望逗号(,)");
            get_src_token();
            skip_space();
            int src_r = R();
            skip_space();
            if (cur_token != ',')
                error("指令不完整，期望逗号(,)");
            get_src_token();
            skip_space();
            string word;
            //首先假设指令第三个参数是立即数，如果尝试读取立即数失败，
            //再尝试读取标记
            try
            {
                word = get_number_word();//读取数字
                int imm = parse_number_16(word);
#if debug
                Debug(String.Format("got imm num {0}", imm));
#endif

                format_line.AppendFormat("R{0}, R{1}, {2}", dest_r, src_r, word);

            }
            catch (DLXException)
            {
                delete_last_error();//删除上面try中产生的错误
                word = get_word();//读取标记
#if debug
                Debug("got imm label:" + word);
#endif
                format_line.AppendFormat("R{0}, R{1}, {2}", dest_r, src_r, word.ToUpper ());
                int index = 0;

                skip_space();
                if (cur_token == '[')
                {
                    get_src_token();
                    skip_space();
                    string n_w = get_number_word();
                    index = parse_number_16(n_w);
#if debug
                    Debug("got array num: " + index.ToString());
#endif
                    skip_space();
                    if (cur_token != ']')
                        error("错误的语法，期望']'");
                    get_src_token();
                    format_line.AppendFormat("[{0}]", n_w);
                }



                if (check_symbol(word.ToUpper(), index) == false)
                    add_to_wait(word.ToUpper(), line, line_at);

            }
            catch
#if debug
                (Exception e)
#endif
            {
#if debug
                Debug(e.StackTrace.ToString());
#endif
            }

        }
        /// <summary>
        /// LHI指令,格式为lhi r0,#100或lhi r0, label_1 或lhi r1,str[#2]
        /// </summary>
        private void I_LHI(string cmd)
        {
            skip_space();
            int dest_r = R();
            skip_space();
            if (cur_token != ',')
                error("指令不完整，期望逗号(,)");
            get_src_token();
            skip_space();
            string word = null;
            //首先假设指令第三个参数是立即数，如果尝试读取立即数失败，
            //再尝试读取标记
            try
            {
                word = get_number_word();
                int imm = parse_number_16(word);
#if debug
                Debug("got lhi imm:" + imm.ToString());
#endif
                format_line.AppendFormat("R{0}, {1}", dest_r, word);
            }
            catch
            {
                //删除上面代码抛出的异常
                delete_last_error();

                word = get_word().ToUpper();
#if debug
                Debug("got lhi imm label:" + word);
#endif
                format_line.AppendFormat("R{0}, {1}", dest_r, word);
                skip_space();
                int index = 0;
                if (cur_token == '[')
                {
                    get_src_token();
                    skip_space();
                    string n_w = get_number_word();
                    index = parse_number_16(n_w);
#if debug
                    Debug("got array num: " + index.ToString());
#endif
                    skip_space();
                    if (cur_token != ']')
                        error("错误的语法，期望']'");
                    get_src_token();
                    format_line.AppendFormat("[{0}]", n_w);
                }
                if (check_symbol(word, index) == false)
                    add_to_wait(word, line, line_at);

            }
        }
        /// <summary>
        /// 数据传送加载指令，包括lb和  lw ，格式为lw r1, #100(r2)或lw r1,num(r2)或lw r3,num[ #2](r0)
        /// </summary>
        private void I_LOAD(string cmd)
        {
            skip_space();
            int dest_r = R();
            skip_space();
            if (cur_token != ',')
                error("指令不完整，期望逗号(,)");
            get_src_token();
            skip_space();

            string word = null;
           // int mode = 0;//0表示使用立即数，1表示使用标记
            int imm = 0;
            int index = 0;
            //首先假设指令第三个参数是立即数，如果尝试读取立即数失败，
            //再尝试读取标记
            try
            {
                //   mode = 0;
                word = get_number_word();
                imm = parse_number_16(word);
#if debug
                Debug("got dw imm:" + imm.ToString());
#endif
                format_line.AppendFormat("R{0}, {1}", dest_r, word);
            }
            catch (DLXException)
            {
                // mode = 1;
                //删除上面的错误
                delete_last_error();
                word = get_word().ToUpper();

                format_line.AppendFormat("R{0}, {1}", dest_r, word);
                skip_space();
                if (cur_token == '[')
                {
                    get_src_token();
                    skip_space();
                    string n_w = get_number_word();
                    index = parse_number_16(n_w);
                    skip_space();
                    if (cur_token != ']')
                        error("错误的语法，期望']'");
                    get_src_token();
                    format_line.AppendFormat("[{0}]", n_w);
                }
                if (check_symbol(word, index) == false)
                    add_to_wait(word, line, line_at);

#if debug
                Debug(string.Format("got dw imm label: {0} with offset {1}", word, index));
#endif

            }
            catch
#if debug
                (Exception e)
#endif
            {
#if debug
                Debug(e.StackTrace.ToString());
#endif
             }

            skip_space();
            if (cur_token != '(')
                error("指令语法错误，期望前括号'('");
            get_src_token(); skip_space();
            int src_r = R();
            skip_space();
            if (cur_token != ')')
                error("指令语法错误，期望后括号')'");
            format_line.AppendFormat("(R{0})", src_r);
            get_src_token();

        }
        /// <summary>
        /// 数据传送储存指令，包括sb,  sw ，格式为sw  #100(r2),r1或sw num(r2),r1或sw num[ #2](r0),r3
        /// </summary>
        private void I_STORE(string cmd)
        {
            skip_space();

            string word = null;
            //int mode = 0;
            int index = 0;
            int imm = 0;
            //首先假设指令第一个参数是立即数，如果尝试读取立即数失败，
            //再尝试读取标记
            try
            {
              //  mode = 0;
                word = get_number_word();
                imm = parse_number_16(word);
#if debug
                Debug("got dw imm:" + imm.ToString());
#endif
                format_line.AppendFormat("{0}", word);
            }
            catch(DLXException )
            {
                //mode = 1;
                //删除上面的错误
                delete_last_error();
                word = get_word().ToUpper();
                format_line.AppendFormat("{0}", word);
                skip_space();
                if (cur_token == '[')
                {
                    get_src_token();
                    skip_space();
                    string n_w = get_number_word();
                    index = parse_number_16(n_w);
                    skip_space();
                    if (cur_token != ']')
                        error("错误的语法，期望']'");
                    get_src_token();
                    format_line.AppendFormat("[{0}]", n_w);
                }
                if (check_symbol(word, index) == false)
                    add_to_wait(word, line, line_at);

#if debug
                Debug(string.Format("got sw imm label: {0} with offset {1}", word, index));
#endif
            }

            skip_space();
            if (cur_token != '(')
                error("指令语法错误，期望前括号'('");
            get_src_token(); skip_space();
            int src_r = R();
            skip_space();
            if (cur_token != ')')
                error("指令语法错误，期望后括号')'");
            get_src_token();
            skip_space();
            if (cur_token != ',')
                error("指令不完整，期望逗号(,)");
            get_src_token();
            skip_space();
            int dest_r = R();

            format_line.AppendFormat("(R{0}), R{1}",src_r, dest_r );


        }
        /// <summary>
        /// 条件分支指令，包括BEQZ和BNEZ，格式为beqz r2,label_1
        /// </summary>
        void I_CB(string cmd)
        {

            skip_space();
            int src_r = R();
            skip_space();
            if (cur_token != ',')
                error("指令不完整，期望逗号(,)");
            get_src_token();
            skip_space();
            string label = get_word().ToUpper();

#if debug
            Debug(String.Format("condition branch to  {0}", label));
#endif

            if (check_text_symbol(label) == false)
                add_to_wait(label, line, line_at);
            format_line.AppendFormat("R{0}, {1}", src_r, label);

        }
        /// <summary>
        /// 使用立即数的跳，包括J和JAL
        /// </summary>
        /// <param name="cmd_index"></param>
        private void I_JMPI(string cmd)
        {
            skip_space();
            string label = get_word().ToUpper();

            if (check_text_symbol(label) == false)
                add_to_wait(label, line, line_at);
            format_line.Append(label);

        }
        /// <summary>
        /// TRAP指令
        /// </summary>
        /// <param name="cmd_index"></param>
        private void I_TRAP(string cmd)
        {
            skip_space();
            int tindex = -1;
            try
            {
                string n_w = get_number_word();
                tindex = parse_number_16(n_w);
                format_line.Append(n_w);
            }
            catch
            {
                delete_last_error();
                string word = get_word();
                if (trap_table.ContainsKey(word.ToUpper()))
                {
                    format_line.Append(word.ToUpper());
                }
                else
                    error("不存在的向量符号" + word);
            }
        }
        /// <summary>
        /// 使用寄存器的跳，包括JR和JALR
        /// </summary>
        /// <param name="cmd_index"></param>
        private void I_JMPR(string cmd)
        {
            skip_space();
            int src_r = R();
            format_line.AppendFormat("R{0}", src_r);
        }
        /// <summary>
        /// 从异常返回
        /// </summary>
        /// <param name="cmd_index"></param>
        void I_RFE(string cmd)
        {
           //
        }

        //registers
        private int R()
        {
            int r_id;
            if (cur_token != 'r' && cur_token != 'R')
                error("错误的指令，寄存器标记必须以'R'开始，后跟0-31的数字");
            get_src_token();
            string n_w = get_number_word();
            r_id = parse_number_16(n_w);
            if (r_id < 0 || r_id > 31)
                error("寄存器只能是从0到31");
            //push_convert('R' + r_id.ToString() + ' ');
#if debug
            Debug("get r" + r_id.ToString());
#endif
            return r_id;
        }
        private void TI()
        {

        }
        private void C()
        {
            format_line.Append("\t\t;" );
            char ch = get_src_token();
            while (ch != '\n' && ch!='\r' && ch != 0)
            {
                format_line.Append(ch);
                ch = get_src_token();
            }
            return;
        }

    }
}
