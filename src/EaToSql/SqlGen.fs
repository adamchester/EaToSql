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

let calculatePrefixedTableNameColNames prefix ownerName cols = sprintf "%s_%s_%s" prefix ownerName (stringJoin "_" cols)
let resolveAutoStrategy (owner: Table) (prefix: string) (nameOrStrategy: ModelNameOrAutoStrategy) (others: string list) =
    match nameOrStrategy with
    | Named nme -> nme
    | Auto PrefixedTableNameColNames -> calculatePrefixedTableNameColNames prefix owner.Name others

let getPkeyName      (owner: Table) (pk: PrimaryKey) = resolveAutoStrategy owner "pk" pk.Name pk.Columns
let getPkeyColumns   (owner: Table) (pk: PrimaryKey) = pk.Columns
let getRelName       (owner: Table) (rel: Relationship) = resolveAutoStrategy owner "fk" rel.Name [rel.Target.TableName]
let getRelTargetName (owner: Table) (rel: Relationship) = rel.Target.TableName
let getRelSrcColumns (owner: Table) (rel: Relationship) = rel.SourceCols
let getRelTgtColumns (owner: Table) (rel: Relationship) = rel.Target.Columns
let getIndexName     (owner: Table) (index: Index) = resolveAutoStrategy owner "ix" index.Name index.Columns
let getIndexColumns  (owner: Table) (index: Index) = index.Columns
let getUniqueName    (owner: Table) (unique: Unique) = resolveAutoStrategy owner "uq" unique.Name unique.Columns
let getUniqueColumns (owner: Table) (unique: Unique) = unique.Columns

let columnsDefsToCsv (columns: ColumnDef list) =
    csv (columns
        |> Seq.map (fun c -> sprintf "%s %s" c.Name (dataTypeToSql c.DataType c.Nullable))
        |> Seq.toArray)

let getCreateTableIdxSql (t: Table) (index: Index) =
    sprintf "CREATE INDEX [%s] ON [%s] (%s)" (getIndexName t index) t.Name (columnRefsCsv (getIndexColumns t index))

let getCreateTableAndIdxSql (t:Table) =
    let createIdxSqlLines =
        t.Indexes |> List.filter (fun idx -> (getIndexColumns t idx).Length > 0) |> List.map (getCreateTableIdxSql t) |> Seq.toArray

    let createIdxSql = joinNewLines createIdxSqlLines
    sprintf "CREATE TABLE [%s] (%s
    CONSTRAINT [%s] PRIMARY KEY CLUSTERED (%s))
  %s"
            t.Name (columnsDefsToCsv t.Columns) (getPkeyName t t.PrimaryKey) (columnRefsCsv (getPkeyColumns t t.PrimaryKey)) createIdxSql

let getCreateFkSql (t: Table) (rel: Relationship) =
    sprintf "ALTER TABLE [%s] ADD CONSTRAINT [%s] FOREIGN KEY (%s) REFERENCES [%s] (%s)"
            (t.Name) (getRelName t rel) (columnRefsCsv (getRelSrcColumns t rel)) (getRelTargetName t rel) (columnRefsCsv (getRelTgtColumns t rel))

let getCreateFksSql (t: Table) =
    t.Relationships
    |> groupByTakeFlatten (fun rel -> (getRelName t rel)) 1 // TODO: why dups?! for now, only take the first of each relationship name
    |> Seq.map (getCreateFkSql t)
    |> Seq.toArray
    |> joinNewLines


