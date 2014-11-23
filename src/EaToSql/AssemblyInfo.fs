namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("EaToSql")>]
[<assembly: AssemblyProductAttribute("EaToSql")>]
[<assembly: AssemblyDescriptionAttribute("Converts EA XMI to SQL")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
