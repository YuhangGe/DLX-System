//#define debug
#define meaning
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DLXAssembler
{
    /// <summary>
    /// 编译器核心的程序段汇编模块
    /// </summary>
    public partial class CompilerCore
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
            location_counter = 0;
            TPseudo();//
            TContent();

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
                            error("未声明的标记"+word , rc.row, rc.colum, false);
                        }
                        catch
                        {
                            if (error_num >= MAX_ERROR_ALLOW)
                                throw dlx_errors;
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
                        error(string.Format("global声明的标记{0}没有定义", kvp.Key), kvp.Value.row, kvp.Value.colum, false);
                    }
                    catch
                    {
                        if (error_num >= MAX_ERROR_ALLOW)
                            throw dlx_errors;
                    }
                }

            }
            Symbol main_s=null;
            if ((main_s = result.textTable.getSymbol("main")) != null)
            {
                if ( main_s.IsExtern!=true && main_s.Value[0] != 0)
                    error("Main标记必须是代码段的第一行");
            }
            result.textTable.Length = location_counter;
        }
        private void TPseudo()//程序段的伪指令区，即位于程序段顶端的.global和.extern
        {
            bool run_to_content = false;//是否汇编到程序段指令区
            while (cur_token != 0 && !run_to_content)
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
                        break;
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
                                if (cur_token == '\r' || cur_token == '\n' || cur_token == '\'' || cur_token == '/' || cur_token == ';')
                                    break;
                                else
                                    error(string.Format("当前伪指令结束后出现不支持的符号{0}", cur_token));
                            }
                            catch
                            {
                                if (tailer_error.IsDead == true || error_num >= MAX_ERROR_ALLOW)
                                    throw dlx_errors;
                                else
                                {
                                    jump_to_next_line();
                                }
                            }

                        }
                        break;
                }


            }
        }
        private void TGlobal()
        {
            skip_space();
            string word = get_word();
            word = this.ignore_case ? word.ToUpper() : word;
#if debug
            Debug("got tglobal: " + word);
#endif

#if meaning
            //添加word至.global声明过的符号列表中
            if (check_data_symbol(word)==true )
                error(string.Format("符号{0}已经在data段声明过，不能重复！", word));
            if (global_symbol_tmp.ContainsKey(word))
                error(string.Format("符号{0}已经被声明过Global,不能重复声明"));
            global_symbol_tmp.Add(word,new row_colum(line,line_at ));
#endif
        }
        private void TExtern()
        {
            skip_space();
            string word = get_word();
            word = this.ignore_case ? word.ToUpper() : word;
#if debug
            Debug("got textern: " + word);
#endif

            //添加word至.extern声明过的符号列表中
            if (check_data_symbol(word) == true)
                error(string.Format("符号{0}已经在data段声明过，不能重复！", word));
            if (extern_symbol_tmp.ContainsKey(word))
                error(string.Format("符号{0}已经被声明过Extern,不能重复声明"));
            extern_symbol_tmp.Add(word,new row_colum(line,line_at ));
            result.textTable.insertSymbol(word, false, true);

        }

        //程序段的实际指令区
        private void TContent()
        {
            while (cur_token != 0)
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
                        break;
                    default:
                        try
                        {
                            if (is_validate_token(cur_token) == true)
                            {
                                I();
                                skip_space();
                                if (cur_token == '\r' || cur_token == '\n' || cur_token == '\'' || cur_token == '/' || cur_token == ';' || cur_token==0)
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
                            if (tailer_error.IsDead == true || error_num >= MAX_ERROR_ALLOW)
                                throw dlx_errors;
                            else
                            {
                                jump_to_next_line();
                            }
                        }

                        break;
                }


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
            Symbol symbol = null;
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
#if meaning
                string upper_word = this.ignore_case ? word.ToUpper() : word ;
                if (extern_symbol_tmp.ContainsKey(upper_word))
                    error(string.Format("标记{0}已被声明为Extern，不能被定义", word));
                if (check_data_symbol(upper_word) == true)
                    error(string.Format("符号{0}已经在data段声明过，不能重复！", word));
                symbol = result.textTable.insertSymbol(upper_word);
                if (symbol == null)
                    error(string.Format("标记{0}出现重定义", word));
                if (global_symbol_tmp.ContainsKey(upper_word))
                {
                    symbol.IsGlobal = true;
                    global_symbol_tmp.Remove(upper_word);
                }
                symbol.Value.Add(location_counter);

                //把标记从等待确认标记是否存在的列表出去除
                remove_from_wait(upper_word);
#endif
                get_src_token();
                skip_space();
                word = get_word();
                w_type = get_word_type(word);
                if (w_type == WORDTYPE.LABEL)
                    error("不能识别的指令操作符\'" + word + '\'');


            }

            //得到操作码在表KEYWORDS中的具体索引，
            //该索引会记录到DLXObject中，linker阶段通过索引
            //进行表驱动
            int cmd_index = get_word_index(word);
            switch (w_type)
            {
                case WORDTYPE.ALR://I-类型算术/逻辑运算
                    I_ALR(cmd_index);
                    break;
                case WORDTYPE.ALI://R-类型算术/逻辑运算
                    I_ALI(cmd_index);
                    break;
                case WORDTYPE.LHI://LHI指令
                    I_LHI(cmd_index);
                    break;
                case WORDTYPE.LOAD ://数据传送加载指令
                    I_LOAD(cmd_index);
                    break;
                case WORDTYPE.STORE ://数据传送储存指令
                    I_STORE(cmd_index);
                    break;
                case WORDTYPE.CB://条件分支
                    I_CB(cmd_index);
                    break;
                case WORDTYPE.JMPI://使用立即数的跳，包括J和JAL
                    I_JMPI(cmd_index);
                    break;
                case WORDTYPE.TRAP://TRAP指令
                    I_TRAP(cmd_index);
                    break;
                case WORDTYPE.JMPR://使用寄存器的跳，包括JR和JALR
                    I_JMPR(cmd_index);
                    break;
                case WORDTYPE.RFE:
                    I_RFE(cmd_index);
                    break;
                case WORDTYPE.MOVE:
                    I_MOVE(cmd_index);
                    break;
                case WORDTYPE.RET://从跳转（函数）返回 ，等价于JR R31
                    I_RET(cmd_index);
                    break;
                default:
                    error("不能识别的指令码" + word);
                    break;
            }
            //push_convert("\n");

        }
        private void I_MOVE(int cmd_index)
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
#if DEBUG
            Console.WriteLine("mov_{0} r{1},{2}",cmd_index,gpr,spr_name);
#endif
            if (!special_register.ContainsKey(spr_name))
            {
                error("错误的特殊寄存器代号,请查阅手册.");
            }
            result.textContent.AppendFormat("{0} {1} {2} ", cmd_index, gpr, special_register[spr_name]);
            location_counter += 4;
        }
        /// <summary>
        /// 从跳转（函数）返回 ，等价于JR R31
        /// </summary>
        /// <param name="cmd_index"></param>
        private void I_RET(int cmd_index)
        {
#if meaning
            result.textContent.AppendFormat("{0} {1} ", get_word_index("JR"),31);
            location_counter += 4;
#endif
        }
        /// <summary>
        /// R-类型算术/逻辑运算，格式为Add r1,r2,r3
        /// </summary>
        private void I_ALR(int cmd_index)
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
#if meaning
            result.textContent.AppendFormat("{0} {1} {2} {3} ", cmd_index, r1, r2, r3);
            location_counter += 4;
#endif

        }
        /// <summary>
        /// I-类型算术/逻辑运算，格式为Addi r1,r2,#100或addi r1,r2,lable_1或addi r1,r2,str[2]
        /// </summary>
        private void I_ALI(int cmd_index)
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
                if (!this.ignore_case)
                {
                    //如果是在从编译器输出的汇编语言
                    //16位立即数仍然按照无符号32位处理
                    int imm = parse_number_32(word);
                    /* addi r1,r2,xAAAABBCC会转成
                     * ANDI R25,R25,0
	                 * LHI R25,xAAAA
	                 * SRLI R25,R25,8
	                 * ORI R25,R25,xBB
	                 * SLLI R25,R25,8
	                 * ORI R25,R25,XCC
	                 * ADD R1,R2,R25
                     * 
                     * 但是可以根据情况节省指令。
                     * */
                    int et = DLXHelp.getExpandType((DLXINST)cmd_index, imm);// 28;
                    uint d = 0;
                    if (et == 0)
                    {
                        //xAAAABBCC 在16位的范围内
                        d = 4;
                    }
                    else if (et == 1)
                    {
                        // xBBCC = 0;
                        d = 12;
                    }
                    else if (et == 2)
                    {
                        //xBBCC & x8000 = 0
                        d = 16;
                    }
                    else if (et == 3)
                    {
                        //xBB = 0 
                        d = 12;
                    }

                    else if (et == 4)
                    {
                        //xCC = 0;
                        d = 24;
                    }
                    else if (et == 5)
                    {
                        d = 28;
                    }
                    location_counter += d;
                    result.textContent.AppendFormat("{0} {1} {2} ${3} {4} ", cmd_index, dest_r, src_r, imm,et);
                }
                else
                {
                    int imm = parse_number_16(word);
                    location_counter += 4;
                    result.textContent.AppendFormat("{0} {1} {2} #{3} ", cmd_index, dest_r, src_r, imm);
                }
                
            }
            catch
            {
                delete_last_error();//删除上面try中产生的错误
                word = get_word();//读取标记
                word = this.ignore_case ? word.ToUpper() : word;

                int index = 0;

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
                }
                /**
                 * 由于addi r1,r2,numbers（假设numbers
                 * 的地址是xAAAABBCC),实际上会被翻译为
                 * AND R25,R25,0
	             * LHI R25,xAAAA(xAAAA是numbers前16位
	             * SRLI R25,R25,2
	             * ORI R25,R25,xBB
	             * SLLI R25,R25,2
	             * ORI R25,R25,XCC
	             * ADD R1,R2,R25
                
                 * 这样的7条指令，故地址计数器实际加28
                 * 这里和直接加立即数不同的是，由于标记的实际
                 * 地址未知，不能像上面那样根据情况节省指令。
                 * */
                location_counter += 28;

                if (check_symbol(word, index) == false)
                    add_to_wait(word, line, line_at);
                result.textContent.AppendFormat("{0} {1} {2} {3} {4} ", cmd_index, dest_r, src_r, word, index);


            }


        }
        /// <summary>
        /// LHI指令,格式为lhi r0,#100或lhi r0, label_1 或lhi r1,str[#2]
        /// </summary>
        private void I_LHI(int cmd_index)
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
                int imm =0;
                if (!this.ignore_case)
                {
                    imm = parse_number_32(word);
                }
                else
                {
                    imm = parse_number_16(word);
                }
               
                location_counter += 4;
                result.textContent.AppendFormat("{0} {1} #{2} ", cmd_index, dest_r, imm);

            }
            catch
            {
                //删除上面代码抛出的异常
                delete_last_error();

                word = get_word();
                word = this.ignore_case ? word.ToUpper() : word;
#if debug
                Debug("got lhi imm label:" + word);
#endif
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
                }

                location_counter += 4;
                if (check_symbol(word, index) == false)
                    add_to_wait(word, line, line_at);
                result.textContent.AppendFormat("{0} {1} {2} {3} ", cmd_index, dest_r, word, index);
            }
        }
        /// <summary>
        /// 数据传送加载指令，包括lb和  lw ，格式为lw r1, #100(r2)或lw r1,num(r2)或lw r3,num[ #2](r0)
        /// </summary>
        private void I_LOAD(int cmd_index)
        {
            skip_space();
            int dest_r = R();
            skip_space();
            if (cur_token != ',')
                error("指令不完整，期望逗号(,)");
            get_src_token();
            skip_space();

            string word = null;
            int mode = 0;//0表示使用立即数，1表示使用标记
            int imm = 0;
            int index = 0;
            //首先假设指令第三个参数是立即数，如果尝试读取立即数失败，
            //再尝试读取标记
            try
            {
                mode = 0;
                word = get_number_word();
                if (!this.ignore_case)
                {
                    /*
                     * 编译器模式
                     */
                    imm = parse_number_32(word);
                }
                else
                {
                    imm = parse_number_16(word);
                }
#if debug
                Debug("got dw imm:" + imm.ToString());
#endif
            }
            catch
            {
                mode = 1;
                //删除上面的错误
                delete_last_error();
                word = get_word();
                word = this.ignore_case ? word.ToUpper() : word;

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
                }

                if (check_symbol(word, index) == false)
                    add_to_wait(word, line, line_at);

#if debug
                Debug(string.Format("got dw imm label: {0} with offset {1}", word, index));
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
#if meaning
            if (mode == 0)
            {
                if (!this.ignore_case)
                {
                    /* sw  xAAAABBCC(r2),r1 指令实际会变成8条如 
                     *
                     * AND R25,R25,0
                     * LHI R25,xAAAA
                     * SRLI R25,R25,8
                     * ORI R25,R25,xBB
                     * SLLI R25,R25,8
                     * ORI R25,R25,XCC
                     * ADD R25,R25,R2
                     * SW #0(R25),R21
                     * 
                     * **/
                    int et = DLXHelp.getExpandType((DLXINST)cmd_index, imm);// 28;
                    uint d = 0;
                    if (et == 0)
                    {
                        //xAAAABBCC 在16位的范围内
                        d = 4;
                    }
                    else if (et == 1)
                    {
                        // xBBCC = 0;
                        d = 12;
                    }
                    else if (et == 2)
                    {
                        //xBBCC & x8000 = 0
                        d = 20;
                    }
                    else if (et == 3)
                    {
                        //xBB = 0 
                        d = 20;
                    }

                    else if (et == 4)
                    {
                        //xCC = 0;
                        d = 28;
                    }
                    else if (et == 5)
                    {
                        d = 32;
                    }
#if DEBUG
                    Console.WriteLine("load d:{0}", d);
#endif
                    location_counter += d;
                    result.textContent.AppendFormat("{0} {1} {2} ${3} {4} ", cmd_index, dest_r, src_r, imm,et);
                  
                }
                else
                {
                    result.textContent.AppendFormat("{0} {1} {2} #{3} ", cmd_index, dest_r, src_r, imm);
                    location_counter += 4;
                }
            }
            else
            {
                /* lw num(r2),r1 num地址0xAAAABBCC
                 * 指令实际会变成8条如
                 *
                 * AND R25,R25,0
	             * LHI R25,xAAAA 
	             * SRLI R25,R25,2
	             * ORI R25,R25,xBB
	             * SLLI R25,R25,2
	             * ORI R25,R25,XCC
                 * ADD R25,R25,R2
	             * SW #0(r25),r1
                 * 
                 * **/
                location_counter += 32;
                result.textContent.AppendFormat("{0} {1} {2} {3} {4} ", cmd_index, dest_r, src_r, word, index);
            }
#endif
            get_src_token();

        }
        /// <summary>
        /// 数据传送储存指令，包括sb,  sw ，格式为sw  #100(r2),r1或sw num(r2),r1或sw num[ #2](r0),r3
        /// </summary>
        private void I_STORE(int cmd_index)
        {
            skip_space();

            string word = null;
            int mode = 0;
            int index = 0;
            int imm = 0;
            //首先假设指令第一个参数是立即数，如果尝试读取立即数失败，
            //再尝试读取标记
            try
            {
                mode = 0;
                word = get_number_word();
                if (!this.ignore_case)
                {
                    /*
                     * 编译器模式
                     */
                    imm = parse_number_32(word);
                }
                else
                {
                    imm = parse_number_16(word);
                }
#if debug
                Debug("got dw imm:" + imm.ToString());
#endif
            }
            catch
            {
                mode = 1;
                //删除上面的错误
                delete_last_error();
                word = get_word();
                word = this.ignore_case ? word.ToUpper() : word;
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
                }
#if meaning
                if (check_symbol(word, index) == false)
                    add_to_wait(word, line, line_at);
#endif
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
#if meaning
            if (mode == 0)
            {
                if (!this.ignore_case)
                {
                /* sw  xAAAABBCC(r2),r1 指令实际会变成7条如 
                 *
                 * AND R25,R25,0
	             * LHI R25,xAAAA
	             * SRLI R25,R25,2
	             * ORI R25,R25,xBB
	             * SLLI R25,R25,2
	             * ORI R25,R25,XCC
                 * ADD R25,R25,R2
	             * SW #0(R25),R21
                 * 
                 * **/
                    int et = DLXHelp.getExpandType((DLXINST)cmd_index,imm);// 28;
                    uint d = 0;
                    if (et == 0)
                    {
                        //xAAAABBCC 在16位的范围内
                        d = 4;
                    }
                    else if (et == 1)
                    {
                        // xBBCC = 0;
                        d = 12;
                    }
                    else if (et == 2)
                    {
                        //xBBCC & x8000 = 0
                        d = 20;
                    }
                    else if (et == 3)
                    {
                        //xBB = 0 
                        d = 20;
                    }

                    else if (et == 4)
                    {
                        //xCC = 0;
                        d = 28;
                    }
                    else if (et == 5)
                    {
                        d = 32;
                    }
                    location_counter += d;
                    result.textContent.AppendFormat("{0} {1} {2} ${3} {4} ", cmd_index, dest_r, src_r, imm,et);
                     
                }
                else
                {
                    result.textContent.AppendFormat("{0} {1} {2} #{3} ", cmd_index, dest_r, src_r, imm);
                    location_counter += 4;
                }
            }
            else
            {
                result.textContent.AppendFormat("{0} {1} {2} {3} {4} ", cmd_index, dest_r, src_r, word, index);
                /* sw num(r2),r1 num地址0xAAAABBCC
                 * 指令实际会变成8条如
                 *
                 * AND R25,R25,0
	             * LHI R25,xAAAA 
	             * SRLI R25,R25,2
	             * ORI R25,R25,xBB
	             * SLLI R25,R25,2
	             * ORI R25,R25,XCC
                 * ADD R25,R25,R2
	             * SW #0(r25),r1
                 * 
                 * **/
                location_counter += 32;
            }
#endif
        }
        /// <summary>
        /// 条件分支指令，包括BEQZ和BNEZ，格式为beqz r2,label_1
        /// </summary>
        void I_CB(int cmd_index)
        {

            skip_space();
            int src_r = R();
            skip_space();
            if (cur_token != ',')
                error("指令不完整，期望逗号(,)");
            get_src_token();
            skip_space();
            string label = get_word();

#if debug
            Debug(String.Format("condition branch to  {0}", label));
#endif
            label = this.ignore_case ? label.ToUpper() : label;

            if (check_text_symbol(label) == false)
                add_to_wait(label, line, line_at);
            result.textContent.AppendFormat("{0} {1} {2} ", cmd_index, src_r, label);
            location_counter += 4;

        }
        /// <summary>
        /// 使用立即数的跳，包括J和JAL
        /// </summary>
        /// <param name="cmd_index"></param>
        private void I_JMPI(int cmd_index)
        {
            skip_space();
            string label = get_word();
            label = this.ignore_case ? label.ToUpper() : label;
            if (check_text_symbol(label) == false)
                add_to_wait(label, line, line_at);
            result.textContent.AppendFormat("{0} {1} ", cmd_index, label);
            location_counter += 4;

        }
        /// <summary>
        /// TRAP指令
        /// </summary>
        /// <param name="cmd_index"></param>
        private void I_TRAP(int cmd_index)
        {
            skip_space();
            int tindex = -1;
            try
            {
                string n_w = get_number_word();
                tindex = parse_number_16(n_w);
            }
            catch
            {
                delete_last_error();
                string word = get_word();
                word = this.ignore_case ? word.ToUpper() : word;
                if (trap_table.ContainsKey(word))
                {
                    tindex = trap_table[word];
                }
                else
                    error("不存在的向量符号" + word);
            }
            result.textContent.AppendFormat("{0} #{1} ", cmd_index, tindex);
            location_counter += 4;

#if debug
            Debug(String.Format("trap {0}\n", tindex));
#endif
        }
        /// <summary>
        /// 使用寄存器的跳，包括JR和JALR
        /// </summary>
        /// <param name="cmd_index"></param>
        private void I_JMPR(int cmd_index)
        {
            skip_space();
            int src_r = R();
            result.textContent.AppendFormat("{0} {1} ", cmd_index, src_r);
            location_counter += 4;
        }
        /// <summary>
        /// 从异常返回
        /// </summary>
        /// <param name="cmd_index"></param>
        void I_RFE(int cmd_index)
        {
            result.textContent.AppendFormat("{0} ", cmd_index);
            location_counter += 4;
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
#if debug
            Console.Write("-debug: Comment:");
#endif

            char ch = get_src_token();
#if debug
            Console.Write(ch.ToString());
#endif
            while (ch != '\n' && ch != 0)
            {
                ch = get_src_token();
#if debug
                Console.Write(ch.ToString());
#endif
            }
            //Console.WriteLine();
            return;
        }

    }
}
