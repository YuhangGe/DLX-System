using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DLXAssembler
{
    /// <summary>
    /// 指令对应二进制码
    /// </summary>
    public enum DLXINST
    {
        ADD = 1,//Convert.ToInt32("000001", 2),
        ADDI =1,// Convert.ToInt32("000001", 2),
        SUB=3,//Convert.ToInt32("000011",2),
        SUBI =3,// Convert.ToInt32("000011", 2),
        AND =9,// Convert.ToInt32("001001", 2),
        ANDI =9,// Convert.ToInt32("001001", 2),
        OR = 10,//Convert.ToInt32("001010", 2),
        ORI = 10,//Convert.ToInt32("001010", 2),
        XOR = 11,//Convert.ToInt32("001011", 2),
        XORI = 11,//Convert.ToInt32("001011", 2),
        LHI = 12,//Convert.ToInt32("001100", 2),
        SLL = 13,//Convert.ToInt32("001101", 2),
        SLLI = 13,//Convert.ToInt32("001101", 2),
        SRL = 14,//Convert.ToInt32("001110", 2),
        SRLI = 14,//Convert.ToInt32("001110", 2),
        SRA = 15,//Convert.ToInt32("001111", 2),
        SRAI = 15,//Convert.ToInt32("001111", 2),
        SLT = 16,//Convert.ToInt32("010000", 2),
        SLTI = 16,//Convert.ToInt32("010000", 2),
        SLE =18, //Convert.ToInt32("010010", 2),
        SLEI = 18,//Convert.ToInt32("010010", 2),
        SEQ = 20,//Convert.ToInt32("010100", 2),
        SEQI = 20,//Convert.ToInt32("010100", 2),
        SNE = 21,//Convert.ToInt32("010101", 2),
        SNEI = 21,//Convert.ToInt32("010101", 2),
        LB = 22,//Convert.ToInt32("010110", 2),
        SB = 23,//Convert.ToInt32("010111", 2),
        LW = 28,//Convert.ToInt32("011100", 2),
        SW = 29,//Convert.ToInt32("011101", 2),
        MOVI2S = 34,//"100010",
        MOVS2I = 35, //"100011"
        BEQZ = 40,//Convert.ToInt32("101000", 2),
        BNEZ = 41,//Convert.ToInt32("101001", 2),
        J = 44,//Convert.ToInt32("101100", 2),
        JR = 45,//Convert.ToInt32("101101", 2),
        JAL = 46,//Convert.ToInt32("101110", 2),
        JALR = 47,//Convert.ToInt32("101111", 2),
        TRAP = 48,//Convert.ToInt32("110000", 2),
        RFE =49// Convert.ToInt32("110001", 2)
    }
}
