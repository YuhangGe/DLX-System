Imports DLXCompiler
Imports System.Text
Imports System.IO
Partial Public Class LinkerCore
    Private cur_index As Integer
    Private cur_token
    Private cur_text As StringBuilder
    Private cur_obj As DLXObject
    '用于生成32指令时的掩码
    Private Const mask_31_26 As Integer = &HFC000000
    Private Const mask_25_21 As Integer = &H3E00000
    Private Const mask_20_16 As Integer = &H1F0000
    Private Const mask_15_11 As Integer = &HF800
    Private Const mask_5_0 As Integer = &H1F
    Private Const mask_15_0 As Integer = &HFFFF
    Private Const mask_25_0 As Integer = &H3FFFFFF

    Private Function GetNextToken() As Char
        If cur_index < cur_text.Length Then
            cur_token = cur_text(cur_index)
            cur_index += 1
        Else
            cur_token = Chr(0)
        End If
        Return cur_token
    End Function
    Private Sub SkipSpace()
        While Char.IsWhiteSpace(cur_token)
            GetNextToken()
            If (cur_token = Chr(0)) Then
                Exit While
            End If
        End While
    End Sub
    Private Function GetNextWord() As String
        Dim rtn As String = ""
        While Not Char.IsWhiteSpace(cur_token)
            rtn += cur_token
            GetNextToken()
        End While
        Return rtn
    End Function
    Private Sub DoLinkText()
        For Each dlx_object As DLXObject In objects
            cur_obj = dlx_object
            cur_text = cur_obj.textContent
            cur_index = 0
            GetNextToken()
            LinkEachText()
        Next
    End Sub
    Private Sub LinkEachText()
        While cur_token <> Chr(0)
            Dim cmdIndex As Integer = Integer.Parse(GetNextWord())
#If Debug = True Then
            Console.WriteLine("cmd index:" + cmdIndex.ToString())
            Debug.Assert(cmdIndex >= 0 AndAlso cmdIndex < KEYWORDS_NUM)
#End If
            SkipSpace()
            f_table(cmdIndex).Invoke()
         
            SkipSpace()
        End While
      
    End Sub
    Private Sub PushInst(ByVal inst As Integer)
        Dim b() As Byte = CompilerCore.int_to_bytes(inst)
        For i As Integer = 0 To 3
            dest_stream.WriteByte(b(i))
        Next
    End Sub
    Private Sub PushALI(ByVal instIndex As Integer, ByVal sr As Integer, ByVal dr As Integer, ByVal imm As Integer) 'I-类型算术/逻辑运算
        Dim inst As Integer = 0
        inst = Merge(inst, mask_25_21, sr, 21)
        inst = Merge(inst, mask_20_16, dr, 16)
        inst = Merge(inst, mask_15_0, imm, 0)
        inst = Merge(inst, mask_31_26, instIndex, 26)
        PushInst(inst)
    End Sub
    'R-类型算术/逻辑运算
    Private Sub PushALR(ByVal instIndex As Integer, ByVal sr1 As Integer, ByVal sr2 As Integer, ByVal dr As Integer)
        Dim inst As Integer = 0
        inst = Merge(inst, mask_25_21, sr1, 21)
        inst = Merge(inst, mask_20_16, sr2, 16)
        inst = Merge(inst, mask_15_11, dr, 11)
        inst = Merge(inst, mask_5_0, instIndex, 0)
        PushInst(inst)
    End Sub

End Class
