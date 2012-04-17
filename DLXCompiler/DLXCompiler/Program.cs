using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace DLXAssembler
{
    class Program
    {
       
        static int Main(string[] args)
        {
            int rtn = 0;
            try
            {

                if (args.Length < 1 || (args.Length ==1 && args[0].ToUpper ()=="-I"))
                {

                    Console.Write("DLX汇编器，由209Team编写。\n使用方法DLXAssembler  filename1 filename2 ...");
                    return 0;
                }
                int i = 0;
                bool i_case = true;
                if (args[0].ToUpper() == "-I")
                {
                    i_case = false;
                    i++;
                }
                for ( ; i < args.Length; i++)
                {
                    FileInfo fi = new FileInfo(args[i]);
                    if (fi.Exists)
                    {
                        FileStream fs = new FileStream(fi.FullName, FileMode.Open);
                        CompilerCore cc = new CompilerCore();
                        try
                        {
                            Console.WriteLine(string.Format("正在汇编文件{0}", fi.FullName));
                            DLXObject dlx = cc.Complie(fs,i_case);
                            string outname = fi.Directory.FullName + '\\' + fi.Name.Substring(0, fi.Name.LastIndexOf('.')) + ".link";
                            StreamWriter sw = new StreamWriter(outname);
                            dlx.OutputXml(sw);

                            sw.Close();
                            fs.Close();

                            Console.WriteLine(string.Format("汇编结束，发现{0}个错误，输出至{1}", 0, outname));
                            
                        }
                        catch (DLXException e)
                        {
                            DLXException cur_e = e;
                            int err_num = 0;
                            while (cur_e != null)
                            {
                                if (cur_e.IsDead == true)
                                    Console.Write("致命错误!");
                                Console.WriteLine(string.Format("错误出现在第{0}行{1}列：{2}", cur_e.Line, cur_e.Colum, cur_e.Message));
                                cur_e = cur_e.NextException;
                                err_num++;
                            }
                            if (err_num >= CompilerCore.MAX_ERROR_ALLOW)
                                Console.WriteLine(string.Format("由于错误超过{0}个，汇编过程中途中止", CompilerCore.MAX_ERROR_ALLOW));
                            else
                            {
                                Console.WriteLine(string.Format("汇编结束，发现{0}个错误", err_num));
                            }
                            fs.Close();
                            rtn =  -1;
                        }

                    }
                    else { 
                        Console.WriteLine("文件{0}不存在!", fi.Name); 
                        rtn = -1; 
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
                rtn = -1;
            }
#if DEBUG
          //  Console.ReadKey();
#endif
            return rtn;
        }
    }
}
