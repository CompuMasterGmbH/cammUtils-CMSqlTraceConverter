Option Explicit On
Option Strict On

Imports CommandLine

Module MainApp

    Sub Main(args As String())
        Dim result As CommandLine.ParserResult(Of TraceConverterOptions)
        result = CommandLine.Parser.Default.ParseArguments(Of TraceConverterOptions)(args)
        Dim exitCode As Integer = result.MapResult(AddressOf Start, AddressOf ParserErrors)
        System.Environment.Exit(exitCode)
    End Sub

    Private Function Start(options As TraceConverterOptions) As Integer
        Try
            TraceConverter.Execute(options)
            Return Nothing
        Catch ex As Exception
            Console.WriteLine("ERROR FOUND: " & vbNewLine & ex.ToString)
            Return 2
        End Try
    End Function

    Private Function ParserErrors(errors As IEnumerable(Of [Error])) As Integer
        Console.WriteLine("Please resolve errors above, first")
        Return 1
    End Function

End Module
