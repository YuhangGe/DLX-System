using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using DLXAssembler;
using System.Text;
using System.IO;
namespace DLXLinker
{

    public partial class LinkerCore
    {
        private int cur_index;
        private char cur_token;
        private StringBuilder cur_text;
        private DLXObject cur_obj;
        //用于生成32指令时的掩码
        private const int mask_31_26 = -67108864;//0xFC000000
        private const int mask_25_21 = 0x3e00000;
        private const int mask_20_16 = 0x1f0000;
        private const int mask_15_11 = 0xf800;
        private const int mask_5_0 = 0x1f;
        private const int mask_15_0 = 0xffff;
        private const int mask_25_0 = 0x3ffffff;

        //特殊寄存器，SR(0xC),CAUSE(0xD),EPC(0xE)
        private int[] SPECIAL_REGISTER = { 0xC, 0xD, 0xE };
        private uint program_counter = 0;//PC值

        private char GetNextToken()
        {
            if (cur_index < cur_text.Length)
            {
                cur_token = cur_text[cur_index];
                cur_index += 1;
            }
            else
            {
                cur_token = (char)0;
            }
            return cur_token;
        }
        private void SkipSpace()
        {
            while (char.IsWhiteSpace(cur_token))
            {
                GetNextToken();
                if (cur_token == 0)
                    break;
            }
        }
        private string GetNextWord()
        {
            string rtn = "";
            while (!char.IsWhiteSpace(cur_token))
            {
                rtn += cur_token;
                GetNextToken();
            }
            return rtn;
        }
#if DEBUG
        public static string IntToBin32(uint num)
        {
            string rtn = Convert.ToString(num, 2);
            if (rtn.Length < 32)
            {
                string l = "";
                int left = 32 - rtn.Length;
                for (int i = 0; i < left; i++)
                    l += '0';
                rtn = l + rtn;
            }
            return rtn;
        }
#endif
        private int Merge(int src, int mask, int maskNum, int offset)
        {
            // 先把mask为1的位清零
            src = src & (~mask);

            //然后把maskNum加到src中
            src = src | (mask & (maskNum << offset));
#if DEBUG
            //Console.WriteLine(IntToBin32((uint)src));
#endif
            return src;
        }
        private void DoLinkText()
        {
            //写入Main入口
            WriteInt(main_address);

            //用户没有自定义初始地址
            if (user_address == 0)
            {
                WriteInt(1);
                WriteInt(global_text_table.TableBase);
                WriteInt(global_text_table.Length);
            }
            else
            {
                WriteInt((uint)objects.Count);
            }
            //link每个dlx_objext
            foreach (DLXObject dlx_object in objects)
            {
                cur_obj = dlx_object;
                cur_text = cur_obj.textContent;
                cur_index = 0;
                if (cur_obj.textTable.Length <= 0)
                    continue;
                program_counter = cur_obj.textTable.TableBase;
                if (user_address == 1)
                {
                    WriteInt(cur_obj.textTable.TableBase);
                    WriteInt(cur_obj.textTable.Length);
                    
                }
                GetNextToken();
                LinkEachText();
            }
        }
        private void LinkEachText()
        {
 
            while (cur_token != 0)
            {
                int cmdIndex = int.Parse(GetNextWord());
#if DEBUG
                //Console.WriteLine("cmd index:" + cmdIndex.ToString());
                Debug.Assert(cmdIndex >= 0 && cmdIndex < f_table.Length );
#endif
                SkipSpace();
                f_table[cmdIndex].Invoke();

                SkipSpace();
            }

        }
        private void PushInstruction(uint inst)
        {
            program_counter += 4;
            WriteInt(inst);
        }
        //形如 操作符(6位) SR1(5位) DR(5位) 立即数(16位)  的指令
        //包括 I-类型算术/逻辑运算  数据传送
        //和 条件转移 及 JR 、 JALR（其中dr未用，置0)
        private void PushOpSrDrImm(int instIndex, int sr, int dr, int imm)
        {
            int inst = 0;
            inst = Merge(inst, mask_25_21, sr, 21);
            inst = Merge(inst, mask_20_16, dr, 16);
            inst = Merge(inst, mask_15_0, imm, 0);
            inst = Merge(inst, mask_31_26, instIndex, 26);
            PushInstruction((uint)inst);
#if DEBUG
            //Console.WriteLine("{0} {1} {2} {3}", (DLXINST)instIndex, dr, sr, imm);
#endif
        }
     
        //R-类型算术/逻辑运算
        private void PushSr1Sr2DrOp(int instIndex, int sr1, int sr2, int dr)
        {
            int inst = 0;
            inst = Merge(inst, mask_25_21, sr1, 21);
            inst = Merge(inst, mask_20_16, sr2, 16);
            inst = Merge(inst, mask_15_11, dr, 11);
            inst = Merge(inst, mask_5_0, instIndex, 0);
            PushInstruction((uint)inst);
#if DEBUG
            //Console.WriteLine("{0} {1} {2} {3}", (DLXINST)instIndex, dr, sr1, sr2);
#endif
        }
       
   
        //形如  操作码(6位) 偏移(26位)  的指令
        //包括 J JAL TRAP 和 RFE
        private void PushOpOff26(int instIndex, int imm)
        {
            int inst = 0;
            inst = Merge(inst, mask_31_26, instIndex, 26);
            inst = Merge(inst, mask_25_0, imm, 0);
            PushInstruction((uint)inst);
#if DEBUG
          //  Console.WriteLine("{0} {1}", (DLXINST)instIndex    , imm);
#endif
        }
        //特权指令，movi2s movis2i
        private void PushMove(int instIndex, int gpr, int spr)
        {
            int inst = 0;
            inst = Merge(inst, mask_31_26, instIndex, 26);
            inst = Merge(inst, mask_20_16, gpr, 16);
            inst = Merge(inst, mask_15_11, spr, 11);
            PushInstruction((uint)inst);

        }
    }
}
