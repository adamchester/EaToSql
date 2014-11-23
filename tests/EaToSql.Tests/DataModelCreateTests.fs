module EaToSql.Tests

open EaToSql.Model
open NUnit.Framework

module DataModelCreateTests =

    [<Test>]
    let ``create an InT with weird casing`` () =
      let actual = DataType.Create(name="InT", isAutoNum=false, columnLength=None, decimalScale=None, decimalPrec=None)
      Assert.AreEqual(DataType.Int, actual)

    [<Test>]
    let ``create an InTAuTo with weird casing`` () =
      let actual = DataType.Create(name="InT", isAutoNum=true, columnLength=None, decimalScale=None, decimalPrec=None)
      Assert.AreEqual(DataType.IntAuto, actual)

    [<Test>]
    let ``ignore length, scale, and prec when creating int`` () =
      let actual = DataType.Create(name="InT", isAutoNum=false,
                    columnLength=Some "1", decimalScale=Some "2", decimalPrec=Some "3")
      Assert.AreEqual(DataType.Int, actual)

    [<Test>]
    let ``use length, scale, and prec when creating dEcImAl`` () =
      let actual = DataType.Create(name="dEcImAl", isAutoNum=false,
                    columnLength=Some "42", decimalScale=Some "38", decimalPrec=Some "98")
      Assert.AreEqual(DataType.Decimal(98, 38), actual)

      let actual = DataType.Create(name="dEcImAl", isAutoNum=false,
                    columnLength=Some "99", decimalScale=Some "74", decimalPrec=Some "01029")
      Assert.AreEqual(DataType.Decimal(1029, 74), actual)

    [<Test>]
    let ``use the lowercase sql name if the type is not a special named one`` () =
      let actual = DataType.Create(name="anythingGoes", isAutoNum=false,
                    columnLength=None, decimalScale=None, decimalPrec=None)
      Assert.AreEqual(DataType.SQLT("anythinggoes"), actual)

      let actual = DataType.Create(name="itWiLlBeConvertedToLowerCase", isAutoNum=false,
                    columnLength=None, decimalScale=None, decimalPrec=None)
      Assert.AreEqual(DataType.SQLT("itwillbeconvertedtolowercase"), actual)
