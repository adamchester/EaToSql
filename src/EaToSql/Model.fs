namespace EaToSql

[<AutoOpen>]
module Model =
    let inline internal printObjectStructure x = sprintf "%A" x
     
    /// The name of a data model object (e.g. Table, Column, Index,  etc).
    type ModelName = string

    /// A SQL type name (e.g. 'varchar(max)' or 'datetime').
    type SqlDataTypeName = string

    /// A 'reference' to a column by it's unique name.
    type ColumnRef = ModelName

    /// The data types defined in the model, especially those that require information in addition
    /// to just their type name (e.g. decimal has a scale, char has a length, 'int' may be
    /// an auto-number).
    type DataType =
        | Int | IntAuto
        | Decimal of precision:int * scale:int
        | Char of length:int
        | VarChar of length:int
        | NVarChar of length:int
        | SQLT of name:SqlDataTypeName
        with
            override x.ToString() = printObjectStructure x
            /// Allows creating a data type from the all the supported components.
            static member Create (name:SqlDataTypeName, isAutoNum: bool, length: int option, decimalPrec: int option, decimalScale: int option) =
                if name = null then nullArg "name"
                let lowerName = name.ToLowerInvariant()
                match lowerName with
                | "int" -> if isAutoNum = true then IntAuto else Int
                | "decimal" -> Decimal(decimalPrec.Value, decimalScale.Value)
                | "char" -> Char(length.Value)
                | "varchar" -> VarChar(length.Value)
                | "nvarchar" -> NVarChar(length.Value)
                | _ -> SQLT(lowerName)

    type AutoNameStrategy =
        /// [prefix]_[table_name]_[first_col]... e.g. ix_table_name_first_col_second_col
        | PrefixedTableNameColNames
        with override x.ToString() = printObjectStructure x

    type ModelNameOrAutoStrategy =
        | Named of ModelName
        | Auto of AutoNameStrategy
        with override x.ToString() = printObjectStructure x

    /// A named object that references one or more column names (e.g. a named table index or named PKey).
    type NamedColumnRefs = { Name: ModelNameOrAutoStrategy; Columns: ColumnRef list }
        with override x.ToString() = printObjectStructure x
    
    type PrimaryKey = NamedColumnRefs
    type Index = NamedColumnRefs
    type Unique = NamedColumnRefs
    
    /// The target table name and column(s) of a table relationship.
    type RelTarget = { TableName: ModelName; Columns: ColumnRef list }
        with override x.ToString() = printObjectStructure x

    /// A named database relationship with source and destination column names.
    type Relationship = { Name: ModelNameOrAutoStrategy; SourceCols: ColumnRef list; Target: RelTarget }
        with override x.ToString() = printObjectStructure x

    /// Describes a table column including the data type
    type ColumnDef = {
        Name: ModelName
        Nullable: bool
        DataType: DataType }
        with override x.ToString() = printObjectStructure x

    /// Describes a table, including columns, PKs, IXs, FKs
    type Table = {
        Name: ModelName
        PrimaryKey: PrimaryKey
        Columns: ColumnDef list
        Indexes: Index list
        Relationships: Relationship list
        Uniques: Unique list }
        with override x.ToString() = printObjectStructure x

[<AutoOpen>]
module Dsl =
    let col name dtype : ColumnDef = { Name = name; DataType = dtype; Nullable = false; }
    
    // AutoNamed
    let private auto = Auto(PrefixedTableNameColNames)
    let pk cols : PrimaryKey = { PrimaryKey.Name=auto; Columns=cols }
    let ix cols : Index = { Index.Name=auto; Columns=cols }
    let uq cols : Unique = { Unique.Name=auto; Columns=cols }
    let rel srcCols target : Relationship = { Relationship.Name=auto; SourceCols=srcCols; Target=target }

    // Named
    let pkn name cols : PrimaryKey = { Name=Named(name); Columns = cols }
    let ixn name cols : Index = { Name=Named(name); Columns = cols }
    let uqn name cols : Unique = { Name=Named(name); Columns = cols }
    let reln name srcCols target : Relationship = { Name=Named(name); SourceCols = srcCols; Target = target }

    let target tname cols : RelTarget = { TableName = tname; Columns = cols; }
