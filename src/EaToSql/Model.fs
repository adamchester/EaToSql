namespace EaToSql

module Model = 
    let private (|InvariantEqual|_|) (str:string) arg = 
      if System.String.Compare(str, arg, System.StringComparison.OrdinalIgnoreCase) = 0
        then Some() else None
    
    type ModelName = string
    type SqlDataTypeName = string

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
            static member CreateT (name:SqlDataTypeName,isAutoNum:bool,columnLength:int option,decimalScale:int option,decimalPrec:int option) =
                match name with
                | InvariantEqual "int" -> if isAutoNum then IntAuto else Int
                | InvariantEqual "decimal" -> Decimal(decimalPrec.Value, decimalScale.Value)
                | InvariantEqual "char" -> Char(columnLength.Value)
                | InvariantEqual "varchar" -> VarChar(columnLength.Value)
                | InvariantEqual "nvarchar" -> NVarChar(columnLength.Value)
                | _ -> SQLT(name.ToLowerInvariant())
            static member Create (name:SqlDataTypeName,isAutoNum:bool,columnLength:string option,decimalScale:string option,decimalPrec:string option) =
                let parseIntIfSome = function | Some s -> Some (System.Int32.Parse(s)) | None -> None
                DataType.CreateT(name,isAutoNum,(parseIntIfSome columnLength),(parseIntIfSome decimalScale),(parseIntIfSome decimalPrec))

    /// A 'reference' to a column by it's unique name.
    type ColumnRef = ModelName

    /// A named object that references one or more column names (e.g. a named table index).
    type NamedColumnsRef = { Name: ModelName; Columns: ColumnRef list }

    /// A named database relationship with source and destination column names.
    type Relationship = { Name:ModelName; Source:NamedColumnsRef; Target:NamedColumnsRef }

    /// Describes a table column including the data type
    type ColumnDef = {
        Name: ModelName
        AllowsNull: bool
        DataType: DataType }

    /// Describes a table, including columns, PKs, IXs, FKs
    type Table = {
        Name: ModelName
        PrimaryKey: NamedColumnsRef
        Columns: ColumnDef list
        Indexes: NamedColumnsRef list
        Relationships: Relationship list }

