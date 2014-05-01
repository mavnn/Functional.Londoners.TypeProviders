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

    let provider = ProvidedTypeDefinition(asm, ns, "Template", Some typeof<obj>)

    let findModelFields template =
        let regex = Regex(@"{{ (?<fieldName>\S+) }}", RegexOptions.Compiled)
        regex.Matches(template)
        |> Seq.cast
        |> Seq.map (fun (m : Match) -> m.Groups.["fieldName"].Value)
        |> Seq.toList

    let parameters = [ProvidedStaticParameter("TemplateDirectory", typeof<string>)]

    do provider.DefineStaticParameters(parameters,
        fun typeName args -> 
            let dir = args.[0] :?> string

            let tProvider = ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>)

            let templates =
                Directory.GetFiles(dir, "*.t_t")
                |> Seq.map (fun f ->
                    Path.GetFileNameWithoutExtension(f), File.ReadAllText(f))

            let createTemplateType (name, template) =
                let modelFieldNames = findModelFields template
                let templateType = ProvidedTypeDefinition(name, Some typeof<obj>)

                let (|IntField|DateField|StringField|) field =
                    match Seq.toList field with
                    | 'i'::':'::i ->
                        IntField (System.String.Concat i)
                    | 'd'::':'::date ->
                        DateField (System.String.Concat date)
                    | 's'::':'::str ->
                        StringField (System.String.Concat str)
                    | str ->
                        StringField (System.String.Concat str)

                let fieldToParam field =
                    match field with
                    | IntField name ->
                        ProvidedParameter(name, typeof<int>)
                    | DateField name ->
                        ProvidedParameter(name, typeof<System.DateTime>)
                    | StringField name ->
                        ProvidedParameter(name, typeof<string>)

                let render =
                    let m = ProvidedMethod(
                                "Render",
                                modelFieldNames |> List.map fieldToParam,
                                typeof<string>)
                    m.InvokeCode <-
                        fun args ->
                            let folder values fieldAndValue =
                                let name, valueExpr = fieldAndValue
                                match name with
                                | IntField _ ->
                                    <@ (name, string (%%valueExpr:int))::%values @>
                                | DateField _ ->
                                    <@ (name, (%%valueExpr:System.DateTime).ToString())::%values @>
                                | StringField _ ->
                                    <@ (name, (%%valueExpr:string))::%values @>

                            let fieldsAndValues =
                                List.zip modelFieldNames args

                            let fields = fieldsAndValues |> List.fold folder <@ [] @>
                            <@
                                let folder (state : string) (field : string * string) =
                                    state.Replace("{{ " + fst field + " }}", snd field)
                                %fields
                                |> List.fold folder template
                            @>.Raw
                    m.IsStaticMethod <- true
                    m

                templateType.AddMember(render)
                templateType
            
            tProvider.AddMembers([for t in templates -> createTemplateType t])
            tProvider)

    do
        this.AddNamespace(ns, [provider])

[<assembly:TypeProviderAssembly>]
do ()