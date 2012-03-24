Class MainWindow 
    Private TxtMain As Abraham.DLXEditor
    Private Sub MainGrid_Loaded(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        TxtMain = New Abraham.DLXEditor
        TxtMain.VerticalAlignment = VerticalAlignment.Stretch
        TxtMain.HorizontalAlignment = VerticalAlignment.Stretch
        TxtMain.FontSize = 30

        AddHandler TxtMain.DocumentFormatted, AddressOf TxtMain_DocumentFormatted
        MainGrid.Children.Add(TxtMain)
    End Sub
    Private Sub TxtMain_DocumentFormatted(ByVal sender As Object, ByVal e As Abraham.CheckResult)
        ListBox1.Items.Clear()
        For Each er As Abraham.DLXException In e.Errors
            ListBox1.Items.Add(er)
        Next
    End Sub

    Private Sub ListBox1_MouseDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        Dim list_item As Abraham.DLXException = CType(ListBox1.SelectedItem, Abraham.DLXException)
        If list_item IsNot Nothing Then
            TxtMain.SelectDocument(list_item.Line, list_item.Colum)
        End If
    End Sub
End Class
