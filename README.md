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

## Now, you're ready for optimization
Now, you're ready to start your MS SQL Server Query Optimizer tool, select the new .sql file and execute the optimization on your database. You'll see that the optimization process will create much more suggestions to create missing indexes, statistics, etc.

## License
Released under the GPL v3 license
