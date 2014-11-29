module EaToSql.SqlGen

open EaToSql

let columnRefsCsv (columns: ColumnRef list) = csv (columns |> List.toArray)

let dataTypeToSql (dataType: DataType) (allowsNull: bool) =
    let nullText = if allowsNull then "NULL" else "NOT NULL"
    match dataType with
    | SQLT "DATE" -> sprintf "datetime %s" nullText
    | SQLT "date" -> sprintf "datetime %s" nullText
    | SQLT "TIME" -> sprintf "smalldatetime %s" nullText
    | SQLT "time" -> sprintf "smalldatetime %s" nullText
    | SQLT name -> sprintf "[%s] %s" name nullText
    | IntAuto -> sprintf "int %s IDENTITY(1,1)" nullText
    | Int -> sprintf "int %s" nullText
    | Char len -> sprintf "char(%i) %s" len nullText
    | VarChar len -> sprintf "varchar(%i) %s" len nullText
    | NVarChar len -> sprintf "nvarchar(%i) %s" len nullText
    | Decimal (prec, scale) -> sprintf "decimal(%i, %i) %s" prec scale nullText

let columnsDefsToCsv (columns: ColumnDef list) =
    csv (columns
        |> Seq.map (fun c -> sprintf "%s %s" c.Name (dataTypeToSql c.DataType c.AllowsNull))
        |> Seq.toArray)

let getCreateTableIdxSql (t: Table) (index: NamedColumnRefs) =
    sprintf "CREATE INDEX [%s] ON [%s] (%s)" index.Name t.Name (columnRefsCsv index.Columns)

let getCreateTableAndIdxSql (t:Table) =
    let createIdxSqlLines =
        t.Indexes |> List.filter (fun idx -> idx.Columns.Length > 0) |> List.map (getCreateTableIdxSql t) |> Seq.toArray

    let createIdxSql = joinNewLines createIdxSqlLines
    sprintf "CREATE TABLE [%s] (%s
  CONSTRAINT [%s] PRIMARY KEY CLUSTERED (%s))
  %s"
            t.Name (columnsDefsToCsv t.Columns) t.PrimaryKey.Name (columnRefsCsv t.PrimaryKey.Columns) createIdxSql

let getCreateFkSql (t: Table) (rel: Relationship) =
    sprintf "ALTER TABLE [%s] ADD CONSTRAINT [%s] FOREIGN KEY (%s) REFERENCES [%s] (%s)"
            (t.Name) rel.Name (columnRefsCsv rel.SourceCols) rel.Target.Name (columnRefsCsv rel.Target.Columns)

let getCreateFksSql (t: Table) =
    t.Relationships
    |> groupByTakeFlatten (fun rel -> rel.Name) 1 // TODO: why dups?! for now, only take the first of each relationship name
    |> Seq.map (getCreateFkSql t)
    |> Seq.toArray
    |> joinNewLines


