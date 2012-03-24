Public Class DLXEditor
  
    Private pre_line As Integer
    Private ignore As Boolean = True
    Public Sub FormatDocument()
        Try

            Dim checker As New DLXChecker()
            Dim cr As CheckResult = checker.Check(TxtMain.Text)

            TxtMain.Text = cr.Content.ToString()
 
            RaiseEvent DocumentFormatted(Me, cr)
        Catch eo As Exception
            Debug.Print(eo.ToString())
        End Try

    End Sub
    Public Event DocumentFormatted(ByVal sender As Object, ByVal e As CheckResult)
    Public Event DocumentChanged(ByVal sender As Object, ByVal e As EventArgs)

    Private Sub TxtMain_TextChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.TextChangedEventArgs)
        Dim cur_line As Integer = TxtMain.LineCount
        '    Debug.Print(pre_line & "," & cur_line & "," & ignore)
        If pre_line <> cur_line AndAlso ignore = False Then
            ignore = True
            Dim i As Integer = TxtMain.GetLineIndexFromCharacterIndex(TxtMain.CaretIndex)
            Dim s As Integer = TxtMain.CaretIndex - TxtMain.GetCharacterIndexFromLineIndex(i)
            FormatDocument()
            TxtMain.CaretIndex = TxtMain.GetCharacterIndexFromLineIndex(i) + s

        End If
    End Sub


    Private Sub TxtMain_PreviewKeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Input.KeyEventArgs)
        pre_line = TxtMain.LineCount
        ignore = False
    End Sub


    '无效属性，为了和Avalon版统一 
    Public Property ShowAutoComplete As Boolean
    '无效属性，为了和Avalon版统一 
    Public Property TipImages As BitmapImage()
        Get
            Return Nothing
        End Get
        Set(ByVal value As BitmapImage())
        End Set
    End Property
    '无效属性，为了和Avalon版统一 
    Public WriteOnly Property HighlightFile As String
        Set(ByVal value As String)
        End Set
    End Property

    Public Sub SelectDocument(ByVal line As Integer, ByVal colum As Integer)
        Dim index As Integer = TxtMain.GetCharacterIndexFromLineIndex(line - 1)
        Try
            Dim len As Integer = TxtMain.GetLineLength(line - 1)
            TxtMain.Focus()
            TxtMain.Select(index, len - 1)
            TxtMain.ScrollToLine(line - 1)
        Catch e As Exception
            Debug.Print(e.ToString)
        End Try
    End Sub

    Private show_line_numbers As Boolean = True
    Public Property ShowLineNumbers As Boolean
        Get
            Return show_line_numbers
        End Get
        Set(ByVal value As Boolean)
            show_line_numbers = value
        End Set
    End Property
    Public Property Text
        Get
            Return TxtMain.Text
        End Get
        Set(ByVal value)
            TxtMain.Text = value
        End Set
    End Property
    Public Overloads Property FontSize As Double
        Get
            Return TxtMain.FontSize
        End Get
        Set(ByVal value As Double)
            TxtMain.FontSize = value
        End Set
    End Property
    Public Overloads Property Foreground As Brush
        Get
            Return TxtMain.Foreground
        End Get
        Set(ByVal value As Brush)
            TxtMain.Foreground = value
        End Set
    End Property
    Public Overloads Property Background As Brush
        Get
            Return TxtMain.Background
        End Get
        Set(ByVal value As Brush)
            TxtMain.Background = value
        End Set
    End Property

End Class
