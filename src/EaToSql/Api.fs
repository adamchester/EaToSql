[<AutoOpen>]
module EaToSql.Api

/// Parses the UML XMI 2.1 XML as input and returns the tables
/// described by the UML model.
let readTablesFromXmi reader = Reader.getDocForXmi(reader) |> Reader.getTablesForDoc

/// Takes the sequence of tables as input and generates the SQL statements
/// necessary to create the database containing those tables.
let generateSqlFromModel tables : string seq = seq {
    yield! tables |> Seq.collect SqlGen.getCreateTableAndIdxSql
    yield! tables |> Seq.collect SqlGen.getCreateFksSql
}
