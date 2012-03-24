using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System;
namespace CheckTest
{
    class Program
    {
        private static  string[] KEYWORDS = new string[]{
	         "ADD","ADDI","SUB","SUBI",
             "AND","ANDI","OR","ORI",
             "XOR","XORI","SLL","SLLI",
             "SRL","SRLI", "SRA","SRAI",
             "SLT","SLTI","SLE","SLEI",
             "SEQ","SEQI","SNE","SNEI",
              "LHI",
			 "LB","SB","LW","SW",
             "BEQZ","BNEZ","J","JAL",
             "JR","JALR","TRAP","RFE","RET"};
        static void Main(string[] args)
        {
            for (int i = 0; i < KEYWORDS.Length; i++)
                Console.Write("\"{0}\",", KEYWORDS[i]);
            Console.ReadKey();


        }
        
    }
}
