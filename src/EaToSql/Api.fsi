[<AutoOpen>]
module EaToSql.Api

/// Parses the UML XMI 2.1 XML as input and returns the tables
/// described by the UML model.
val readTablesFromXmi : (System.IO.TextReader -> Table seq)

/// Takes a sequence of tables as input and generates the SQL statements
/// necessary to create those tables as on SQL Server.
val generateSqlFromModel : (Table seq -> string seq)
