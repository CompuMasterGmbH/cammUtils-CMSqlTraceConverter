﻿Option Explicit On
Option Strict On

Imports System.Text.RegularExpressions

Public Module TraceConverter

    Public Sub Execute(options As TraceConverterOptions)
        Dim MyCmd As New SqlClient.SqlCommand("", New SqlClient.SqlConnection(options.ConnectionString))
        MyCmd.CommandType = CommandType.Text
        MyCmd.CommandText = "SELECT CAST(TextData AS nvarchar(max))" & vbNewLine &
                            "FROM ::fn_trace_gettable('" & options.TrcFileServerPath & "', default)" & vbNewLine &
                            "WHERE IsNull(TextData,'') not like ''" & vbNewLine &
                            "    and textdata not like 'exec sp_reset_connection%'" & vbNewLine &
                            "GROUP BY CAST(TextData AS nvarchar(max))"

        Dim SqlStatements As List(Of String) = CompuMaster.Data.DataQuery.AnyIDataProvider.ExecuteReaderAndPutFirstColumnIntoGenericList(Of String)(MyCmd, CompuMaster.Data.DataQuery.AnyIDataProvider.Automations.AutoOpenAndCloseAndDisposeConnection)
        System.Console.WriteLine(SqlStatements.Count & " unique SQL statements found")
        Dim sb As New System.Text.StringBuilder()
        Dim MyRegEx As New System.Text.RegularExpressions.Regex("exec sp_executesql N'(?<sql>.*)',N'(?<declares>[^']*)'(?<params>,@(?<pname>.*)=(?<pvalue>.*))*", RegexOptions.Singleline Or RegexOptions.Compiled Or RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
        For MyCounter As Integer = 0 To SqlStatements.Count - 1
            Dim sql As String = IsNull(SqlStatements(MyCounter), "")
            If MyRegEx.IsMatch(sql) Then
                'unbox dynamic sql statement into a static one
                Console.WriteLine("DYNAMIC SQL FOUND:")
                Console.WriteLine(sql)
                Console.WriteLine()

                Dim m As Match = MyRegEx.Match(sql.Replace("''", ChrW(27)))
                Dim Declares As String = m.Groups("declares").Value
                Dim ParamSelects As New List(Of String)
                Dim MatchedParams As Group = m.Groups("params")
                For ParamCounter As Integer = 0 To MatchedParams.Captures.Count - 1
                    ParamSelects.Add(MatchedParams.Captures(ParamCounter).Value)
                Next
                Console.WriteLine("CONVERTED TO STATIC SQL:")
                sql = "DECLARE " & Declares & vbNewLine &
                    "SELECT " & Strings.Join(ParamSelects.ToArray, "").Substring(1) & vbNewLine &
                    m.Groups("sql").Value.Replace(ChrW(27), "'")
                Console.WriteLine(sql)
                Console.WriteLine()
                Console.WriteLine("------------------------------------------------------------------------------")
                Console.WriteLine()
            End If
            sb.AppendLine(sql)
            sb.AppendLine("GO")
            sb.AppendLine()
        Next
        Dim TempFile As String
        If options.SqlFileLocalPath = "" Then
            TempFile = System.IO.Path.GetTempFileName & ".txt"
        Else
            TempFile = options.SqlFileLocalPath
        End If
        System.Console.WriteLine("Writing SQL statements to " & TempFile)
        System.IO.File.WriteAllText(TempFile, sb.ToString)
        If options.AutoOpenSqlFile Then System.Diagnostics.Process.Start(TempFile)

    End Sub

    Private Function IsNull(value As System.Object, alternative As String) As String
        If Not IsDBNull(value) Then
            Return CType(value, String)
        Else
            Return alternative
        End If
    End Function

End Module