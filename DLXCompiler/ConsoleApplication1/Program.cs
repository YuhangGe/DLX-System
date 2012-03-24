using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Test
{
    class Program
    {
        public static byte[] uint_to_bytes(uint num)
        {
            byte[] rtn = new byte[4];
            rtn[3] = (byte)(num & 0x000000ff);
            rtn[2] = (byte)((num & 0x0000ff00) >> 8);
            rtn[1] = (byte)((num & 0x00ff0000) >> 16);
            rtn[0] = (byte)((num & 0xff000000) >> 24);
            return rtn;
        }
        public static uint bytes_to_uint(byte[] c)
        {
            return (uint)( c[0] << 24 | c[1] << 16 | c[2] << 8 | c[3]);
        }
        public static uint read_uint(BinaryReader br)
        {
            byte[] b = new byte[4];
            for(int i=0;i<4;i++)
                b[i] = br.ReadByte();
            return bytes_to_uint(b);
        }
        static void Main(string[] args)
        {
            BinaryReader  br = new BinaryReader (File.OpenRead(args[0]));
            br.ReadByte(); br.ReadByte(); br.ReadByte(); br.ReadByte();
            uint data_num = read_uint(br);
            Console.WriteLine("找到{0}个Data段", data_num);
            for (int i = 0; i < data_num; i++)
            {
                uint data_init = read_uint(br);
                uint data_len = read_uint(br);
                Console.WriteLine("第{0}个Data段起始位置{1}(x{2}),长度{3}(x{4})",
                       i+1,data_init ,Convert.ToString(data_init,16),
                       data_len ,Convert.ToString(data_len,16));
              
                //保证为4的倍数
              //  System.Diagnostics.Debug.Assert(data_len % 4 == 0);
                
                for (uint j = 0; j < data_len; j++)
                {
                    byte b = br.ReadByte();
                    Console.WriteLine("{0} {1}",b,(char)b);
                }
            }
            uint main_address = read_uint(br);
            Console.WriteLine("Main入口:{0}(x{1})",main_address ,Convert.ToString(main_address,16));
            uint text_num = read_uint(br);
      
            Console.WriteLine("找到{0}个Text段,", text_num );

          

            for (int i = 0; i < text_num ; i++)
            {
                uint text_init = read_uint(br);
                uint text_len = read_uint(br);
                Console.WriteLine("第{0}个Text段起始位置{1}(x{2}),长度{3}(x{4})",
                       i + 1, text_init, Convert.ToString(text_init, 16),
                       text_len, Convert.ToString(text_len, 16));

                uint end = text_len / 4;
                //保证为4的倍数
                System.Diagnostics.Debug.Assert(text_len % 4 == 0);

                for (int j = 0; j < end; j++)
                {
                    uint text = read_uint(br);
                    byte[] ts = uint_to_bytes(text);

                    Console.WriteLine("{0} {1}(x{2}) {3},{4},{5},{6} {7}{8}{9}{10})",
                        IntToBin32(text), text, Convert.ToString(text, 16),
                        ts[0], ts[1], ts[2], ts[3],
                        (char)ts[0], (char)ts[1], (char)ts[2], (char)ts[3]);
                }
            }
            br.Close();
            Console.WriteLine();
            Console.WriteLine("Finish!");
            Console.ReadKey();
        }
        public static string IntToBin32(uint num)
        {
            string rtn = Convert.ToString(num, 2);
            if (rtn.Length < 32)
            {
                string l="";
                int left = 32 - rtn.Length;
                for (int i = 0; i < left; i++)
                    l += '0';
                rtn = l + rtn;
            }
            return rtn;
        }
    }
}
