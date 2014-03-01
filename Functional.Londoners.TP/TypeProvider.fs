module ``F#unctional``.Londoners.TP

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection

[<TypeProvider>]
type MyTypeProvider () as this =
    inherit TypeProviderForNamespaces ()

    let ns = "F#unctional.Londoners.Provided"
    let asm = Assembly.GetExecutingAssembly()

    let newType = 
        ProvidedTypeDefinition(asm, ns, "NewType", Some typeof<obj>)

    do
        this.AddNamespace(ns, [newType])

[<assembly:TypeProviderAssembly>]
do ()