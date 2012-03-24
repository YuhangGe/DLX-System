Public Class DLXCompletionData
    Implements ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData
    'Private Shared Images() As BitmapImage = { _
    '    New BitmapImage(New Uri(Environment.CurrentDirectory + "\instruction.png")), _
    '    New BitmapImage(New Uri(Environment.CurrentDirectory + "\label.png")), _
    '    New BitmapImage(New Uri(Environment.CurrentDirectory + "\directive.png"))}
    Public Shared Property Images() As BitmapImage()

    Public Enum COMPLETIONTYPE
        INSTRUCTION = 0
        LABEL = 1
        DIRECTIVE = 2
    End Enum
    Private _text As String
    Private _type As COMPLETIONTYPE
    Public Sub New(ByVal text As String, ByVal type As COMPLETIONTYPE)
        Me._text = text
        Me._type = type
    End Sub
    Public Sub Complete(ByVal textArea As ICSharpCode.AvalonEdit.Editing.TextArea, ByVal completionSegment As ICSharpCode.AvalonEdit.Document.ISegment, ByVal insertionRequestEventArgs As System.EventArgs) Implements ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData.Complete
        Dim r_str As String = Nothing
        Dim new_com As New ICSharpCode.AvalonEdit.Document.TextSegment
        new_com.StartOffset = completionSegment.Offset
        new_com.Length = completionSegment.Length
        If (_type <> COMPLETIONTYPE.DIRECTIVE) Then
            new_com.StartOffset -= 1
            new_com.Length += 1
        End If
        textArea.Document.Replace(new_com, Me._text + " ")
    End Sub

    Public ReadOnly Property Content As Object Implements ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData.Content
        Get
            Return Me.Text
        End Get
    End Property

    Public ReadOnly Property Description As Object Implements ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData.Description
        Get
            Select Case _type
                Case COMPLETIONTYPE.DIRECTIVE
                    Return "伪指令：" + Me.Text
                Case COMPLETIONTYPE.INSTRUCTION
                    Return "指令：" + Me.Text
                Case COMPLETIONTYPE.LABEL
                    Return "当前程序已声明标记：" + Me.Text
            End Select
            Return "未知"
        End Get
    End Property

    Public ReadOnly Property Image As System.Windows.Media.ImageSource Implements ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData.Image
        Get
            If (Images.Length = 3) Then
                Return Images(_type)
            End If
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property Priority As Double Implements ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData.Priority
        Get
            Return 0
        End Get
    End Property
    ReadOnly Property Text As String Implements ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData.Text
        Get
            Return Me._text
        End Get
    End Property
End Class
