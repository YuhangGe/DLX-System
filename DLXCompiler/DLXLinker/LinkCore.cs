//#define DEBUG
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
    public partial  class LinkerCore
    {
        /// <summary>
        /// 前四个byte分别是 'd' 'l' 'x' 209,
        /// 作为文件标志，然后一个int代表有几个data段，
        /// 然后依次是每一个data段，每个data段第一个int是data起始加载位，
        /// 第二个int是该data段的长度，然后就是具体数据。
        ///
        ///所有data段读完后，第一个int是text段入口，然后第一个int是text有几段，
        ///然后是分别每个text段，每个text段第一个int是该段加载地址，
        ///然后一个int是长度，然后具体数据
        /// </summary>
        private SymbolTable global_data_table = null;
        private SymbolTable global_text_table = null;
        private int data_num = 0;//data 段数量
        private int text_num = 0;//text 段数量
        private uint main_address;//程序入口
        private int user_address = -1;//表示用户自定义初始地址,-1表示未知，0表示没有自定义，1表示自定义
        /// <summary>
        /// 检查初始地址是否合法，用户要么不指定初始地址，要么指定所有初始地址
        /// 即不能data段指定初始地址，text不指定，或一个文件里指定 ，其它文件不指定
        /// </summary>
        private void checkAddress()
        {
            foreach (DLXObject dlx_object in objects)
            {
                SymbolTable d_table = dlx_object.dataTable;
                SymbolTable t_table = dlx_object.textTable;
/*                if (d_table.TableBase != uint.MaxValue )
                {
                    if (user_address == 0)
                        ThrowError("初始地址指定不合法。");
                    user_address = 1;
                }
                else
                {
                    if (user_address == 1)
                        ThrowError("初始地址指定不合法。");
                    user_address = 0;
                }
                if (t_table.TableBase != uint.MaxValue )
                {
                    if (user_address == 0)
                        ThrowError("初始地址指定不合法。");
                    user_address = 1;
                }
                else
                {
                    if (user_address == 1)
                        ThrowError("初始地址指定不合法。");
                    user_address = 0;
                }
 */
                if (d_table.TableBase != uint.MaxValue)
                {
                    dlx_object.is_data_addressed = true;
                }
                if (t_table.TableBase != uint.MaxValue)
                {
                    dlx_object.is_text_addressed = true;
                }
                if (d_table.Length > 0 )
                    data_num++;
                if (t_table.Length > 0)
                    text_num++;
            }
        }
        /// <summary>
        /// 构造全局表
        /// </summary>
        /// <remarks></remarks>
        private void CreateGlobalTable()
        {
            uint data_lc = 0;
            //数据段计数器
           uint text_lc = 0;
            //程序段计数器
            global_data_table = new SymbolTable();
            global_text_table = new SymbolTable();
           
            global_data_table.TableBase = this.init_data_address;
            global_text_table.TableBase = this.init_text_address;
            

            foreach (DLXObject dlx_object in objects)
            {
#if DEBUG
                Console.WriteLine("Link File: {0}",dlx_object.linkFile);
#endif
                SymbolTable d_table = dlx_object.dataTable;
                SymbolTable t_table = dlx_object.textTable;
                if (!dlx_object.is_data_addressed)
                {
                    d_table.TableBase = data_lc + global_data_table.TableBase;
                    data_lc += d_table.Length;
                }
                if (!dlx_object.is_text_addressed)
                {
                    t_table.TableBase = text_lc + global_text_table.TableBase;
                    text_lc += t_table.Length;
                }
                
#if DEBUG
                Console.WriteLine("data:{0}(0x{1:x}), text:{2}(0x{3:x})", d_table.TableBase, d_table.TableBase, t_table.TableBase, t_table.TableBase);
#endif

                InsertTable(d_table, global_data_table);
                InsertTable(t_table, global_text_table);

            }
            global_data_table.Length = data_lc;
            global_text_table.Length = text_lc;
            Symbol main=global_text_table.getSymbol("main");
            if (main == null)
                ThrowError("没有找到Main入口");
            else
            {
                main_address = main.Value[0];
            }

        }
        private void InsertTable(SymbolTable table_src, SymbolTable table_dest)
        {
            foreach (KeyValuePair<string, Symbol> ks in table_src.table)
            {

                Symbol sym = ks.Value;
                //先将所有Symbol偏移转为绝对地址
                for (int i = 0; i <= sym.Value.Count - 1; i++)
                {
                    sym.Value[i] += table_src.TableBase;
                }
#if DEBUG
                if (!sym.IsExtern)
                {
                    string t_s = "";
                    if (sym.IsGlobal || sym.IsExtern)
                    {
                        t_s = "(" + (sym.IsGlobal ? "Global " : "") + (sym.IsExtern ? "Extern " : "") + ")";
                    }
                    uint t_v = sym.Value.Count > 0 ? sym.Value[0] : 0;
                    Console.WriteLine("{0}{1}: {2}(0x{3:x})", sym.Name, t_s, t_v, t_v);
                }
                
#endif
                //将声明为globbal的Symbol复制到global表中
                if (sym.IsGlobal)
                {
                    Symbol n_sym = table_dest.insertSymbol(sym.Name, true);
                    if (n_sym == null)
                    {
                        ThrowError(string.Format("出现重复的Global标记:{0}", sym.Name));
                    }
                    for (int i = 0; i <= sym.Value.Count - 1; i++)
                    {
                        n_sym.Value.Add(sym.Value[i]);
                    }
                }
            }
        }
        private List<DLXObject> objects = null;
        private StringBuilder result = new StringBuilder();
        private Stream dest_stream = null;
        public void Link(List<DLXAssembler.DLXObject> objects, Stream dest)
        {
            this.objects = objects;
            dest_stream = dest;
            checkAddress();
            CreateGlobalTable();
            DoLink();
#if DEBUG
          //  OutputGlobalTable();
#endif
        }
        private void DoLink()
        {
            //首先写入文件头:'d' 'l' 'x' 209
            WriteByte((byte)'d');
            WriteByte((byte)'l');
            WriteByte((byte)'x');
            WriteByte(209);

            DoLinkData();
            DoLinkText();
        }
        private void WriteInt(uint num)
        {
            byte[] b = CompilerCore.uint_to_bytes(num);
            for (int i = 0; i <= 3; i++)
            {
                dest_stream.WriteByte(b[i]);
            }

        }
        private void WriteByte(byte num)
        {
            dest_stream.WriteByte(num);
        }
        private void DoLinkData()
        {
            //用户没有自定义初始地址
            if (user_address == 0)
            {
                WriteInt(1);
                WriteInt(global_data_table.TableBase);
                WriteInt(global_data_table.Length);
                foreach (DLXObject dlx_object in objects)
                {
                    StringBuilder data = dlx_object.dataContent;

                    for (int i = 0; i <= data.Length - 1; i++)
                    {
                        WriteByte((byte)data[i]);
                    }
                }
            }
            else
            {
                WriteInt((uint)data_num);
                foreach (DLXObject dlx_object in objects)
                {
                    SymbolTable d_table = dlx_object.dataTable;
                    if (d_table.Length <= 0)
                        continue;
                    StringBuilder data = dlx_object.dataContent;
                    WriteInt(d_table.TableBase);
                    WriteInt(d_table.Length);

                    for (int i = 0; i <= data.Length - 1; i++)
                    {
                        dest_stream.WriteByte((byte)data[i]);
                    }
                }
            }



        }

        private void ThrowError(string msg)
        {
            throw new Exception(msg);
        }
#if DEBUG
        private void OutputGlobalTable()
        {
            Console.WriteLine(string.Format("DataTable Base='{0}(0x{1:x})' Length='{2}(0x{3:x})'", global_data_table.TableBase, global_data_table.TableBase, global_data_table.Length, global_data_table.Length));
            foreach (KeyValuePair<string, Symbol> symbol in global_data_table.table)
            {
                Console.WriteLine(symbol.Value.ToXML());
            }
            Console.WriteLine(string.Format("TextTable Base='{0}(0x{1:x})' Length='{2}(0x{3:x})'", global_text_table.TableBase, global_text_table.TableBase, global_text_table.Length, global_text_table.Length));
            foreach (KeyValuePair<string, Symbol> symbol in global_text_table.table)
            {
                Console.WriteLine(symbol.Value.ToXML());
            }
        }
#endif
    }


}
