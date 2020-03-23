module App

open Fable.SimpleHttp
open Fable.SimpleJson 
open Fable.React
open Fable.React.Props
open Elmish
open Elmish.React

type AgeGroup = 
    {
        age_group: string
        count: int
        percent: int
    }

type BirthAge = 
    {
        year: int
        count: int
    }

type Model =
    | Failure of string
    | Loading
    | Success of AgeGroup list

type Msg = 
    | LoadedAgeGroups of Result<AgeGroup list, string>
    | LoadedBirthAges of Result<BirthAge list, string>


let fetchDataByAgeGroup = async {
    let! (statusCode, responseText) = Http.get "/FenixWeb/Data/R14/1137/ByAgeGroup/"

    match statusCode with
    | 200 -> return LoadedAgeGroups (Json.tryParseAs<AgeGroup list> responseText)
    | _ -> return LoadedAgeGroups (Error responseText)
}


let fetchDataByBirthAge = async {
    let! (statusCode, responseText) = Http.get "/FenixWeb/Data/R14/1137/"

    match statusCode with
    | 200 -> return LoadedBirthAges (Json.tryParseAs<BirthAge list> responseText)
    | _ -> return LoadedBirthAges (Error responseText)
}

//TODO: Cmd batch and interop with JavaScript
let init () =
  Loading, Cmd.OfAsync.result fetchDataByAgeGroup

let update (msg: Msg) (model: Model, cmd: Cmd<Msg>) : Model * Cmd<Msg> =
    match msg with
    | LoadedAgeGroups result ->
        match result with
        | Ok r14AgeGroups ->
            (Success r14AgeGroups, Cmd.none)
        | Error errorMsg ->
            (Failure errorMsg, Cmd.none)
    
    | LoadedBirthAges result ->
        match result with
        | Ok r14BirthAges ->
            (model, Cmd.none) //TODO: interop JavaScript
        | Error errorMsg ->
            (Failure errorMsg, Cmd.none)


let view model dispatch =
  div []
      []



Program.mkSimple init update view
|> Program.withReactBatched "elmish-app"
|> Program.withConsoleTrace
|> Program.run