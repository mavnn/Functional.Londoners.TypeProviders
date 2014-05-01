module ``F#unctional``.Londoners.TP

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open System.IO
open System.Text.RegularExpressions

[<TypeProvider>]
type MyTypeProvider () as this =
    inherit TypeProviderForNamespaces ()

    let ns = "F#unctional.Londoners.Provided"
    let asm = Assembly.GetExecutingAssembly()

    let findModelFields template =
        let regex = Regex(@"{{ (?<fieldName>\S+) }}", RegexOptions.Compiled)
        regex.Matches(template)
        |> Seq.cast
        |> Seq.map (fun (m : Match) -> m.Groups.["fieldName"].Value)
        |> Seq.toList

    let templates =
        Directory.GetFiles(@"C:\Temp", "*.t_t")
        |> Seq.map (fun f ->
            Path.GetFileNameWithoutExtension(f), File.ReadAllText(f))

    let createTemplateType (name, template) =
        let modelFields = findModelFields template
        let templateType = ProvidedTypeDefinition(asm, ns, name, Some typeof<obj>)

        let render =
            let m = ProvidedMethod(
                        "Render",
                        modelFields |> List.map (fun f -> ProvidedParameter(f, typeof<string>)),
                        typeof<string>)
            m.InvokeCode <-
                fun args ->
                    let folder values arg =
                        <@ (%%arg:string)::%values @>
                    let fieldValues = args |> List.fold folder <@ [] @>
                    <@
                        let fields = List.zip modelFields (%fieldValues |> List.rev)
                        let folder (state : string) (field : string * string) =
                            state.Replace("{{ " + fst field + " }}", snd field)
                        fields
                        |> List.fold folder template
                    @>.Raw
            m.IsStaticMethod <- true
            m

        templateType.AddMember(render)
        templateType

    do
        this.AddNamespace(ns, [for t in templates -> createTemplateType t])

[<assembly:TypeProviderAssembly>]
do ()