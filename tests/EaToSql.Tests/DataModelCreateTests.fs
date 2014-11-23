namespace EaToSql.Tests

open EaToSql
open NUnit.Framework

module DataModelCreateTests =

    [<Test>]
    let ``create an InT with weird casing`` () =
        let actual = DataType.Create("InT", isAutoNum=false, length=None, decimalScale=None, decimalPrec=None)
        Assert.AreEqual(Int, actual)

    [<Test>]
    let ``create an InTAuTo with weird casing`` () =
        let actual = DataType.Create("InT", isAutoNum=true, length=None, decimalScale=None, decimalPrec=None)
        Assert.AreEqual(IntAuto, actual)

    [<Test>]
    let ``ignore length, scale, and prec when creating int`` () =
        let actual = DataType.Create("InT", isAutoNum=false, length=Some "1", decimalScale=Some "2", decimalPrec=Some "3")
        Assert.AreEqual(Int, actual)

    [<Test>]
    let ``use length, scale, and prec when creating dEcImAl`` () =
        let actual = DataType.Create("dEcImAl", isAutoNum=false, length=Some "42", decimalScale=Some "38", decimalPrec=Some "98")
        Assert.AreEqual(Decimal(98, 38), actual)

        let actual = DataType.Create("dEcImAl", isAutoNum=false, length=Some "99", decimalScale=Some "74", decimalPrec=Some "01029")
        Assert.AreEqual(Decimal(1029, 74), actual)

    [<Test>]
    let ``use the lowercase name if the type is not a special named one`` () =
        let actual = DataType.Create("anythingGoes", isAutoNum=false, length=None, decimalScale=None, decimalPrec=None)
        Assert.AreEqual(SQLT("anythinggoes"), actual)

        let actual = DataType.Create("itWiLlBeConvertedToLowerCase", isAutoNum=false, length=None, decimalScale=None, decimalPrec=None)
        Assert.AreEqual(SQLT("itwillbeconvertedtolowercase"), actual)
