# CMSqlTraceConverter

## The typical problem on optimizing MS SQL Server databases

SQL statements executed with ADO.NET are often converted by the .NET framework into dynamic SQL statements with 
```sql
EXEC sp_executesql N'INSERT INTO table (column1, column2) 
VALUES (@p1, @p2)',N'@p1 int, @p2 int',@p1 = 123',@p2 = 456'
```

MS SQL Server Profiles can create trace files (.trc) for all executed queries, but MS SQL Server Optimizer can't optimize for dynamic SQL statements.

## The solution

CMSqlTraceConverter does several steps at once
- convert MS SQL Server Trace files (.trc) into .sql files
- unboxing of all dynamic SQL statements "EXEC sp_executesql ..." into static SQL statements
```sql
EXEC sp_executesql N'INSERT INTO table (column1, column2) 
VALUES (@p1, @p2)',N'@p1 int, @p2 int',@p1 = 123',@p2 = 456'
```
will be converted to static SQL statement
```sql
DECLARE @p1 int, @p2 int
SELECT @p1 = 123',@p2 = 456'
INSERT INTO table (column1, column2) 
VALUES (@p1, @p2)'
```
- put all final SQL statements into a text file (.sql) and separate all SQL statements with the separator "GO"

## Usage

Please note: 
- a running SQL server instance with supervisor account is always required to read the .trc file
- the path to the .trc file must be accessible by the SQL server service

### Show syntax/Help
```cmd
CMSqlTraceConverter.exe --help
```

### Convert a .trc file into a .sql file 
```cmd
CMSqlTraceConverter.exe -t "C:\temp\profiled-db.trc" -c "SERVER=sqlserver\sqlexpress;DATABASE=tempdb;UID=sa;PWD=yourpassword;" 
```

### Convert a .trc file into a .sql file at a defined location
```cmd
CMSqlTraceConverter.exe -t "C:\temp\profiled-db.trc" -s "C:\temp\profiled-db.sql" -c "SERVER=sqlserver\sqlexpress;DATABASE=tempdb;UID=sa;PWD=yourpassword;" 
```

### Convert a .trc file into a .sql file and immediately open the .sql file  
```cmd
CMSqlTraceConverter.exe -t "C:\temp\profiled-db.trc" -o -c "SERVER=sqlserver\sqlexpress;DATABASE=tempdb;UID=sa;PWD=yourpassword;" 
```

## Now, you're ready for optimization
Now, you're ready to start your MS SQL Server Query Optimizer tool, select the new .sql file and execute the optimization on your database. You'll see that the optimization process will create much more suggestions to create missing indexes, statistics, etc.

## License
Released under the [GPL v3 license](https://raw.githubusercontent.com/CompuMasterGmbH/cammUtils-CMSqlTraceConverter/master/LICENSE)

## Honors
This library has been developed and maintained by [CompuMaster GmbH](http://www.compumaster.de/) as one useful tool of the [camm](http://www.camm.biz/) Utils collection.
