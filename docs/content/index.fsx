(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
EA to SQL
=========

Documentation

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      This project has no decent doco.
    </div>
  </div>
  <div class="span1"></div>
</div>

Example
-------

This example demonstrates using a function defined in this sample library.
*)
#r "EaToSql.dll"
open EaToSql

let model = 
    [ table "t1" [ col "id" IntAuto ]
      { table "t2" [ col "t2id" IntAuto ]
            with Indexes = [ ix [ "id" ] ]
                 Relationships = [ rel ["id"] (target "t1" ["id"]) ]}
      { table "t3" [ col "first" (NVarChar 100)
                     col "last" (NVarChar 100) ] with Indexes = 
                                                          [ ix [ "first" ]
                                                            ix [ "last" ]
                                                            ix [ "first"; "last" ] ] } ]

model |> generateSqlFromModel |> Seq.toArray

(**
The output is:
    val it : string [] =
      [|"CREATE TABLE [t1] (id int NOT NULL IDENTITY(1,1)";
        "CONSTRAINT [pk_t1_id] PRIMARY KEY CLUSTERED (id))";
        "CREATE TABLE [t2] (t2id int NOT NULL IDENTITY(1,1)";
        "CONSTRAINT [pk_t2_t2id] PRIMARY KEY CLUSTERED (t2id))";
        "CREATE INDEX [ix_t2_id] ON [t2] (id)";
        "CREATE TABLE [t3] (first nvarchar(100) NOT NULL, last nvarchar(100) NOT NULL";
        "CONSTRAINT [pk_t3_first] PRIMARY KEY CLUSTERED (first))";
        "CREATE INDEX [ix_t3_first] ON [t3] (first)";
        "CREATE INDEX [ix_t3_last] ON [t3] (last)";
        "CREATE INDEX [ix_t3_first_last] ON [t3] (first, last)";
        "ALTER TABLE [t2] ADD CONSTRAINT [fk_t2_t1] FOREIGN KEY (id) REFERENCES [t1] (id)"|]

*)

(**
Samples & documentation
-----------------------

The library comes with comprehensible documentation. 
It can include tutorials automatically generated from `*.fsx` files in [the content folder][content]. 
The API reference is automatically generated from Markdown comments in the library implementation.

 * [Tutorial](tutorial.html) contains a further explanation of this sample library.

 * [API Reference](reference/index.html) contains automatically generated documentation for all types, modules
   and functions in the library. This includes additional brief samples on using most of the
   functions.
 
Contributing and copyright
--------------------------

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork 
the project and submit pull requests. If you're adding a new public API, please also 
consider adding [samples][content] that can be turned into a documentation. You might
also want to read the [library design notes][readme] to understand how it works.

The library is available under Public Domain license, which allows modification and 
redistribution for both commercial and non-commercial purposes. For more information see the 
[License file][license] in the GitHub repository. 

  [content]: https://github.com/adamchester/EaToSql/tree/master/docs/content
  [gh]: https://github.com/adamchester/EaToSql
  [issues]: https://github.com/adamchester/EaToSql/issues
  [readme]: https://github.com/adamchester/EaToSql/blob/master/README.md
  [license]: https://github.com/adamchester/EaToSql/blob/master/LICENSE.txt
*)
