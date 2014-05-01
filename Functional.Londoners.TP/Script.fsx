// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.

#r "bin/Debug/Functional.Londoners.TP.dll"
open ``F#unctional``.Londoners

Provided.test.Render("first", "second")

Provided.TypedTemplate.Render(10, System.DateTime.Today, "A string")