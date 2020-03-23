module App

type Model = int

type Msg = Increment | Decrement


open Elmish

let init () =
  0

let update (msg:Msg) count =
  match msg with
  | Increment -> count + 1
  | Decrement -> count - 1

open Fable.React
open Fable.React.Props


let view model dispatch =
  div []
      [ button [ OnClick (fun _ -> dispatch Decrement) ] [ str "-" ]
        div [] [ str (sprintf "%A" model) ]
        button [ OnClick (fun _ -> dispatch Increment) ] [ str "+" ] ]

open Elmish.React

Program.mkSimple init update view
|> Program.withReactBatched "elmish-app"
|> Program.withConsoleTrace
|> Program.run