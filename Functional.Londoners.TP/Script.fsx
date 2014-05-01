// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.

#r "bin/Debug/Functional.Londoners.TP.dll"
open ``F#unctional``.Londoners

type Temp = Provided.Template< @"c:\Temp">

Temp.TypedTemplate.Render(22, System.DateTime.Now, "My string!")