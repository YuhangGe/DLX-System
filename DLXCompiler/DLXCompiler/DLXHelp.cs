using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DLXAssembler
{
    class DLXHelp
    {
        /// <summary>
        /// 0:不需要展开;1:
        /// </summary>
        /// <param name="inst"></param>
        /// <param name="imm"></param>
        /// <returns></returns>
        public static int getExpandType( DLXINST inst, int imm)
        {
            switch (inst)
            {
                case DLXINST.LHI:
                case DLXINST.SLLI:
                case DLXINST.SRLI:
                case DLXINST.SRAI:
                    return 0;
                default:
                    break;
            }
            if (imm <= Int16.MaxValue && imm >= Int16.MinValue)
            {
                return 0;
            }
            else if ((imm & 0xffff) == 0)
            {
                return 1;
            }
            else if ((imm & 0xff00) == 0)
            {
                return 2;
            }
            else if ((imm & 0xff) == 0)
            {
                return 3;
            }
            else
            {
                return 4;
            }

        }
    }
}
