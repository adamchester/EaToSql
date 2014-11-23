namespace EaToSql

[<AutoOpen>]
module Model = 
    let private (|InvariantEqual|_|) (str:string) arg = 
      if System.String.Compare(str, arg, System.StringComparison.OrdinalIgnoreCase) = 0
        then Some() else None
    
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
            /// Allows creating a data type from the all the supported components.
            static member CreateT (name:SqlDataTypeName, isAutoNum:bool, length:int option, decimalScale:int option, decimalPrec:int option) =
                match name with
                | InvariantEqual "int" -> if isAutoNum then IntAuto else Int
                | InvariantEqual "decimal" -> Decimal(decimalPrec.Value, decimalScale.Value)
                | InvariantEqual "char" -> Char(length.Value)
                | InvariantEqual "varchar" -> VarChar(length.Value)
                | InvariantEqual "nvarchar" -> NVarChar(length.Value)
                | _ -> SQLT(name.ToLowerInvariant())
            /// Allows creating a data type from the all the supported components.
            static member Create (name:SqlDataTypeName,isAutoNum:bool, length:string option, decimalScale:string option, decimalPrec:string option) =
                let parseIntIfSome = function | Some s -> Some (System.Int32.Parse(s)) | None -> None
                DataType.CreateT (
                    name,isAutoNum,
                    (parseIntIfSome length),
                    (parseIntIfSome decimalScale),
                    (parseIntIfSome decimalPrec))

    /// A named object that references one or more column names (e.g. a named table index).
    type NamedColumnRefs = { Name: ModelName; Columns: ColumnRef list }

    /// A named database relationship with source and destination column names.
    type Relationship = { Name:ModelName; Source:NamedColumnRefs; Target:NamedColumnRefs }

    /// Describes a table column including the data type
    type ColumnDef = {
        Name: ModelName
        AllowsNull: bool
        DataType: DataType }

    /// Describes a table, including columns, PKs, IXs, FKs
    type Table = {
        Name: ModelName
        PrimaryKey: NamedColumnRefs
        Columns: ColumnDef list
        Indexes: NamedColumnRefs list
        Relationships: Relationship list }

