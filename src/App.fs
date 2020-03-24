module App

open Fable.SimpleHttp
open Fable.SimpleJson
open Elmish
open Elmish.React

type AgeGroup =
    { age_group: string
      count: int
      percent: int }

type BirthAge =
    { year: int
      count: int }

type Model =
    | Failure of string
    | Loading
    | Success of AgeGroup list

type Msg =
    | LoadAgeGroup
    | LoadedAgeGroups of Result<AgeGroup list, string>
    | LoadedBirthAges of Result<BirthAge list, string>


let fetchDataByAgeGroup =
    async {
        let! (statusCode, responseText) = Http.get "/FenixWeb/Data/R14/1137/ByAgeGroup/"

        match statusCode with
        | 200 -> return LoadedAgeGroups(Json.tryParseAs<AgeGroup list> responseText)
        | _ -> return LoadedAgeGroups(Error responseText)
    }


let fetchDataByBirthAge =
    async {
        let! (statusCode, responseText) = Http.get "/FenixWeb/Data/R14/1137/"

        match statusCode with
        | 200 -> return LoadedBirthAges(Json.tryParseAs<BirthAge list> responseText)
        | _ -> return LoadedBirthAges(Error responseText)
    }

//TODO: Cmd batch and interop with JavaScript
let init() = 
    Loading, Cmd.ofMsg LoadAgeGroup

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | LoadAgeGroup -> (Loading, Cmd.OfAsync.result fetchDataByAgeGroup)

    | LoadedAgeGroups result ->
        match result with
        | Ok r14AgeGroups -> (Success r14AgeGroups, Cmd.none)
        | Error errorMsg -> (Failure errorMsg, Cmd.none)

    | LoadedBirthAges result ->
        match result with
        | Ok r14BirthAges -> (model, Cmd.none) //TODO: interop JavaScript
        | Error errorMsg -> (Failure errorMsg, Cmd.none)


open Fable.React
open Fable.React.Props

let renderTableRow (ageGroup: AgeGroup) =
    let maybeBold =
        if ageGroup.percent > 7 then [ FontWeight "bold" ] else []

    let percent =
        if ageGroup.percent > 0 then (ageGroup.percent.ToString() + "%") else "<1%"

    tr []
        [ td [ Style(TextAlign TextAlignOptions.Left :: maybeBold) ] [ str ageGroup.age_group ]
          td [ Style(TextAlign TextAlignOptions.Right :: maybeBold) ] [ str percent ]
          td [ Style [ TextAlign TextAlignOptions.Right ] ] [ str (ageGroup.count.ToString()) ] ]

let renderTable (model: Model) dispatch =
    match model with
    | Failure errorMessage ->
        div []
            [ p [] [ str "Kan inte ladda data från servern." ]
              p [] [ str errorMessage ] ]

    | Loading -> div [] [ p [] [ str "Laddar data..." ] ]

    | Success ageGroups ->
        div []
            [ table
                [ Class "table table-condensed table-extra-condensed"
                  Style
                      [ Clear "both"
                        Width "100%"
                        MarginTop "30px" ] ]
                  [ thead []
                        [ tr []
                              [ th
                                  [ Style
                                      [ Width "120px"
                                        TextAlign TextAlignOptions.Left ] ] [ str "Åldersgrupp" ]
                                th
                                    [ Style
                                        [ Width "120px"
                                          TextAlign TextAlignOptions.Right ] ] []
                                th
                                    [ Style
                                        [ Width "120px"
                                          TextAlign TextAlignOptions.Right ] ] [ str "Antal" ] ] ]
                    tbody [] (List.map renderTableRow ageGroups) ] ]


let view (model: Model) (dispatch: Dispatch<Msg>) =
    div [ Id "bootstrap-override" ]
        [ div []
              [ a
                  [ Class "pull-right btn btn-sm btn__svea--primary mb-4"
                    Href "../../Data/R14/1137/ByAgeGroup/?format=slk" ]
                    [ span [] [ str "Hämta grupperad till Excel" ] ] ]
          div []
              [ a
                  [ Class "pull-right btn btn-sm btn__svea--primary mb-4"
                    Href "../../Data/R14/1137/?format=slk" ] [ span [] [ str "Hämta grupperad till Excel" ] ] ]
          renderTable model dispatch ]

Program.mkProgram init update view
|> Program.withReactBatched "elmish-app"
|> Program.withConsoleTrace
|> Program.run
