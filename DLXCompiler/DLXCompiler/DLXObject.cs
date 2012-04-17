//#define debug
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
namespace DLXAssembler
{
    /**
     * 用于符号表的数据结构，value表示值，如果该符号是label,value代表
     * 其地址（相对），如果符号是常量，value代表常量值
     * isGlobal表示当符号代表Label的时候其是否是全局符号
     * isExtern表示当前符号是否是外部引用符号
     **/
    public class Symbol
    {
        public List<uint> Value=null;
        public bool IsGlobal;
        public bool IsExtern;
        public string Name=null;
        public Symbol(string name, bool isGlobal=false,bool isExtern=false)
        {
            this.Name = name;
            this.Value = new List<uint>();
            this.IsExtern = isExtern;
            this.IsGlobal = isGlobal;
        }
        public Symbol(string name, uint value, bool isGlobal = false, bool isExtern = false)
        {
            this.Name = name;
            this.Value = new List<uint>();
            this.Value.Add(value);
            this.IsExtern = isExtern;
            this.IsGlobal = isGlobal;
        }
        public string ToXML()
        {
            StringBuilder rtn = new StringBuilder();
            rtn.AppendFormat("<Symbol name='{0}' isGlobal='{1}' isExtern='{2}' >",
                this.Name,this.IsGlobal,this.IsExtern );
            int count = this.Value.Count;
            if (count > 0)
            {
                rtn.Append(this.Value[0]);
                for (int i = 1; i < count; i++)
                    rtn.AppendFormat(",{0}", this.Value[i]);
            }
            rtn.Append("</Symbol>");
            return rtn.ToString();

        }
    }
    //符号表
    public class SymbolTable 
    {
        private uint table_base = 0;//符号表基址
        public uint TableBase
        {
            get { return table_base; }
            set
            {
                if (value % 4 == 0 || value==uint.MaxValue )
                     table_base = value;
                else
                    throw new ArgumentException("符号表基地址必须是4的倍数");
            }
        }
        public uint Length = 0;//表对应段（程序或数据段）的大小，以byte为单位
        public  Dictionary<string,Symbol > table = new Dictionary<string, Symbol>();

        public Symbol insertSymbol(string symbol ,bool isGlobal=false,bool isExtern=false)
        {
            if (table.ContainsKey(symbol.ToLower()))
                return null;
            Symbol rtn=new Symbol(symbol ,isGlobal ,isExtern );
            table.Add(symbol.ToLower(), rtn);
            return rtn;
        }
        public Symbol  getSymbol(string symbol)
        {
            if (table.ContainsKey(symbol.ToLower()))
                return table[symbol.ToLower()];
            else
                return null;
        }
        public uint getSymbol(string symbol, int index)
        {
            if (table.ContainsKey(symbol.ToLower ()))
                return table[symbol.ToLower()].Value[index];
            else
                return uint.MaxValue ;
        }
    }
    /**
     * DLX汇编之后生成的Object，linker可以对其进行link
     **/
    public class DLXObject
    {
        public bool is_data_addressed = false;
        public bool is_text_addressed = false;
        public String linkFile = "";
        //数据段(data)符号表
        public  SymbolTable dataTable= new SymbolTable();
        //程序段(text)符号表
        public SymbolTable textTable = new SymbolTable();
        //数据段汇编后的代码
        public  StringBuilder dataContent = new StringBuilder();
        //程序段汇编后的代码
        public StringBuilder textContent = new StringBuilder();

        /// <summary>
        /// 从XML得到DLXObject
        /// </summary>
        /// <returns></returns>
        public static DLXObject FromXML(XmlDocument doc)
        {
            try
            {

                DLXObject rtn = new DLXObject ();
                XmlNode data = doc.GetElementsByTagName("Data")[0];
                ParseData(data, rtn);

                XmlNode text = doc.GetElementsByTagName("Text")[0];
                ParseText(text, rtn);
          
                return rtn;
            }
            catch
            {
                return null;
            }
           
        }
        private static void ParseData(XmlNode data, DLXObject rtn)
        {
            XmlNode data_table = data.FirstChild;
            XmlNode data_content = data.LastChild;
            rtn.dataTable.Length = uint.Parse(data_table.Attributes["Length"].Value);
            rtn.dataTable.TableBase = uint.Parse(data_table.Attributes["Base"].Value);
            foreach (XmlNode symbol in data_table.ChildNodes)
            {
                Symbol s = rtn.dataTable.insertSymbol(symbol.Attributes["name"].Value,
                    bool.Parse(symbol.Attributes["isGlobal"].Value), bool.Parse(symbol.Attributes["isExtern"].Value));
                string values = symbol.InnerText ;
                if (!string.IsNullOrEmpty(values) && !Regex.IsMatch(values, @"^\s*$"))
                {
                    string[] vs = values.Trim().Split(',');
                    for (int i = 0; i < vs.Length; i++)
                        s.Value.Add(uint.Parse(vs[i]));
                }
            }
            string con =  data_content.InnerText;
            for (int i = 0; i <= con.Length - 2; i += 2)
            {
                rtn.dataContent.Append((char)Convert.ToByte(con.Substring(i, 2), 16));

            }
        }
        private static void ParseText(XmlNode text, DLXObject rtn)
        {
            XmlNode text_table = text.FirstChild;
            XmlNode text_content = text.LastChild;
            rtn.textTable.Length = uint.Parse(text_table.Attributes["Length"].Value);
            rtn.textTable.TableBase = uint.Parse(text_table.Attributes["Base"].Value);
            foreach (XmlNode symbol in text_table.ChildNodes)
            {
                Symbol s = rtn.textTable.insertSymbol(symbol.Attributes["name"].Value,
                    bool.Parse(symbol.Attributes["isGlobal"].Value), bool.Parse(symbol.Attributes["isExtern"].Value));
                string values = symbol.InnerText ;
                if (!string.IsNullOrEmpty(values) && !Regex.IsMatch(values, @"^\s*$"))
                {
                    string[] vs = values.Trim().Split(',');
                    for (int i = 0; i < vs.Length; i++)
                        s.Value.Add(uint.Parse(vs[i]));
                }
            }
            rtn.textContent.Append(text_content.InnerText);
        }
        /// <summary>
        /// 以xml格式输出该DLXObject对象
        /// </summary>
        /// <param name="tw"></param>
        public void OutputXml(TextWriter  tw)
        {
            /*
             * to do
             * */
            OutputData(tw);
        }
        /// <summary>
        /// 输出数据段
        /// </summary>
        /// <param name="tw"></param>
        private void OutputData(TextWriter tw)
        {
#if debug
            System.Diagnostics.Debug.Assert(this.dataContent .Length % 4 == 0);
         //   System.Diagnostics.Debug.Assert(this.textContent .Length % 4 == 0);
#endif
            tw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<DLXObject>");
            tw.WriteLine(string.Format("<Data>\r\n\t<DataTable Base='{0}' Length='{1}'>", this.dataTable.TableBase,dataTable.Length ));
            foreach (KeyValuePair<string, Symbol> symbol in dataTable.table)
            {
                tw.Write("\t\t");
                tw.WriteLine(symbol.Value.ToXML());
            }
            tw.Write("\t</DataTable>\r\n\t<DataContent>");
            int len = this.dataContent.Length;
            if (len > 0)
            {
                for (int i = 0; i < len; i++)
                {
                    tw.Write(string.Format("{0:X2}",(byte)dataContent[i]));
                }
            }
            tw.WriteLine("</DataContent>\r\n</Data>\r\n<Text>\r\n");
            tw.WriteLine(string.Format("\t<TextTable Base='{0}' Length='{1}'>", textTable.TableBase,textTable.Length ));
            foreach (KeyValuePair<string, Symbol> symbol in textTable.table)
            {
                tw.Write("\t\t");
                tw.WriteLine(symbol.Value.ToXML());
            }
            tw.Write("\t</TextTable>\r\n\t<TextContent>");
            tw.Write(textContent.ToString());
            tw.WriteLine("</TextContent>\r\n</Text>\r\n</DLXObject>");

        }
    }
}
