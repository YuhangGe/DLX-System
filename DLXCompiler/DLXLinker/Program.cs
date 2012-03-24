//#define debug
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DLXAssembler;
using System.Xml;

namespace DLXLinker
{
    class Program
    {
        static uint INIT_DATA_ADDRESS = 0x30000;
        static uint INIT_TEXT_ADDRESS = 0x100000;
        static uint data_addrss = INIT_DATA_ADDRESS;
        static uint text_address = INIT_TEXT_ADDRESS;
        static int  Main(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("DLX链接器，由209Team编写。\n使用方法DlxLinker [-d address] [-t address]  infile1 infile2 ... outfile");
                    return 0;
                }
              
                int args_index = 0;
                if (args[args_index++].Equals("-d"))
                {
                    try
                    {
                        data_addrss = uint.Parse(args[args_index++]);
                        if (!args[args_index++].Equals("-t"))
                        {
                            Console.WriteLine("如果不希望使用默认的地址，Data段和Text段的初始地址必须同时指定.");
                            return -1;
                        }
                        else
                        {
                            try
                            {
                                text_address = uint.Parse(args[args_index++]);
                                if (args.Length - 4 < 2)
                                {
                                    Console.WriteLine("缺少参数。");
                                    return -1;
                                }
                            }
                            catch
                            {
                                Console.WriteLine("Text段初始地址不合法");
                                return -1;
                            }
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Data段初始地址不合法");
                        return -1;
                    }
                }
                else
                {
                    args_index = 0;
                }

              
                List<DLXAssembler.DLXObject> dlx_list = new List<DLXAssembler.DLXObject>();
                for (; args_index < args.Length - 1; args_index++)
                {
                    FileInfo fi = new FileInfo(args[args_index]);
                    if (fi.Exists)
                    {
                        try
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.Load(fi.FullName);
                            DLXObject dlx_objext = DLXObject.FromXML(doc);
                            dlx_list.Add(dlx_objext);
                        }
                        catch
#if debug 
                        (Exception e)
#endif
                        {
#if debug 
                       Console.WriteLine(e.Message);
#endif

                        }

                    }

                }
                if (dlx_list.Count == 0)
                {
                    Console.WriteLine("加载文件失败");
                    return -1;
                }
                BufferedStream bs = null;
                try
                {
                    bs = new BufferedStream(new FileStream(args[args_index], FileMode.Create));
                }
                catch (Exception e)
                {
                    Console.WriteLine("输出文件设置错误:" + e.Message);
                    return -1;
                }
                try
                {
                    LinkerCore lc = new LinkerCore(data_addrss, text_address );
                    lc.Link(dlx_list, bs);
                    bs.Close();
                    Console.WriteLine("链接成功，输出到:" + args[args_index]);
#if DEBUG 
                    Console.ReadKey();
#endif
                    return 0;
                }
                catch (Exception e)
                {
                    bs.Close();
                    // File.Delete(args[i]);
                    Console.WriteLine("出现错误:" + e.Message + "\n链接终止");
                    return -1;
                }
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
                return -1;
            }
   
        }
      
    }
}
