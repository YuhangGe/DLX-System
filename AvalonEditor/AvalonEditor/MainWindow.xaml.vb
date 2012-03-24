Imports ICSharpCode.AvalonEdit
Imports ICSharpCode.AvalonEdit.Highlighting
Imports ICSharpCode.AvalonEdit.CodeCompletion
Imports Abraham
Imports System.Collections.Specialized
Class MainWindow
 

    Private AEditor As TextEditor
    Private completionWindow As CompletionWindow
    Public Sub New()

        ' 此调用是设计器所必需的。
        InitializeComponent()

        ' 在 InitializeComponent() 调用之后添加任何初始化。


        AEditor = New TextEditor
        AEditor.FontFamily = New FontFamily("Consolas")
        Try
            Dim xr As Xml.XmlReader = Xml.XmlReader.Create("Highlight.xshd")
            AEditor.SyntaxHighlighting = Xshd.HighlightingLoader.Load(Xshd.HighlightingLoader.LoadXshd(xr), HighlightingManager.Instance)
        Catch ex As Exception
            Debug.Print(ex.ToString())
        End Try
        AddHandler AEditor.TextArea.TextEntered, AddressOf textEditor_TextArea_TextEntered
        AddHandler AEditor.TextArea.TextEntering, AddressOf textEditor_TextArea_TextEntering
        AddHandler AEditor.Document.LineCountChanged, AddressOf textEditor_Document_LineCountChanged
        Grid1.Children.Add(AEditor)
    End Sub
    Private line_changed As Integer = 0
    Private Sub textEditor_TextArea_Caret_PositionChanged(ByVal sender As Object, ByVal e As EventArgs)
        line_changed += 1
        If (line_changed = 1) Then

            line_changed = False
            Dim cp As TextViewPosition = AEditor.TextArea.Caret.Position
            FormatDocument()
            AEditor.TextArea.Caret.Position = cp
        Else
            line_changed = 0
        End If
    End Sub
    Private Sub textEditor_Document_LineCountChanged(ByVal sender As Object, ByVal e As EventArgs)
        line_changed += 1
        If line_changed = 1 Then
            line_changed = False
            FormatDocument()
        Else
            line_changed = 0
        End If
    End Sub
    Private Directive() As String = {"DATA", "ASCIIZ", "ASCII", "WORD", "SPACE", "TEXT", _
                                     "GLOBAL", "BYTE"}
    Private Keywords() As String = {"ADD", "ADDI", "SUB", "SUBI", "AND", "ANDI", _
                                    "OR", "ORI", "XOR", "XORI", "SLL", "SLLI", "SRL", _
                                    "SRLI", "SRA", "SRAI", "SLT", "SLTI", "SLE", "SLEI", _
                                    "SEQ", "SEQI", "SNE", "SNEI", "LHI", "LB", "SB", "LW", _
                                    "SW", "BEQZ", "BNEZ", "J", "JAL", "JR", "JALR", _
                                    "TRAP", "RFE", "RET"}
    Private Labels As String() = {}
    Private Function getSimilarInstructions(ByVal c As String) As String()
        Dim rtn As List(Of String) = New List(Of String)
        For Each kw As String In Keywords
            If kw.StartsWith(c.ToUpper()) Then
                rtn.Add(kw)
            End If
        Next
        Return rtn.ToArray()
    End Function
    Private Function getSimilarLabels(ByVal c As String) As String()
        Dim rtn As New List(Of String)
        For Each l As String In Labels
            If l.StartsWith(c.ToUpper()) Then
                rtn.Add(l)
            End If
        Next
        Return rtn.ToArray()
    End Function
    Private Sub textEditor_TextArea_TextEntering(ByVal sender As Object, ByVal e As TextCompositionEventArgs)
        If (e.Text = " " AndAlso completionWindow IsNot Nothing) Then
            completionWindow.CompletionList.RequestInsertion(e)
            e.Handled = True
        End If
    End Sub
    Private Sub textEditor_TextArea_TextEntered(ByVal sender As Object, ByVal e As TextCompositionEventArgs)
        If e.Text = "." Then
            If (completionWindow IsNot Nothing) Then
                Exit Sub
            End If
            completionWindow = New CompletionWindow(AEditor.TextArea)

            Dim data As IList(Of ICompletionData) = completionWindow.CompletionList.CompletionData
            For Each d As String In Directive
                data.Add(New DLXCompletionData(d, DLXCompletionData.COMPLETIONTYPE.DIRECTIVE))
            Next
            completionWindow.Show()

            AddHandler completionWindow.Closed, AddressOf completionWinodw_Closed
            ' AddHandler completionWindow.CompletionList.InsertionRequested, AddressOf completionWindow_CompletionList_InsertionRequest
            AddHandler CType(completionWindow.CompletionList.ListBox.Items, INotifyCollectionChanged).CollectionChanged, AddressOf Window1_CollectionChanged
        ElseIf Char.IsLetter(e.Text(0)) OrElse e.Text(0) = "$" Then
            If (completionWindow IsNot Nothing) Then
                Exit Sub
            End If
            Dim i_similars As String() = getSimilarInstructions(e.Text)
            Dim l_similars As String() = getSimilarLabels(e.Text)
            If i_similars.Length > 0 OrElse l_similars.Length > 0 Then
                completionWindow = New CompletionWindow(AEditor.TextArea)

                Dim data As IList(Of ICompletionData) = completionWindow.CompletionList.CompletionData
                For Each d As String In i_similars
                    data.Add(New DLXCompletionData(d, DLXCompletionData.COMPLETIONTYPE.INSTRUCTION))
                Next
                For Each d As String In l_similars
                    data.Add(New DLXCompletionData(d, DLXCompletionData.COMPLETIONTYPE.LABEL))
                Next
                completionWindow.Show()
                ' AddHandler completionWindow.CompletionList.InsertionRequested, AddressOf completionWindow_CompletionList_InsertionRequest
                AddHandler completionWindow.Closed, AddressOf completionWinodw_Closed
                AddHandler CType(completionWindow.CompletionList.ListBox.Items, INotifyCollectionChanged).CollectionChanged, AddressOf Window1_CollectionChanged
            End If
        End If

    End Sub
    Private Sub completionWindow_CompletionList_InsertionRequest(ByVal sender As Object, ByVal e As EventArgs)

    End Sub
    Private Sub Window1_CollectionChanged(ByVal sender As Object, ByVal e As NotifyCollectionChangedEventArgs)
        If (completionWindow.CompletionList.ListBox.Items.Count = 0) Then
            completionWindow.Close()
        End If

    End Sub
    Private Sub completionWinodw_Closed(ByVal sender As Object, ByVal e As EventArgs)
        completionWindow = Nothing
    End Sub
    Private Sub BtnTest_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        FormatDocument()

    End Sub
    Private Sub FormatDocument()
        Try
            Dim cp As TextViewPosition = AEditor.TextArea.Caret.Position
            Dim doc As Document.TextDocument = AEditor.Document
            Dim checker As New DLXChecker()
            Dim cr As CheckResult = checker.Check(doc.Text)
            List1.Items.Clear()
            For Each ex As DLXException In cr.Errors
                List1.Items.Add(ex)
            Next
            doc.Text = cr.Content.ToString()
            AEditor.TextArea.Caret.Position = cp
            Labels = cr.Table.ToArray()
        Catch eo As Exception
            Debug.Print(eo.ToString())
        End Try


    End Sub
    Private Sub List1_MouseDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        Dim ex As DLXException = List1.SelectedItem
        If (ex IsNot Nothing) Then
            Dim index As Integer = 0
            For i As Integer = 0 To ex.Line - 2
                Dim line = AEditor.Document.Lines(i)
                index += line.Length + 2 '\r\n
            Next
            index += ex.Colum - 1
            AEditor.Select(index, 2)
        End If
    End Sub

    Private Sub Window_ContentRendered(ByVal sender As System.Object, ByVal e As System.EventArgs)
        AEditor.Text = ".data" & vbCrLf & vbCrLf & ".text"

    End Sub
End Class
