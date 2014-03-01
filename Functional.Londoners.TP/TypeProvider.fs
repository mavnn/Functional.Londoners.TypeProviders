module ``F#unctional``.Londoners.TP

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices

[<TypeProvider>]
type MyTypeProvider () =
    inherit TypeProviderForNamespaces ()

[<assembly:TypeProviderAssembly>]
do ()