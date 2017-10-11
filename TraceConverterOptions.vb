Option Strict On
Option Explicit On

Imports CommandLine

<Assembly: CommandLine.Text.AssemblyUsage("CMSqlTraceConverter.exe -t {tracefile} -c {connection string} [-s {outputfile}] [-o] [-v] [-u false]")>
'<Assembly: CommandLine.Text.AssemblyUsage("CMSqlTraceConverter.exe -t {tracefile} -c {connection string} [-s {outputfile}] [-o] [-v] [-u false]",
'                                          "CMSqlTraceConverter.exe -t ""C:\temp\profiled-db.trc"" -s ""C:\temp\profiled-db.sql"" -c ""SERVER=sqlserver\sqlexpress;DATABASE=tempdb;UID=sa;PWD=yourpassword;""",
'                                          "CMSqlTraceConverter.exe -t ""C:\temp\profiled-db.trc"" -s ""C:\temp\profiled-db.sql"" -c ""SERVER=sqlserver\sqlexpress;DATABASE=tempdb;Trusted_Connection=True;""")>
'<Assembly: CommandLine.Text.MultilineText("line1", "line2")>

Public Class TraceConverterOptions

    <[Option]("c"c, "ConnectionString", Required:=True, HelpText:="A connection string to the MS SQL Server with supervisor privileges (e.g. User ""sa"")")>
    Public Property ConnectionString As String

    <[Option]("t"c, "TraceFile", Required:=True, HelpText:="The path at the SQL server to the .trc file which shall be read (the file must be accessible by your SQL server)")>
    Public Property TrcFileServerPath As String

    <[Option]("s"c, "SqlFile", Required:=False, HelpText:="The local path to the .sql file which shall be created/written (the file will contain all SQL statements)")>
    Public Property SqlFileLocalPath As String

    <[Option]("u"c, "UnboxSpExecuteSql", [Default]:=True, Required:=False, HelpText:="Convert all usage of ""EXEC sp_executesql ..."" into non-dynamic SQL statements (required for SQL optimizer to analyze and optimize your queries)")>
    Public Property UnboxSpExecuteSql As Boolean

    <[Option]("v"c, "Verbose", Required:=False, HelpText:="Enable verbose mode")>
    Public Property VerboseMode As Boolean

    Private _AutoOpenSqlFile As Boolean
    <[Option]("o"c, "AutoOpenSqlFile", Required:=False, HelpText:="Open the local .sql file after is has been saved")>
    Public Property AutoOpenSqlFile As Boolean
        Get
            Return _AutoOpenSqlFile OrElse Me.SqlFileLocalPath = Nothing
        End Get
        Set(value As Boolean)
            _AutoOpenSqlFile = value
        End Set
    End Property

    '<CommandLine.Text.Usage()>
    'Public Property UsageText() As String
    '    Get
    '        Return "CMSqlTraceConverter.exe -t ""C:\temp\profiled-db.trc"" -s ""C:\temp\profiled-db.sql"" -c ""SERVER=sqlserver\sqlexpress;DATABASE=tempdb;UID=sa;PWD=yourpassword;"""
    '    End Get
    '    Set(value As String)

    '    End Set
    'End Property

    '<CommandLine.Text.MultilineText("line1", "line2")>
    'Public Property UsageSample As String
    '    Get

    '    End Get
    '    Set(value As String)

    '    End Set
    'End Property

End Class
