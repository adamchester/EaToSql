[<AutoOpen>]
module EaToSql.Api

/// Parses the UML XMI 2.1 XML as input and returns the tables
/// described by the UML model.
let readTablesFromXmi reader = Reader.getDocForXmi(reader) |> Reader.getTablesForDoc

/// Takes the UML XMI 2.1 XML as input and generates the SQL statements
/// necessary to create the database.
let generateSqlFromModel tables = seq {
    yield! tables |> Seq.map SqlGen.getCreateTableAndIdxSql
    yield! tables |> Seq.map SqlGen.getCreateFksSql
}
