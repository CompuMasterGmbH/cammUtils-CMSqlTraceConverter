Option Explicit On
Option Strict On

Imports System.Text.RegularExpressions

Public Module TraceConverter

    Public Sub Execute(options As TraceConverterOptions)
        'Read all SQL statements from the trace file via the SQL server trace file filter engine
        Dim MyCmd As New SqlClient.SqlCommand("", New SqlClient.SqlConnection(options.ConnectionString))
        MyCmd.CommandType = CommandType.Text
        MyCmd.CommandText = "SELECT CAST(TextData AS nvarchar(max))" & vbNewLine &
                            "FROM ::fn_trace_gettable('" & options.TrcFileServerPath & "', default)" & vbNewLine &
                            "WHERE IsNull(TextData,'') not like ''" & vbNewLine &
                            "    and textdata not like 'exec sp_reset_connection%'" & vbNewLine &
                            "GROUP BY CAST(TextData AS nvarchar(max))"
        Dim SqlStatements As List(Of String) = CompuMaster.Data.DataQuery.AnyIDataProvider.ExecuteReaderAndPutFirstColumnIntoGenericList(Of String)(MyCmd, CompuMaster.Data.DataQuery.AnyIDataProvider.Automations.AutoOpenAndCloseAndDisposeConnection)
        System.Console.WriteLine(SqlStatements.Count & " unique SQL statements found")
        'Unbox all dynamic SQLs into static ones if requested by commandline parameters
        If options.UnboxSpExecuteSql Then
            Dim MyRegEx As New System.Text.RegularExpressions.Regex("exec sp_executesql N'(?<sql>.*)',N'(?<declares>[^']*)'(?<params>,@(?<pname>.*)=(?<pvalue>.*))*", RegexOptions.Singleline Or RegexOptions.Compiled Or RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
            For MyCounter As Integer = 0 To SqlStatements.Count - 1
                Dim sql As String = IsNull(SqlStatements(MyCounter), "")
                If MyRegEx.IsMatch(sql) Then
                    'unbox dynamic sql statement into a static one
                    If options.VerboseMode Then
                        Console.WriteLine("DYNAMIC SQL FOUND:")
                        Console.WriteLine(sql)
                        Console.WriteLine()
                    End If
                    'Identify inner SQL command, parameter definition, parameter assignment
                    Dim m As Match = MyRegEx.Match(sql.Replace("''", ChrW(27))) 'escape doubled quotation mark in string to prevent wrong end-of-string-detection by reg-expression
                    Dim Declares As String = m.Groups("declares").Value
                    Dim ParamSelects As New List(Of String)
                    Dim MatchedParams As Group = m.Groups("params")
                    For ParamCounter As Integer = 0 To MatchedParams.Captures.Count - 1
                        ParamSelects.Add(MatchedParams.Captures(ParamCounter).Value)
                    Next
                    '[Microsoft][ODBC Driver 11 for SQL Server][SQL Server]Die Datentypen 'text', 'ntext' und 'image' sind für lokale Variablen ungültig. 
                    Declares = Replace(Declares, " text", " varchar(max) --ex: text" & vbNewLine)
                    Declares = Replace(Declares, " ntext", " nvarchar(max) --ex: ntext" & vbNewLine)
                    sql = "DECLARE " & Declares & vbNewLine &
                    "SELECT " & Strings.Join(ParamSelects.ToArray, "").Substring(1) & vbNewLine &
                    m.Groups("sql").Value.Replace(ChrW(27), "'") 'change back all previously escaped doubled quotation mark into single quotation mark (single instead of doubled ones because the SQL statement is now a regular static SQL command instead of a string-escaped one for the exec sp_executecommand call
                    If options.VerboseMode Then
                        Console.WriteLine("CONVERTED TO STATIC SQL:")
                        Console.WriteLine(sql)
                        Console.WriteLine()
                        Console.WriteLine("------------------------------------------------------------------------------")
                        Console.WriteLine()
                    End If
                    SqlStatements(MyCounter) = sql
                End If
            Next
        End If
        'Watch out for the output file
        Dim TempFile As String
        If options.SqlFileLocalPath = "" Then
            TempFile = System.IO.Path.GetTempFileName & ".sql"
        Else
            TempFile = System.IO.Path.Combine(System.Environment.CurrentDirectory, options.SqlFileLocalPath)
        End If
        System.Console.WriteLine("Writing SQL statements to " & TempFile)
        'Export into the appropriate output format
        Select Case System.IO.Path.GetExtension(TempFile.ToLowerInvariant)
            Case ".csv"
                Dim t As DataTable = ConvertToDataTable(SqlStatements)
                CompuMaster.Data.Csv.WriteDataTableToCsvFile(TempFile, t, False, System.Globalization.CultureInfo.InstalledUICulture)
            Case ".xlsx"
                Dim t As DataTable = ConvertToDataTable(SqlStatements)
                CompuMaster.Data.XlsEpplus.WriteDataTableToXlsFile(TempFile, t)
            Case Else
                Dim sb As New System.Text.StringBuilder()
                For MyCounter As Integer = 0 To SqlStatements.Count - 1
                    Dim sql As String = SqlStatements(MyCounter)
                    sb.AppendLine(sql)
                    sb.AppendLine("GO")
                    sb.AppendLine()
                Next
                System.IO.File.WriteAllText(TempFile, sb.ToString, System.Text.Encoding.Default)
        End Select
        'Automatically open the exported file if requested by commandline parameters
        If options.AutoOpenSqlFile Then System.Diagnostics.Process.Start(TempFile)

    End Sub

    Private Function ConvertToDataTable(list As List(Of String)) As DataTable
        Dim t As New DataTable("SQL Statements")
        t.Columns.Add("SQL")
        For MyCounter As Integer = 0 To list.Count - 1
            Dim sql As String = list(MyCounter)
            Dim row As System.Data.DataRow = t.NewRow
            row(0) = sql
            t.Rows.Add(row)
        Next
        Return t
    End Function

    Private Function IsNull(value As System.Object, alternative As String) As String
        If Not IsDBNull(value) Then
            Return CType(value, String)
        Else
            Return alternative
        End If
    End Function

End Module
