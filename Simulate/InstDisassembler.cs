using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulate
{
    class InstDisassembler
    {
        //用于生成32指令时的掩码
        private const uint mask_31_26 = 0xFC000000;
        private const uint mask_25_21 = 0x3e00000;
        private const uint mask_20_16 = 0x1f0000;
        private const uint mask_15_11 = 0xf800;
        private const uint mask_5_0 = 0x1f;
        private const uint mask_15_0 = 0xffff;
        private const uint mask_25_0 = 0x3ffffff;

       
        /// <summary>
        /// 把4个8位byte合并成一个32位uint
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static uint BytesToUint(byte[] c)
        {

            System.Diagnostics.Debug.Assert(c.Length == 4);

            return (uint)( c[0] << 24 | c[1] << 16 | c[2] << 8 | c[3]);
        }
    }
}
