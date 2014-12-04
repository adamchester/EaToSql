namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("EaToSql")>]
[<assembly: AssemblyProductAttribute("EaToSql")>]
[<assembly: AssemblyDescriptionAttribute("Converts EA XMI to SQL")>]
[<assembly: AssemblyVersionAttribute("0.0.2")>]
[<assembly: AssemblyFileVersionAttribute("0.0.2")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.2"
