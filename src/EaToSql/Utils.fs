﻿[<AutoOpen>]
module internal EaToSql.Utils

/// Gets an item from a dictionary, or fails if the item doesn't exist.
let getOrFail failFormat (dict: System.Collections.Generic.IDictionary<'key, 'value>) (id: 'key) =
    let exists, elem = dict.TryGetValue id
    if not exists then failwithf failFormat id
    elem

/// Generates comma-separated values from a sequence of strings
let csv (strings:string seq) = System.String.Join(",\n\t", strings|>Seq.toArray)

/// Joins a sequence of strings into a a single string using the provided separator.
let stringJoin separator (strings: string seq) = System.String.Join(separator, strings|>Seq.toArray)

/// Joins a sequence of strings using new line characters.
let joinNewLines strings = stringJoin System.Environment.NewLine strings

/// Group a sequence by a custom projection, then take a number from each group,
/// then flatten the results into a new sequence.
let groupByTakeFlatten (projection: 'a -> 'b) (takeCount) (items: 'a seq) =
    items
    |> Seq.groupBy projection
    |> Seq.map (fun (_, grp) -> (grp |> Seq.take takeCount))
    |> Seq.collect id


