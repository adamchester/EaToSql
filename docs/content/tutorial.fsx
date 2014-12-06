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
open EaToSql.Dsl

// Define a model
let tables = [
    { Name = "person"
      Columns = [ col "id" IntAuto; col "first" (NVarChar(100)); col "last" (NVarChar(100)) ]
      PrimaryKey = pk ["id"]
      Indexes = [ ix ["first"]; ix ["last"]; ix ["first";"last"] ]
      Uniques = []; Relationships = []; }
    { Name = "ref_tag"
      Columns = [ col "id" IntAuto; col "tag_nme" (NVarChar(100)); ]
      PrimaryKey = pk [ "id" ]
      Indexes = [ ix ["first"]; ix ["last"]; ix ["first"; "last"]; ]
      Uniques = [ uq ["tag_nme"] ]
      Relationships = [] }
    { Name = "person_tag"
      Columns = [ col "person_id" Int; col "tag_id" Int ]
      PrimaryKey = pk ["person_id";"tag_id"]
      Indexes = [ ix ["person_id"] ]
      Uniques = [ uq ["person_id";"tag_id"] ]
      Relationships = [ rel ["person_id"] (target "person" ["person_id"]) ]}
]

// Generate the SQL create statements
let sql = generateSqlFromModel tables |> Seq.toArray

(**
The output is:

    val sql : string [] =
      [|"CREATE TABLE [person] (id int NOT NULL IDENTITY(1,1), first n"+[50 chars];
        "CONSTRAINT [pk_person_id] PRIMARY KEY CLUSTERED (id))";
        "CREATE INDEX [ix_person_first] ON [person] (first)";
        "CREATE INDEX [ix_person_last] ON [person] (last)";
        "CREATE INDEX [ix_person_first_last] ON [person] (first, last)";
        "CREATE TABLE [ref_tag] (id int NOT NULL IDENTITY(1,1), tag_nm"+[24 chars];
        "CONSTRAINT [pk_ref_tag_id] PRIMARY KEY CLUSTERED (id))";
        "CREATE INDEX [ix_ref_tag_first] ON [ref_tag] (first)";
        "CREATE INDEX [ix_ref_tag_last] ON [ref_tag] (last)";
        "CREATE INDEX [ix_ref_tag_first_last] ON [ref_tag] (first, last)";
        "CREATE TABLE [person_tag] (person_id int NOT NULL, tag_id int NOT NULL";
        "CONSTRAINT [pk_person_tag_person_id_tag_id] PRIMARY KEY CLUST"+[25 chars];
        "CREATE INDEX [ix_person_tag_person_id] ON [person_tag] (person_id)";
        "ALTER TABLE [person_tag] ADD CONSTRAINT [fk_person_tag_person"+[57 chars]|]
*)
