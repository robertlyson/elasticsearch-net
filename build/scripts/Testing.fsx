﻿#I @"../../packages/build/FAKE/tools"
#r @"FakeLib.dll"
#r "System.Xml.Linq.dll"

#load @"Paths.fsx"

open System
open Fake 
open Paths
open Fake.Testing
open System.Xml.Linq;

// xunit console runner is broken on mono 4.0.2 better run with a nightly build:
// http://download.mono-project.com/archive/nightly/macos-10-x86/

// however even in the latest beta 4.3.0 the runner hangs on mono for me
// https://github.com/xunit/xunit/issues/158

module Tests = 
    let xmlOutput = Paths.Output("TestResults.xml")
    let htmlOutput = Paths.Output("TestResults.html")

    let RunAllUnitTests() =
        !! Paths.Source("Tests/bin/Release/Tests.dll") 
            |> xUnit2 (fun p -> 
            {
                p with 
                    XmlOutputPath = Some <| xmlOutput 
                    HtmlOutputPath = Some <| htmlOutput 
            } )

    let RunAllIntegrationTests(commaSeparatedEsVersions) =
        ActivateBuildFailureTarget "NotifyTestFailures"
        let esVersions = 
            match commaSeparatedEsVersions with
            | "" ->
                failwith "when running integrate you have to pass a commaseperated list of elasticsearch versions to test"
            | _ ->
                commaSeparatedEsVersions.Split ',' |> Array.toList 
        
        for esVersion in esVersions do
            setProcessEnvironVar "NEST_INTEGRATION_VERSION" esVersion
            !! Paths.Source("**/bin/Release/Tests.dll") 
                |> xUnit2 (fun p -> 
                { 
                    p with 
                        XmlOutputPath = Some <| xmlOutput 
                        HtmlOutputPath = Some <| htmlOutput 
                })

    let Notify = fun _ -> 
        match fileExists xmlOutput with
        | false -> ignore
        | _ ->
            let results = XDocument.Load xmlOutput
            let assembly = results.Root.Element <| XName.Get "assembly"
            let attr name = 
                let a = assembly.Attribute <| XName.Get name
                Int32.Parse(a.Value)

            let failed = 
                (results.Root.Descendants <| XName.Get "test") 
                |> Seq.filter(fun e -> (e.Attribute <| XName.Get "result").Value = "Fail")

            let errors = attr "failed"
            let total = attr "total"
            let skipped = attr "skipped"
            let o = sprintf "\"%s\"" ((new System.Uri(System.IO.Path.GetFullPath(htmlOutput))).AbsoluteUri)
            failed 
                |> Seq.iter (fun e -> 
                    let m  = (e.Attribute <| XName.Get "method").Value
                    let t  = (e.Attribute <| XName.Get "type").Value
                    traceError (sprintf "%s failed in %s" m t)
                    let messages = (e.Descendants <| XName.Get "message")
                    messages |> Seq.iter (fun m -> traceImportant m.Value )
                )

            match errors with
            | 0 ->
                let successMessage = sprintf "\"All %i tests are passing!\"" total
                printfn "%s" successMessage
                Paths.Tooling.Notifier.Exec ["-t " + successMessage; "-m " + successMessage]
                ignore
            | _ ->
                let errorMessage = sprintf "\"%i failed %i run, %i skipped\"" errors total skipped
                printfn "%s" errorMessage
                Paths.Tooling.Notifier.Exec ["-t " + errorMessage; "-m " + errorMessage; "-o " + o]
                ignore

    let RunContinuous = fun _ ->
        ActivateBuildFailureTarget "NotifyTestFailures"
        Paths.Tooling.Notifier.Exec ["-t " + "\"Starting tests!\""; "-m " + "\"...\""]
        //try 
        !! Paths.Source("Tests/bin/Release/Tests.dll") 
        |> xUnit2 (fun p -> 
        { 
            p with 
                XmlOutputPath = Some <| xmlOutput 
                HtmlOutputPath = Some <| htmlOutput 
        })
        //finally
        Notify() |> ignore
