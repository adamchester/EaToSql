module EaToSql.Tests.ApiTests

open EaToSql
open Xunit

/// The expected schema after processing the sample models.
/// This must be kept exactly in sync with the EA model.
let expectedSampleModel = [
    { table "person" [ col "person_id" IntAuto
                       col "first_nme" (VarChar 100)
                       col "last_nme" (VarChar 100) ]
        with PrimaryKey = pkn "PK_Person" ["person_id"]
             Indexes = [ ixn "ix_person_first" ["first_nme"]
                         ixn "ix_person_last" ["last_nme"]
                         ixn "ix_person_first_last" ["first_nme"; "last_nme"] ] }
    { table "person_salary" [ col "person_salary_id" IntAuto; col "person_id" Int
                              {(col "salary_amt" (Decimal(10, 2))) with Nullable = true}
                              {(col "contract_pdf" (SQLT "varbinary(max)")) with Nullable = true}]
        with PrimaryKey = pkn "PK_person_salary" ["person_salary_id"]
             Indexes = [ ixn "IXFK_person_salary_person" ["person_id"]]
             Relationships = [ reln "FK_person_salary_person" ["person_id"] (target "person" ["person_id"]) ]
             Uniques = [ uqn "UQ_person_salary_person_id" ["person_id"; "salary_amt"] ] }
    { table "person_skill" [ col "person_skill_id" IntAuto
                             col "person_id" Int
                             col "skill_type_id" Int ]
        with PrimaryKey = pkn "PK_person_skill" ["person_skill_id"]
             Indexes = [ ixn "IXFK_person_skill_person" ["person_id"]
                         ixn "IXFK_person_skill_ref_skill_type" ["skill_type_id"]]
             Relationships = [ reln "FK_person_skill_ref_skill_type" ["skill_type_id"] (target "ref_skill_type" ["skill_type_id"])
                               reln "FK_person_skill_person" ["person_id"] (target "person" ["person_id"]) ] }
    { table "person_tag" [ col "person_id" Int; col "tag_type_id" Int]
        with PrimaryKey = pkn "PK_person_tag" ["person_id";"tag_type_id"]
             Indexes = [ ixn "IXFK_person_tag_person" ["person_id"]
                         ixn "IXFK_person_tag_tag_type" ["tag_type_id"] ]
             Relationships = [ reln "FK_person_tag_tag_type" ["tag_type_id"] (target "tag_type" ["tag_type_id"])
                               reln "FK_person_tag_person" ["person_id"] (target "person" ["person_id"]) ] }
    { table "ref_skill_type" [ col "skill_type_id" IntAuto
                               col "skill_type_nme" (VarChar 100) ]
        with PrimaryKey = pkn "PK_ref_skill_type" ["skill_type_id"]
             Uniques = [ uqn "UQ_ref_skill_type_skill_type_nme" ["skill_type_nme"] ] }
    {  table "tag_type" [ col "tag_type_id" IntAuto; col "tag_type_nme" (VarChar 100)]
        with PrimaryKey = pkn "PK_tag_type" ["tag_type_id"]
             Uniques = [ uqn "UQ_tag_type_tag_type_nme" ["tag_type_nme"] ] }
]

[<Fact>]
let ``generate auto-named objects correctly`` () =
    let table = { Table.Name = "t1"
                  Columns = [col "c1" IntAuto; col "c2" (NVarChar(100)); ]
                  PrimaryKey = pk ["c1"]
                  Indexes = [ix ["c1"]; ix ["c1";"c2"]]
                  Uniques = [uq ["c1"]]
                  Relationships = [rel ["c1"] (target "t2" ["t2c1"])] }

    let generatedSql = (generateSqlFromModel [table]) |> Seq.toArray
    let expectedSql =
        [| "CREATE TABLE [t1] (c1 int NOT NULL IDENTITY(1,1), c2 nvarchar(100) NOT NULL"
           "CONSTRAINT [pk_t1_c1] PRIMARY KEY CLUSTERED (c1))"
           "CREATE INDEX [ix_t1_c1] ON [t1] (c1)"
           "CREATE INDEX [ix_t1_c1_c2] ON [t1] (c1, c2)"
           "ALTER TABLE [t1] ADD CONSTRAINT [fk_t1_t2] FOREIGN KEY (c1) REFERENCES [t2] (t2c1)" |]

    Assert.Equal<string seq>(expectedSql, generatedSql)

[<Fact>]
let ``generate the correct model from samples`` () =
    let sampleXmlFile = @"SampleModel_xmi2_1.xml"
    use xml = new System.IO.StreamReader(sampleXmlFile)
    let actual = readTablesFromXmi (xml) |> Seq.toList
    let expectedSql = (generateSqlFromModel expectedSampleModel) |> Seq.toList
    let actualSql = Seq.toList (generateSqlFromModel actual)
    // printfn "EXPECTED: %A\nACTUAL: %A" expectedSql actualSql
    Assert.Equal<string seq>(expectedSql, actualSql)
    Assert.Equal<Table seq>(expectedSampleModel, actual)

[<Fact>]
let ``generate the correct SQL using the named objects`` () =
    let actualSqlStatements =
        generateSqlFromModel
            [ { Table.Name = "t1"; Columns = [col "id" Int]; PrimaryKey = pkn "t1_pk" ["id"]; 
                Indexes = [ ixn "t1_ix" ["id"] ]; Uniques = [ uqn "t1_uq" ["id"] ]
                Relationships = [ reln "r1" ["id"] (target "t2" ["??"])
                                  reln "r2" ["anysrccol?"] (target "anytarget?" ["??";"yep"]) ]}
              { Table.Name = "t2"; Columns = [ { (col "whatever1" (NVarChar 999)) with Nullable = true; }
                                               { (col "whatever2" (Decimal(99, 99))) with Nullable = true; }]
                PrimaryKey = pkn "pkt2" ["whatever1";"whatever2"]
                Indexes = [ ixn "ixt2" ["whatever1";"whatever2"] ]
                Uniques = [ uqn "uqt2" ["whatever1";"whatever2"] ]
                Relationships = [ reln "t2r1" ["id"] (target "t2" ["??"])
                                  reln "t2r2" ["anysrccol?"] (target "anytarget?" ["??";"yep"]) ]} ]
        |> Seq.toList

    let expectedSqlStatements =
        [ "CREATE TABLE [t1] (id int NOT NULL"
          "CONSTRAINT [t1_pk] PRIMARY KEY CLUSTERED (id))"
          "CREATE INDEX [t1_ix] ON [t1] (id)"
          "CREATE TABLE [t2] (whatever1 nvarchar(999) NULL, whatever2 decimal(99, 99) NULL"
          "CONSTRAINT [pkt2] PRIMARY KEY CLUSTERED (whatever1, whatever2))"
          "CREATE INDEX [ixt2] ON [t2] (whatever1, whatever2)"
          // note we put fk constraints at the end so tables are created before they're referenced
          "ALTER TABLE [t1] ADD CONSTRAINT [r1] FOREIGN KEY (id) REFERENCES [t2] (??)"
          "ALTER TABLE [t1] ADD CONSTRAINT [r2] FOREIGN KEY (anysrccol?) REFERENCES [anytarget?] (??, yep)"
          "ALTER TABLE [t2] ADD CONSTRAINT [t2r1] FOREIGN KEY (id) REFERENCES [t2] (??)"
          "ALTER TABLE [t2] ADD CONSTRAINT [t2r2] FOREIGN KEY (anysrccol?) REFERENCES [anytarget?] (??, yep)" ]

    Assert.Equal<string list>(expectedSqlStatements, actualSqlStatements)
            