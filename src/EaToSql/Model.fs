namespace EaToSql

/// Describes a relational data model: Tables, Columns, Indexes, Relationships, etc.
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

    /// Describes the available strategies for auto-naming objects
    type AutoNameStrategy =
        /// [prefix]_[table_name]_[first_col]... e.g. ix_table_name_first_col_second_col
        | PrefixedTableNameColNames
        with override x.ToString() = printObjectStructure x

    /// Describes either a direct (Named) object or an auto naming strategy
    type ModelNameOrAutoStrategy =
        | Named of ModelName
        | Auto of AutoNameStrategy
        with override x.ToString() = printObjectStructure x

    /// A named object that references one or more column names (e.g. a named table index or named PKey).
    type NamedColumnRefs = { Name: ModelNameOrAutoStrategy; Columns: ColumnRef list }
        with override x.ToString() = printObjectStructure x

    /// Describes the primary key of a table.
    type PrimaryKey = NamedColumnRefs

    /// Describes an index of a table; a name and columns.
    type Index = NamedColumnRefs

    /// Describes a unique constraint of a table; a name and columns.
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

/// Provides a slightly more succinct syntax for creating Model objects.
[<AutoOpen>]
module Dsl =
    /// Creates a ColumnDef record in a more succinct way.
    let col name dtype : ColumnDef = { Name = name; DataType = dtype; Nullable = false; }
    
    // AutoNamed
    let private auto = Auto(PrefixedTableNameColNames)
    /// creates a primary key with auto-naming, and the specified columns.
    let pk cols : PrimaryKey = { Name=auto; Columns=cols }
    /// creates an index with auto-naming, and the specified columns.
    let ix cols : Index = { Name=auto; Columns=cols }
    /// creates a unique constraint with auto-naming, and the specified columns.
    let uq cols : Unique = { Name=auto; Columns=cols }
    /// creates a relationship with auto-naming, and the specified source columns, and target table/columns.
    let rel srcCols target : Relationship = { Name=auto; SourceCols=srcCols; Target=target }

    /// Creates a named primary key.
    let pkn name cols : PrimaryKey = { Name=Named(name); Columns = cols }
    /// Creates a named index.
    let ixn name cols : Index = { Name=Named(name); Columns = cols }
    /// Creates a named unique constraint.
    let uqn name cols : Unique = { Name=Named(name); Columns = cols }
    /// Creates a named relationship, with source columns and a target table/columns.
    let reln name srcCols target : Relationship = { Name=Named(name); SourceCols = srcCols; Target = target }
    /// Creates a new RelTarget; the target table/columns of a relationship.
    let target tname cols : RelTarget = { TableName = tname; Columns = cols; }
