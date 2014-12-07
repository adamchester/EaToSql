(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
EA to SQL
=========

This utility converts an Enterprise Architect (EA) XMI data model export into a SQL create script for SQL Server.

Usage
*)

(**
Create a data model in [Enterprise Architect](http://www.sparxsystems.com.au/products/ea/):
![EA Sample Model](img/ea_sample_model.png "A screenshot of Enterprise Architect Data Model diagram")

Export the data model to XMI 2.1:

![EA XMI 2.1 Export](img/ea_export_xmi21.png "A screenshot of Enterprise Architect Data Model Export XMI 2.1")
*)
#r "EaToSql.dll"
open EaToSql

// read in the tables from the XMI file
let binFolder filename = System.IO.Path.Combine(__SOURCE_DIRECTORY__, "../../bin", filename)
let tablesFromXmi =
    use reader = new System.IO.StreamReader(binFolder "SampleModel_xmi2_1.xml")
    readTablesFromXmi reader

(** Or just define your data model manually: *)
open EaToSql.Dsl

let tables = [
    { table "person" [ col "id" IntAuto
                       col "first" (NVarChar 100)
                       col "last" (NVarChar 100) ]
        with Indexes = [ ix ["first"]
                         ix ["last"]
                         ix ["first";"last"] ]
    }
    { table "ref_tag" [ col "id" IntAuto
                        col "tag_nme" (NVarChar 100) ]
        with Indexes = [ ix ["first"]
                         ix ["last"]
                         ix ["first"; "last"] ]
             Uniques = [ uq ["tag_nme"] ]
    }
    { table "person_tag" [ col "person_id" Int; col "tag_id" Int ]
        with PrimaryKey = pk ["person_id";"tag_id"]
             Indexes = [ ix ["person_id"] ]
             Uniques = [ uq ["person_id";"tag_id"] ]
             Relationships =
                [ rel ["person_id"] (target "person" ["person_id"]) ]}
]

(** Finally, generate the SQL create statements: *)
generateSqlFromModel tables |> Seq.toArray

(**
The output is:

    val it : string [] =
      [|"CREATE TABLE [person] (id int NOT NULL IDENTITY(1,1), first nvarchar(100) NOT NULL, last nvarchar(100) NOT NULL";
        "CONSTRAINT [pk_person_id] PRIMARY KEY CLUSTERED (id))";
        "CREATE INDEX [ix_person_first] ON [person] (first)";
        "CREATE INDEX [ix_person_last] ON [person] (last)";
        "CREATE INDEX [ix_person_first_last] ON [person] (first, last)";
        "CREATE TABLE [ref_tag] (id int NOT NULL IDENTITY(1,1), tag_nme nvarchar(100) NOT NULL";
        "CONSTRAINT [pk_ref_tag_id] PRIMARY KEY CLUSTERED (id))";
        "CREATE INDEX [ix_ref_tag_first] ON [ref_tag] (first)";
        "CREATE INDEX [ix_ref_tag_last] ON [ref_tag] (last)";
        "CREATE INDEX [ix_ref_tag_first_last] ON [ref_tag] (first, last)";
        "CREATE TABLE [person_tag] (person_id int NOT NULL, tag_id int NOT NULL";
        "CONSTRAINT [pk_person_tag_person_id_tag_id] PRIMARY KEY CLUSTERED (person_id, tag_id))";
        "CREATE INDEX [ix_person_tag_person_id] ON [person_tag] (person_id)";
        "ALTER TABLE [person_tag] ADD CONSTRAINT [fk_person_tag_person] FOREIGN KEY (person_id) REFERENCES [person] (person_id)"|]
*)
