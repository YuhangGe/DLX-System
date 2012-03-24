using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using DLXAssembler;

namespace DLXLinker
{
    /// <summary>
    /// 函数表
    /// </summary>
    /// <remarks></remarks>
    public partial class LinkerCore
    {
        private delegate void text_delegate();
        private text_delegate[] f_table = null;
        private const int inter_r = 25;//取寄存器25做为addi和sw的中介

        private uint init_data_address = 0;
        private uint init_text_address = 0;
        public LinkerCore(uint data_address, uint text_address)
        {
            init_data_address = data_address;
            init_text_address = text_address;

            /**
             * 指令函数表，对应CompilerCore中的KEYWORDS数组
             * */
            f_table = new text_delegate[] {
	         new text_delegate(I_ADD),
		     new text_delegate(I_ADDI),
		     new text_delegate(I_SUB),
             new text_delegate(I_SUBI),
             new text_delegate(I_AND),
             new text_delegate(I_ANDI),
             new text_delegate(I_OR),
             new text_delegate(I_ORI),
             new text_delegate(I_XOR),
             new text_delegate(I_XORI),
             new text_delegate(I_SLL),
             new text_delegate(I_SLLI),
             new text_delegate(I_SRL),
             new text_delegate(I_SRLI),
             new text_delegate(I_SRA),
             new text_delegate(I_SRAI),
             new text_delegate(I_SLT),
             new text_delegate(I_SLTI),
             new text_delegate(I_SLE),
             new text_delegate(I_SLEI),
             new text_delegate(I_SEQ),
             new text_delegate(I_SEQI),
             new text_delegate(I_SNE),
             new text_delegate(I_SNEI),
             new text_delegate(I_LHI),
             new text_delegate(I_LB),
             new text_delegate(I_SB),
             new text_delegate(I_LW),
             new text_delegate(I_SW),
             new text_delegate(I_BEQZ),
             new text_delegate(I_BNEZ),
             new text_delegate(I_J),
             new text_delegate(I_JAL),
             new text_delegate(I_JR),
             new text_delegate(I_JALR),
             new text_delegate(I_TRAP),
             new text_delegate(I_RFE),
             new text_delegate(I_MOVI2S),
             new text_delegate(I_MOVS2I),
             //最后一个RET没有对应函数，因为RET在Assembler阶段实际被翻译成了JR R31
             null
	                                };
        }

        private Symbol GetSymbol(string label)
        {
            //首先在局部表中查找符号
            Symbol rtn = cur_obj.dataTable.getSymbol(label);
            if (rtn == null)
            {
                rtn = cur_obj.textTable.getSymbol(label);
            }
            //如果该符号声明为extern，则在全局表中查找
            if (rtn.IsExtern == true)
            {
                rtn = global_data_table.getSymbol(label);
                if (rtn == null)
                {
                    rtn = global_text_table.getSymbol(label);
                }
            }
            return rtn;
        }
        private void I_ALI(DLXINST inst)//I-类型算术/逻辑运算
        {
            int dr = int.Parse(GetNextWord());
            SkipSpace();
            int sr = int.Parse(GetNextWord());
            SkipSpace();

            if (cur_token == '#')
            {
                GetNextToken();
                int imm = int.Parse(GetNextWord());
                PushOpSrDrImm ((int)inst, sr, dr, imm);
            }
            else if (cur_token == '$')
            {
                GetNextToken();
                int imm = int.Parse(GetNextWord());
                //清空R25
                PushOpSrDrImm((int)DLXINST.AND, inter_r, inter_r, 0);
                //OR 16位(相当于加上低16位的无符号整数）
                PushOpSrDrImm((int)DLXINST.ORI, inter_r, inter_r, imm);
                //ADD
                PushSr1Sr2DrOp((int)DLXINST.ADD, inter_r, sr, dr);
            }
            else
            {
                string label = GetNextWord();
                SkipSpace();
                int offset = int.Parse(GetNextWord());
                Symbol s = GetSymbol(label);
                if (s == null)
                {
                    ThrowError("没有找到标记:" + label);
                }
                if (offset >= s.Value.Count)
                {
                    ThrowError("错误的偏移");
                }
                int imm = (int)s.Value[offset];
                //转换为4条指令
                //清空R25
                PushOpSrDrImm((int)DLXINST.AND, inter_r, inter_r, 0);
                //LHI 指令，imm为高16位
                PushOpSrDrImm ((int)DLXINST.LHI, 0, inter_r, (int)(imm & 0xffff0000) >> 16);
                //ADDI 低16位
                PushOpSrDrImm((int)DLXINST.ORI, inter_r, inter_r, imm & 0xffff);
                //
                PushSr1Sr2DrOp ((int)inst, sr, inter_r, dr);

            
            }
        }
        private void I_ALR(DLXINST inst)//R-类型算术/逻辑运算
        {
            int dr = int.Parse(GetNextWord());
            SkipSpace();
            int sr1 = int.Parse(GetNextWord());
            SkipSpace();
            int sr2 = int.Parse(GetNextWord());
            PushSr1Sr2DrOp ((int)inst, sr1, sr2, dr);
        }
        private void I_MOVE(DLXINST inst)
        {
            int gpr = int.Parse(GetNextWord());
            SkipSpace();
            int sp_r = int.Parse(GetNextWord());
            if (Array.IndexOf<int>(SPECIAL_REGISTER, sp_r) < 0)
            {
                ThrowError("特权转移指令中使用了错误的特殊寄存器，请查阅手册。");
            }
            PushMove((int)inst, gpr, sp_r);
        }
        private void I_MOVS2I()
        {
            I_MOVE(DLXINST.MOVS2I);
        }
        private void I_MOVI2S()
        {
            I_MOVE(DLXINST.MOVI2S);
        }
        void I_ADD()
        {
            I_ALR(DLXINST.ADD);
        }
        void I_ADDI()
        {
            I_ALI(DLXINST.ADDI);
        }

        private void I_SUB()
        {
            I_ALR(DLXINST.SUB);
        }
        private void I_SUBI()
        {
            I_ALI(DLXINST.SUBI);
        }
        private void I_AND()
        {
            I_ALR(DLXINST.AND);
        }
        private void I_ANDI()
        {
            I_ALI(DLXINST.ANDI);
        }
        private void I_OR()
        {
            I_ALR(DLXINST.OR);
        }
        private void I_ORI()
        {
            I_ALI(DLXINST.ORI);
        }
        private void I_XOR()
        {
            I_ALR(DLXINST.XOR);
        }
        private void I_XORI()
        {
            I_ALI(DLXINST.XORI );
        }
        private void I_SLL()
        {
            I_ALR(DLXINST.SLL );
        }
        private void I_SLLI()
        {
            I_ALI(DLXINST.SLLI );
        }
        private void I_SRL()
        {
            I_ALR(DLXINST.SRL );
        }
        private void I_SRLI()
        {
            I_ALI(DLXINST.SRLI );
        }
        private void I_SRA()
        {
            I_ALR(DLXINST.SRA );
        }
        private void I_SRAI()
        {
            I_ALI(DLXINST.SRAI );
        }
        private void I_SLT()
        {
            I_ALR(DLXINST.SLT );
        }
        private void I_SLTI()
        {
            I_ALI(DLXINST.SLTI );
        }
        private void I_SLE()
        {
            I_ALR(DLXINST.SLE );
        }
        private void I_SLEI()
        {
            I_ALI(DLXINST.SLEI );
        }
        private void I_SEQ()
        {
            I_ALR(DLXINST.SEQ );
        }
        private void I_SEQI()
        {
            I_ALI(DLXINST.SEQI );
        }
        private void I_SNE()
        {
            I_ALR(DLXINST.SNE );
        }
        private void I_SNEI()
        {
            I_ALI(DLXINST.SNEI );
        }
        private void I_LHI()
        {
            int dr = int.Parse(GetNextWord());
            SkipSpace();
            if (cur_token != '#')
                ThrowError("LHI指令立即数错误");
            GetNextToken();
            int imm = int.Parse(GetNextWord());
            PushOpSrDrImm ((int)DLXINST.LHI, 0, dr, imm);
        }
        private void I_LOAD_STORE(DLXINST inst)
        {
            int dr = int.Parse(GetNextWord());
            SkipSpace();
            int sr = int.Parse(GetNextWord());
            SkipSpace();

            if (cur_token == '#')
            {
                GetNextToken();
                int imm = int.Parse(GetNextWord());
                PushOpSrDrImm((int)inst, sr, dr, imm);
            }
           
            else
            {
                string label = GetNextWord();
                SkipSpace();
                int offset = int.Parse(GetNextWord());
                Symbol s = GetSymbol(label);
                if (s == null)
                {
                    ThrowError("没有找到标记:" + label);
                }
                if (offset >= s.Value.Count)
                {
                    ThrowError("错误的偏移");
                }
                int imm = (int)s.Value[offset];
                //转换为5条指令
                //清空R25
                PushOpSrDrImm((int)DLXINST.AND, inter_r, inter_r, 0);
                //LHI 指令，imm为高16位
                PushOpSrDrImm((int)DLXINST.LHI, 0, inter_r, (int)(imm & 0xffff0000) >> 16);
                //OR 低16位(相当于加上低16位的无符号整数）
                PushOpSrDrImm((int)DLXINST.ORI, inter_r, inter_r, imm & 0xffff);
                //ADD
                PushSr1Sr2DrOp((int)DLXINST.ADD, inter_r, sr, inter_r);
                //Load or store
                PushOpSrDrImm((int)inst, inter_r , dr , 0);
            }
        }
        private void I_LB()
        {
            I_LOAD_STORE(DLXINST.LB);
        }
        private void I_SB()
        {
            I_LOAD_STORE(DLXINST.SB);
        }
        private void I_LW()
        {
            I_LOAD_STORE(DLXINST.LW);
        }
        private void I_SW()
        {
            I_LOAD_STORE(DLXINST.SW);
        }
        private void I_CB(DLXINST inst)
        {
            int sr = int.Parse(GetNextWord());
            SkipSpace();
            string label = GetNextWord();
            Symbol symbol = GetSymbol(label);
            if (symbol == null)
            {
                ThrowError("没有找到标记:" + label);
            }
            int imm = (int)(symbol.Value[0]-program_counter-4) ;
            PushOpSrDrImm((int)inst, sr, 0, imm);
        }
        private void I_BEQZ()
        {
            I_CB(DLXINST.BEQZ);
        }
        private void I_BNEZ()
        {
            I_CB(DLXINST.BNEZ);
        }
        private void I_J()
        {
            string label = GetNextWord();
            Symbol symbol = GetSymbol(label);
            if (symbol == null)
            {
                ThrowError("没有找到标记:" + label);
            }
            int imm = (int)(symbol.Value[0] - program_counter-4);
            PushOpOff26((int)DLXINST.J, imm);
        }
        private void I_JAL()
        {
            string label = GetNextWord();
            Symbol symbol = GetSymbol(label);
            if (symbol == null)
            {
                ThrowError("没有找到标记:" + label);
            }
            int imm = (int)(symbol.Value[0] - program_counter-4);
            PushOpOff26((int)DLXINST.JAL, imm);
        }
        private void I_JR()
        {
           int sr = int.Parse(GetNextWord());
           PushOpSrDrImm((int)DLXINST.JR, sr, 0, 0);
        }
        private void I_JALR()
        {
            int sr = int.Parse(GetNextWord());
            PushOpSrDrImm((int)DLXINST.JALR, sr, 0, 0);
        }
        private void I_TRAP()
        {
            if (cur_token != '#')
                ThrowError("TRAP指令立即数错误");
            GetNextToken();
            int imm = int.Parse(GetNextWord());
            PushOpOff26((int)DLXINST.TRAP , imm);
        }
        private void I_RFE()
        {
            PushOpOff26 ((int)DLXINST.RFE, 0);
        }
    }
}
