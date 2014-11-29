module EaToSql.Tests.ApiTests

open EaToSql
open NUnit.Framework

/// An empty table to use as a template
let tbl = { Table.Name = ""; Columns=[]; PrimaryKey={Name=""; Columns=[]}; Indexes=[]; Uniques=[]; Relationships=[] }

/// The expected schema after processing the sample model
let expectedSampleModelXmi2_1 = [
    { tbl with Name = "person"
               Columns = [col "person_id" IntAuto; col "first_nme" (VarChar 100); col "last_nme" (VarChar 100)]
               PrimaryKey = pk "PK_Person" ["person_id"]
               Indexes = [ix "ix_person_first" ["first_nme"]; ix "ix_person_last" ["last_nme"]; ix "ix_person_first_last" ["first_nme"; "last_nme"]] }
    { tbl with Name = "person_salary"
               Columns = [col "person_salary_id" IntAuto; col "person_id" Int
                          {(col "salary_amt" (Decimal(10, 2))) with AllowsNull = true}
                          {(col "contract_pdf" (SQLT "varbinary(max)")) with AllowsNull = true}]
               PrimaryKey = pk "PK_person_salary" ["person_salary_id"]
               Indexes = [ix "IXFK_person_salary_person" ["person_id"]]
               Relationships = [rel "FK_person_salary_person" ["person_id"] (target "person" ["person_id"])] }
    { tbl with Name = "person_skill"
               Columns = [col "person_skill_id" IntAuto; col "person_id" Int; col "skill_type_id" Int]
               PrimaryKey = pk "PK_person_skill" ["person_skill_id"]
               Indexes = [ix "IXFK_person_skill_person" ["person_id"]; ix "IXFK_person_skill_ref_skill_type" ["skill_type_id"]]
               Relationships = [rel "FK_person_skill_ref_skill_type" ["skill_type_id"] (target "ref_skill_type" ["skill_type_id"])
                                rel "FK_person_skill_person" ["person_id"] (target "person" ["person_id"])] }
    { tbl with Name = "person_tag"
               Columns = [col "person_id" Int; col "tag_type_id" Int]
               PrimaryKey = pk "PK_person_tag" ["person_id";"tag_type_id"]
               Indexes = [ix "IXFK_person_tag_person" ["person_id"]; ix "IXFK_person_tag_tag_type" ["tag_type_id"]]
               Relationships = [rel "FK_person_tag_person" ["person_id"] (target "person" ["person_id"])
                                rel "FK_person_tag_tag_type" ["tag_type_id"] (target "ref_tag_type" ["tag_type_id"])] }
    { tbl with Name = "ref_skill_type"
               Columns = [col "skill_type_id" IntAuto; col "skill_type_nme" (VarChar 100)]
               PrimaryKey = pk "PK_ref_skill_type" ["skill_type_id"]
               Uniques = [uq "UQ_ref_skill_type_skill_type_nme" ["skill_type_nme"]] }
    { tbl with Name = "tag_type"
               Columns = [col "tag_type_id" IntAuto; col "tag_type_nme" (VarChar 100)]
               PrimaryKey = pk "PK_tag_type" ["tag_type_id"]
               Uniques = [uq "UQ_tag_type_tag_type_nme" ["tag_type_nme"]] }
]

[<Test; Ignore>]
let ``generate the correct model from samples`` () =
    let sampleXmlFile = @"SampleModel_xmi2_1.xml"
    use xml = new System.IO.StreamReader(sampleXmlFile)
    let actual = readTablesFromXmi (xml) |> Seq.toList

    let expectedSql = Seq.toList (generateSqlFromModel expectedSampleModelXmi2_1)
    let actualSql = Seq.toList (generateSqlFromModel actual)

    Assert.AreEqual(expectedSql, actualSql)

[<Test>]
let ``generate the correct SQL from samples`` () =
    let actualSqlStatements =
        generateSqlFromModel
            [ { Table.Name = "t1"; Columns = [{ ColumnDef.Name = "id"; DataType = Int; AllowsNull = false; } ]
                PrimaryKey = { Name="t1_pk"; Columns = ["id"] }
                Indexes = [ { Name="t1_ix"; Columns = ["id"] } ]
                Uniques = [ { Name="t1_uq"; Columns = ["id"] } ]
                Relationships = [
                                { Relationship.Name = "r1"; SourceCols = ["id"]; Target = { Name="t2"; Columns=["??"] } }
                                { Relationship.Name = "r2"; SourceCols = ["anysrccol?"]; Target = { Name="anytarget?"; Columns=["??";"yep"] } }
                                ] }
              { Table.Name = "t2"; Columns = [ { ColumnDef.Name = "whatever1"; DataType = (NVarChar 999); AllowsNull = true; }
                                               { ColumnDef.Name = "whatever2"; DataType = (Decimal(99, 99)); AllowsNull = true; }
                                             ]
                PrimaryKey = { Name="pkt2"; Columns = ["whatever1";"whatever2"] }
                Indexes = [ { Name="ixt2"; Columns = ["whatever1";"whatever2"] } ]
                Uniques = [ { Name="uqt2"; Columns = ["whatever1";"whatever2"] } ]
                Relationships = [
                                { Relationship.Name = "t2r1"; SourceCols = ["id"]; Target = { Name="t2"; Columns=["??"] } }
                                { Relationship.Name = "t2r2"; SourceCols = ["anysrccol?"]; Target = { Name="anytarget?"; Columns=["??";"yep"] } }
                                ] }
            ]
        |> Seq.toList

    // For now, each (table+indexes) is a single string, each set of FKs (per table) is a single string
    let expectedSqlStatements = [
        // Expect two strings as output for now
        "CREATE TABLE [t1] (id int NOT NULL
  CONSTRAINT [t1_pk] PRIMARY KEY CLUSTERED (id))
  CREATE INDEX [t1_ix] ON [t1] (id)"

        "CREATE TABLE [t2] (whatever1 nvarchar(999) NULL, whatever2 decimal(99, 99) NULL
  CONSTRAINT [pkt2] PRIMARY KEY CLUSTERED (whatever1, whatever2))
  CREATE INDEX [ixt2] ON [t2] (whatever1, whatever2)"
        
        "ALTER TABLE [t1] ADD CONSTRAINT [r1] FOREIGN KEY (id) REFERENCES [t2] (??)
ALTER TABLE [t1] ADD CONSTRAINT [r2] FOREIGN KEY (anysrccol?) REFERENCES [anytarget?] (??, yep)"

        "ALTER TABLE [t2] ADD CONSTRAINT [t2r1] FOREIGN KEY (id) REFERENCES [t2] (??)
ALTER TABLE [t2] ADD CONSTRAINT [t2r2] FOREIGN KEY (anysrccol?) REFERENCES [anytarget?] (??, yep)"
    ]

    Assert.AreEqual(expectedSqlStatements, actualSqlStatements)
            