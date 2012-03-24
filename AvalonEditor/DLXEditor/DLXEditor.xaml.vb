Imports ICSharpCode.AvalonEdit
Imports ICSharpCode.AvalonEdit.CodeCompletion
Imports ICSharpCode.AvalonEdit.Highlighting
Imports Abraham
Imports System.Collections.Specialized
Public Class DLXEditor
    Private AEditor As TextEditor
<<<<<<< .mine
    Public Overloads Property FontSize As Double
        Get
            Return AEditor.FontSize
        End Get
        Set(ByVal value As Double)
            AEditor.FontSize = value
        End Set
    End Property
=======


    Public Property ShowAutoComplete As Boolean = True
       


>>>>>>> .r438
    Public Property Text
        Get
            Return AEditor.Text
        End Get
        Set(ByVal value)
            AEditor.Text = value
        End Set
    End Property
    Public Overloads Sub Focus()
        MyBase.Focus()
        AEditor.Focus()
    End Sub
    Public Sub SelectDocument(ByVal line As Integer, ByVal colum As Integer)
        Dim index As Integer = AEditor.Document.GetOffset(line, 0)
        Try
            Dim len As Integer = AEditor.Document.Lines(line - 1).Length
            AEditor.Select(index, len + 1)
            AEditor.ScrollToLine(line)
        Catch e As Exception
            Debug.Print(e.ToString)
        End Try
    End Sub
    Public Property TipImages As BitmapImage()
        Get
            Return DLXCompletionData.Images
        End Get
        Set(ByVal value As BitmapImage())
            DLXCompletionData.Images = value
        End Set
    End Property
    Public Property ShowLineNumbers As Boolean
        Get
            Return AEditor.ShowLineNumbers
        End Get
        Set(ByVal value As Boolean)
            AEditor.ShowLineNumbers = value
        End Set
    End Property
    Private completionWindow As CompletionWindow
    Public Sub New()
        ' 此调用是设计器所必需的。
        InitializeComponent()


        AEditor = New TextEditor
        AEditor.FontFamily = New FontFamily("Consolas")
        AEditor.ShowLineNumbers = True
        AddHandler AEditor.TextArea.TextEntered, AddressOf textEditor_TextArea_TextEntered
        AddHandler AEditor.TextArea.TextEntering, AddressOf textEditor_TextArea_TextEntering
        AddHandler AEditor.Document.LineCountChanged, AddressOf textEditor_Document_LineCountChanged
        AddHandler AEditor.Document.TextChanged, AddressOf textEditor_Document_TextChanged
        MainGrid.Children.Add(AEditor)
    End Sub
    Private Sub textEditor_Document_TextChanged(ByVal sender As Object, ByVal e As EventArgs)
        RaiseEvent DocumentChanged(Me, e)
    End Sub
    Public WriteOnly Property HighlightFile As String
        Set(ByVal value As String)
            Try
                Dim xr As Xml.XmlReader = Xml.XmlReader.Create(value)
                AEditor.SyntaxHighlighting = Xshd.HighlightingLoader.Load(Xshd.HighlightingLoader.LoadXshd(xr), HighlightingManager.Instance)
            Catch ex As Exception
                Debug.Print(ex.ToString())
            End Try
        End Set
    End Property
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
    Private Shared Directive() As String = {"DATA", "ASCIIZ", "ASCII", "WORD", "SPACE", "TEXT", _
                                     "GLOBAL", "BYTE"}
    Private Shared Keywords() As String = {"ADD", "ADDI", "SUB", "SUBI", "AND", "ANDI", _
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
        If ShowAutoComplete = False Then
            Return
        End If
        If (e.Text = " " AndAlso completionWindow IsNot Nothing) Then
            completionWindow.CompletionList.RequestInsertion(e)
            e.Handled = True
        End If
    End Sub
    Private Sub textEditor_TextArea_TextEntered(ByVal sender As Object, ByVal e As TextCompositionEventArgs)
        If ShowAutoComplete = False Then
            Return
        End If
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

                AddHandler completionWindow.Closed, AddressOf completionWinodw_Closed
                AddHandler CType(completionWindow.CompletionList.ListBox.Items, INotifyCollectionChanged).CollectionChanged, AddressOf Window1_CollectionChanged
            End If
        End If

    End Sub

    Private Sub Window1_CollectionChanged(ByVal sender As Object, ByVal e As NotifyCollectionChangedEventArgs)
        If (CompletionWindow.CompletionList.ListBox.Items.Count = 0) Then
            CompletionWindow.Close()
        End If
    End Sub
    Private Sub completionWinodw_Closed(ByVal sender As Object, ByVal e As EventArgs)
        CompletionWindow = Nothing
    End Sub
    Public Sub FormatDocument()
        Try
            Dim cp As TextViewPosition = AEditor.TextArea.Caret.Position
            Dim doc As Document.TextDocument = AEditor.Document
            Dim checker As New DLXChecker()
            Dim cr As CheckResult = checker.Check(doc.Text)
            doc.Text = cr.Content.ToString()
            AEditor.TextArea.Caret.Position = cp
            Labels = cr.Table.ToArray()

            RaiseEvent DocumentFormatted(Me, cr)
        Catch eo As Exception
            Debug.Print(eo.ToString())
        End Try

    End Sub
    Public Overloads Property FontSize As Double
        Get
            Return AEditor.FontSize
        End Get
        Set(ByVal value As Double)
            AEditor.FontSize = value
        End Set
    End Property

    Public Event DocumentFormatted(ByVal sender As Object, ByVal e As CheckResult)
    Public Event DocumentChanged(ByVal sender As Object, ByVal e As EventArgs)

End Class