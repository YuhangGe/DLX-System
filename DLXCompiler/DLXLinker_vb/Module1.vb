Imports System.Xml
Imports DLXCompiler
Imports DLXLinker
Imports System.IO

Module Module1

    Sub Main()
        Dim doc As New XmlDocument
        doc.Load("../../../test1.xml")
        Dim obj As DLXCompiler.DLXObject = DLXCompiler.DLXObject.FromXML(doc)
        'obj.OutputXml(Console.Out)
        Dim l As New List(Of DLXObject)
        l.Add(obj)

        doc = New XmlDocument
        doc.Load("../../../test2.xml")
        obj = DLXCompiler.DLXObject.FromXML(doc)

        l.Add(obj)

        Dim linker As New LinkerCore()
        Dim out As Stream = New BufferedStream(New FileStream("../../../result.bin", FileMode.Create))

        Try
            linker.Link(l, out)
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
        out.Close()
        Console.ReadKey()
    End Sub

End Module
