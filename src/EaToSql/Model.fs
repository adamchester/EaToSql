namespace EaToSql

[<AutoOpen>]
module Model = 
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
            override x.ToString() = sprintf "%A" x
            /// Allows creating a data type from the all the supported components.
            static member Create (name:SqlDataTypeName, isAutoNum: bool, length: int option, decimalPrec: int option, decimalScale: int option) =
                if name = null then nullArg "name"
                match name.ToLowerInvariant() with
                | "int" -> if isAutoNum = true then IntAuto else Int
                | "decimal" -> Decimal(decimalPrec.Value, decimalScale.Value)
                | "char" -> Char(length.Value)
                | "varchar" -> VarChar(length.Value)
                | "nvarchar" -> NVarChar(length.Value)
                | _ -> SQLT(name.ToLowerInvariant())

    /// A named object that references one or more column names (e.g. a named table index).
    type NamedColumnRefs = { Name: ModelName; Columns: ColumnRef list }
        with override x.ToString() = sprintf "%A" x

    /// A named database relationship with source and destination column names.
    type Relationship = { Name:ModelName; Source:NamedColumnRefs; Target:NamedColumnRefs }
        with override x.ToString() = sprintf "%A" x

    /// Describes a table column including the data type
    type ColumnDef = {
        Name: ModelName
        AllowsNull: bool
        DataType: DataType }
        with override x.ToString() = sprintf "%A" x

    /// Describes a table, including columns, PKs, IXs, FKs
    type Table = {
        Name: ModelName
        PrimaryKey: NamedColumnRefs
        Columns: ColumnDef list
        Indexes: NamedColumnRefs list
        Relationships: Relationship list }
        with override x.ToString() = sprintf "%A" x
