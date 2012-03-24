#Const DEBUG = True
Imports DLXCompiler
Imports System.Text
Imports System.IO
Public Class LinkerCore
    Private global_data_table As SymbolTable = Nothing
    Private global_text_table As SymbolTable = Nothing
    ''' <summary>
    ''' 构造全局表
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub CreateGlobalTable()
        Dim data_lc As Integer = 0 '数据段计数器
        Dim text_lc As Integer = 0 '程序段计数器
        global_data_table = New SymbolTable()
        global_data_table.TableBase = 400000
        global_text_table = New SymbolTable()
        global_text_table.TableBase = 10000000
        For Each dlx_object As DLXObject In objects
            Dim d_table As SymbolTable = dlx_object.dataTable
            Dim t_table As SymbolTable = dlx_object.textTable
            d_table.TableBase = data_lc + global_data_table.TableBase
            t_table.TableBase = text_lc + global_text_table.TableBase
            InsertTable(d_table, global_data_table)
            InsertTable(t_table, global_text_table)
            data_lc += d_table.Length
            text_lc += t_table.Length
        Next
        global_data_table.Length = data_lc
        global_text_table.Length = text_lc
    End Sub
    Private Sub InsertTable(ByVal table_src As SymbolTable, ByVal table_dest As SymbolTable)
        For Each ks In table_src.table
            Dim sym As Symbol = ks.Value
            '先将所有Symbol偏移转为绝对地址
            For i As Integer = 0 To sym.Value.Count - 1
                sym.Value(i) += table_src.TableBase
            Next
            '将声明为globbal的Symbol复制到global表中
            If sym.IsGlobal = True Then
                Dim n_sym As Symbol = table_dest.insertSymbol(sym.Name, True)
                If n_sym Is Nothing Then
                    ThrowError(String.Format("出现重复的Global标记:{0}", sym.Name))
                End If
                For i As Integer = 0 To sym.Value.Count - 1
                    n_sym.Value.Add(sym.Value(i))
                Next
            End If
        Next
    End Sub
    Private objects As List(Of DLXObject) = Nothing
    Private result As StringBuilder = New StringBuilder
    Private dest_stream As Stream = Nothing
    Public Sub Link(ByVal objects As List(Of DLXCompiler.DLXObject), ByVal dest As Stream)
        Me.objects = objects
        dest_stream = dest
        CreateGlobalTable()
        DoLink()
#If Debug = True Then
        OutputGlobalTable()
#End If
    End Sub
    Private Sub DoLink()
        DoLinkData()
        DoLinkText()
    End Sub
    Private Sub DoLinkData()
        Dim b() As Byte = CompilerCore.int_to_bytes(global_data_table.Length)
        For i As Integer = 0 To 3
            dest_stream.WriteByte(b(i))
        Next

        For Each dlx_object As DLXObject In objects
            Dim data As StringBuilder = dlx_object.dataContent
            Dim c(4) As Char
#If DEBUG Then
            Debug.Assert(data.Length Mod 4 = 0)
#End If
            For i As Integer = 0 To data.Length - 1
                dest_stream.WriteByte(Convert.ToByte(data(i)))
            Next
        Next
    End Sub

    Private Sub ThrowError(ByVal msg As String)
        Throw New Exception(msg)
    End Sub
#If Debug = True Then
    Private Sub OutputGlobalTable()
        Console.WriteLine(String.Format("DataTable Base='{0}' Length='{1}'", global_data_table.TableBase, global_data_table.Length))
        For Each symbol In global_data_table.table
            Console.WriteLine(symbol.Value.ToXML())
        Next
        Console.WriteLine(String.Format("TextTable Base='{0}' Length='{1}'", global_text_table.TableBase, global_text_table.Length))
        For Each symbol In global_text_table.table
            Console.WriteLine(symbol.Value.ToXML())
        Next
    End Sub
#End If
End Class
