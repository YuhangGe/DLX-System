Imports DLXCompiler

''' <summary>
''' 函数表
''' </summary>
''' <remarks></remarks>
Partial Public Class LinkerCore
    Private Delegate Sub text_delegate()
    Private Const KEYWORDS_NUM As Integer = 35
    Dim f_table() As text_delegate = New text_delegate(34) _
    {
        AddressOf I_ADD, AddressOf I_ADDI, AddressOf I_SUB, AddressOf I_SUBI, _
        AddressOf I_AND, AddressOf I_ANDI, AddressOf I_OR, AddressOf I_ORI, _
        AddressOf I_XOR, AddressOf I_XORI, AddressOf I_SLL, AddressOf I_SLLI, _
        AddressOf I_SRL, AddressOf I_SRLI, AddressOf I_SRA, AddressOf I_SRAI, _
        AddressOf I_SLT, AddressOf I_SLTI, AddressOf I_SLE, AddressOf I_SLEI, _
        AddressOf I_SEQ, AddressOf I_SEQI, AddressOf I_SNE, AddressOf I_SNEI, _
        AddressOf I_LHI, _
        AddressOf I_LW, AddressOf I_SW, AddressOf I_BEQZ, AddressOf I_BNEZ, _
        AddressOf I_J, AddressOf I_JAL, AddressOf I_JR, AddressOf I_JALR, _
        AddressOf I_TRAP, AddressOf I_RFE}
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="src"></param>
    ''' <param name="mask"></param>
    ''' <param name="maskNum"></param>
    ''' <param name="offset"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function Merge(ByVal src As Integer, ByVal mask As Integer, ByVal maskNum As Integer, ByVal offset As Integer) As Integer
        src = src And (Not mask) ' 先把mask为1的位清零

        src = src Or (mask And (maskNum << offset)) '然后把maskNum加到src中

        Return src
    End Function
    Private Function GetSymbol(ByVal label As String) As Symbol
        '首先在局部表中查找符号
        Dim rtn As Symbol = cur_obj.dataTable.getSymbol(label)
        If rtn Is Nothing Then
            rtn = cur_obj.textTable.getSymbol(label)
        End If
        '如果该符号声明为extern，则在全局表中查找
        If rtn.IsExtern = True Then
            rtn = global_data_table.getSymbol(label)
            If rtn Is Nothing Then
                rtn = global_text_table.getSymbol(label)
            End If
        End If
        Return rtn
    End Function
    Private Sub I_ADD()
        Dim inst As Integer = 0 '32位指令
        Dim dr As Integer = Integer.Parse(GetNextWord)
        SkipSpace()
        Dim sr1 As Integer = Integer.Parse(GetNextWord())
        SkipSpace()
        Dim sr2 As Integer = Integer.Parse(GetNextWord())
#If Debug = True Then
        Console.Write(String.Format("add {0} {1} {2}", dr, sr1, sr2))
#End If
        PushALR(1, sr1, sr2, dr)
    End Sub
    Private Sub I_ADDI()
        Dim inst As Integer = 0 '32位指令
        Dim dr As Integer = Integer.Parse(GetNextWord)
        SkipSpace()
        Dim sr As Integer = Integer.Parse(GetNextWord())
        SkipSpace()

        If (cur_token = "#") Then
            GetNextToken()
            Dim imm As Integer = Integer.Parse(GetNextWord())
            PushALI(1, sr, dr, imm)
        Else
            Dim label As String = GetNextWord()
            SkipSpace()
            Dim offset As Integer = Integer.Parse(GetNextWord())
            Dim s As Symbol = GetSymbol(label)
            If s Is Nothing Then
                Error ("没有找到标记:" + label)
            End If
            If offset >= s.Value.Count Then
                Error ("错误的偏移")
            End If
            Dim imm As Integer = s.Value(offset)
            '转换为3条指令
            PushALI(12, 0, dr, (imm And &HFFFF0000) >> 16) 'LHI 指令，imm为高16位
            PushALI(1, dr, dr, imm And &HFFFF) 'ADDI 低16位
            PushALR(1, sr, dr, dr) 'ADD

        End If
    End Sub
    Private Function I_SUB() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_SUBI() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_AND() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_ANDI() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_OR() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_ORI() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_XOR() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_XORI() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_SLL() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_SLLI() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_SRL() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_SRLI() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_SRA() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_SRAI() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_SLT() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_SLTI() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_SLE() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_SLEI() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_SEQ() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_SEQI() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_SNE() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_SNEI() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_LHI() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_LW() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_SW() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_BEQZ() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_BNEZ() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_J() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_JAL() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_JR() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_JALR() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_TRAP() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function
    Private Function I_RFE() As Integer
        Dim inst As Integer = 0 '32位指令
        Return inst
    End Function






End Class
