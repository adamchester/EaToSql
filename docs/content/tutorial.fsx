(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
EA to SQL
=========

This utility converts an Enterprise Architect (EA) XMI data model export
into a SQL create script for SQL Server.

*)
#r "EaToSql.dll"
open EaToSql

let tables = [
    { Name = "person"
      Columns = [ { Name = "id"; DataType = IntAuto; AllowsNull = false }
                  { Name = "first"; DataType = SQLT "varchar(100)"; AllowsNull = false }
                  { Name = "last"; DataType = SQLT "varchar(100)"; AllowsNull = false } ]
      PrimaryKey = { Name="pk_person"; Columns = [ "id" ] }
      Indexes = [ { Name = "ix_person_first"; Columns = [ "first" ] }
                  { Name = "ix_person_last"; Columns = [ "last" ] }
                  { Name = "ix_person_first_last"; Columns = [ "first"; "last" ] } ]
      Relationships = [] }

    { Name = "person"
      Columns = [ { Name = "id"; DataType = IntAuto; AllowsNull = false }
                  { Name = "first"; DataType = SQLT "varchar(100)"; AllowsNull = false }
                  { Name = "last"; DataType = SQLT "varchar(100)"; AllowsNull = false } ]
      PrimaryKey = { Name="pk_person"; Columns = [ "id" ] }
      Indexes = [ { Name = "ix_person_first"; Columns = [ "first" ] }
                  { Name = "ix_person_last"; Columns = [ "last" ] }
                  { Name = "ix_person_first_last"; Columns = [ "first"; "last" ] } ]
      Relationships = [] }
]

(**
Some more info
*)
