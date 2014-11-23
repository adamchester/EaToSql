[<AutoOpen>]
module EaToSql.Api

let readTablesFromXmi reader = Reader.getDocForXmi(reader) |> Reader.getTablesForDoc
let generateSqlFromModel (tables: Table seq) = seq { yield "" }
