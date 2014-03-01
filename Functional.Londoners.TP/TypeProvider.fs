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

    let helloWorld =
        ProvidedProperty(
            "Hello", 
            typeof<string>, 
            IsStatic = true,
            GetterCode = (fun _ -> <@@ "Hello world" @@>))

    let cons =
        ProvidedConstructor(
            [],
            InvokeCode = fun _ -> <@@ "My internal state" :> obj @@>)

    let paramCons =
        ProvidedConstructor(
            [ProvidedParameter("InternalState", typeof<string>)],
            InvokeCode = fun args -> <@@ (%%(args.[0]) : string) :> obj @@>)

    let internalState =
        ProvidedProperty(
            "InternalState",
            typeof<string>,
            IsStatic = false,
            GetterCode = (fun args -> <@@ (%%args.[0] :> obj) :?> string @@>))

    do
        newType.AddMember(helloWorld)
        newType.AddMember(cons)
        newType.AddMember(paramCons)
        newType.AddMember(internalState)

    do
        this.AddNamespace(ns, [newType])

[<assembly:TypeProviderAssembly>]
do ()