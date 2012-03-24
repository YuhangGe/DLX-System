using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit;

namespace Edit
{
    /// <summary>
    /// CodeEditPanel.xaml 的交互逻辑
    /// </summary>
    public partial class CodeEditPanel : UserControl
    {
        public CodeEditPanel()
        {
            InitializeComponent();
        }
 
        public static Block BlockGet(BlockCollection blocks, int index)
        {
            if (index >= blocks.Count || index < 0)
                throw new IndexOutOfRangeException(blocks.ToString() +" "+ index);
            IEnumerator<Block> enumeratorBlock = blocks.GetEnumerator();
            enumeratorBlock.Reset();
            for (int i = 0; i < index; i++)
                enumeratorBlock.MoveNext();
            return enumeratorBlock.Current;
        }
        public static void BlockType(Block b)
        {
            if (b is Paragraph)
                Debug.WriteLine("paragraph");
            if (b is Section)
                Debug.WriteLine("section");
            if (b is BlockUIContainer)
                Debug.WriteLine("blockUIcontainer");
        }
    }
}
