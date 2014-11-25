module EaToSql.Tests.ApiTests

open EaToSql
open NUnit.Framework

let expectedTables = [
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

[<Test>]
let ``generate the correct model from samples`` () =
    let sampleXmlFile = @"SampleModel_xmi2_1.xml"
    use xml = new System.IO.StreamReader(sampleXmlFile)
    let actual = Api.readTablesFromXmi (xml) |> Seq.toArray
    Assert.AreEqual(expectedTables, actual)
