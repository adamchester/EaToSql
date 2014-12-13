namespace EaToSql.Tests

open EaToSql
open Xunit

module DataModelCreateTests =

    [<Fact>]
    let ``create an InT with weird casing`` () =
        let actual = DataType.Create("InT", isAutoNum=false, length=None, decimalScale=None, decimalPrec=None)
        Assert.Equal(Int, actual)

    [<Fact>]
    let ``create an InTAuTo with weird casing`` () =
        let actual = DataType.Create("InT", isAutoNum=true, length=None, decimalScale=None, decimalPrec=None)
        Assert.Equal(IntAuto, actual)

    [<Fact>]
    let ``ignore length, scale, and prec when creating int`` () =
        let actual = DataType.Create("InT", isAutoNum=false, length=Some 1, decimalScale=Some 2, decimalPrec=Some 3)
        Assert.Equal(Int, actual)

    [<Fact>]
    let ``use only scale and prec when creating dEcImAl`` () =
        let actual = DataType.Create("dEcImAl", isAutoNum=true, length=Some 42, decimalPrec=Some 98, decimalScale=Some 38)
        Assert.Equal(Decimal(98, 38), actual)

        let actual = DataType.Create("dEcImAl", isAutoNum=true, length=Some 99, decimalPrec=Some 01029, decimalScale=Some 74)
        Assert.Equal(Decimal(1029, 74), actual)

    [<Fact>]
    let ``convert to lowercase name if the type is not a special named one`` () =
        let actual = DataType.Create("anythingGoes", isAutoNum=true, length=None, decimalScale=None, decimalPrec=None)
        Assert.Equal(SQLT("anythinggoes"), actual)

        let actual = DataType.Create("itWiLlBeConvertedToLowerCase", isAutoNum=true, length=None, decimalScale=None, decimalPrec=None)
        Assert.Equal(SQLT("itwillbeconvertedtolowercase"), actual)
