module Handle.Saga

open Akka.FSharp
open System
open Kernel.Types
open AkkaCommon
open CommandHandler
open Common.Operators

type ForwardCommand = Forward of Command * Compensation: Command
type BackwardCommand = Backward of Command * Compensation: Command

[<RequireQualifiedAccessAttribute>]
type SagaState =
    | Empty
    | Fill of ForwardCommand list
    | Forward of Currect: ForwardCommand * Remains: ForwardCommand list * Completed: ForwardCommand list
    | Backward of Error: string * Currect: BackwardCommand * Remains: BackwardCommand list * Completed: BackwardCommand list
    | BackwardAbort of Error: string
    | ForwardComplete
    | BackwardComplete of Error: string
    | Stop
    
    
[<RequireQualifiedAccessAttribute>]
type SagaEvent =
    | SagaForwardCommandAdded of ForwardCommand
    | Started
    | Forwarded
    | ForwardFailed of Error: string //TODO make error generic?
    | Backwarded
    | BackwardFailed of Error: string
    | ForwardDone
    | BackwardDone
    | Stoped


[<RequireQualifiedAccessAttribute>]
type SagaCommand =
    | AddForwardCommand of ForwardCommand
    | Start
    | Stop
    
    
[<RequireQualifiedAccess>]
type SagaResponse =
    | ForwardComplete
    | BackwardComplete of Error: string
    | BackwardAbort of Error: string
    | Stop

let forwardToBackward (Forward (command, compensation)) = Backward (command, compensation)

let applyEvent state event =
    //printfn "Current state is: %A and event %A" state event

    match state, event with
    | SagaState.Empty, SagaEvent.SagaForwardCommandAdded cmd ->
        SagaState.Fill ^ [cmd]

    | SagaState.Fill cmds, SagaEvent.SagaForwardCommandAdded cmd ->
        SagaState.Fill ^ (cmd::cmds)

    | SagaState.Fill (_::_ as cmds), SagaEvent.Started ->
        let current, remains = cmds |> List.rev |> (fun (x::xs) -> x, xs)
        SagaState.Forward (current, remains, [])

    | SagaState.Forward (current, next::remains, completed), SagaEvent.Forwarded ->
        SagaState.Forward (next, remains, current::completed)

    | SagaState.Forward (_, [], _), SagaEvent.Forwarded ->
        SagaState.ForwardComplete
    
    | SagaState.Forward (current, _, completed), SagaEvent.ForwardFailed error ->
        SagaState.Backward (error, current |> forwardToBackward, completed |> List.map forwardToBackward, [])

    | SagaState.Backward (error, current, next::remains, completed), SagaEvent.Backwarded ->
        SagaState.Backward (error, next, remains, current::completed)

    | SagaState.Backward (error, _, [], _), SagaEvent.Backwarded ->
        SagaState.BackwardComplete error

    | SagaState.Backward (error, _, _, _), SagaEvent.BackwardFailed backwardError ->
        SagaState.BackwardAbort (error + " " + backwardError) //TODO make better concatenation

    | _, SagaEvent.Stoped ->
        SagaState.Stop
    | state, event -> failwith (sprintf "Wrong state %A %A" state event)

let playEvents state events = events |> List.fold applyEvent state


let runSaga () =
    spawn system ("saga-handler-actor-" + Guid.NewGuid().ToString()) 
            (fun mailbox -> 
                let rec loop state = actor {
                    match state with 
                    | SagaState.Empty ->
                        let! msg = mailbox.Receive()

                        match msg with
                        | SagaCommand.AddForwardCommand forwardCommand ->
                            let events = [SagaEvent.SagaForwardCommandAdded forwardCommand]
                            return! loop (events |> playEvents state)
                        | _ ->
                            return! loop state
                    | SagaState.Fill _ ->
                        let! msg = mailbox.Receive()

                        match msg with
                        | SagaCommand.AddForwardCommand forwardCommand ->
                            let events = [SagaEvent.SagaForwardCommandAdded forwardCommand]
                            
                            return! loop (events |> playEvents state)
                        | SagaCommand.Start ->
                            let events = [SagaEvent.Started]
                            return! loop (events |> playEvents state)
                        
                        | _ ->
                            return! loop state
                    | SagaState.Forward (Forward (command, _), _, _) ->
                        let sagaEvents =
                            match handleCommand command with
                            | Ok () -> [SagaEvent.Forwarded]
                            | Error error -> [SagaEvent.ForwardFailed error]                            

                        return! loop (sagaEvents |> playEvents state)
                    | SagaState.Backward (_, Backward (_, compensation), _, _) ->
                        let sagaEvents =
                            match handleCommand compensation with
                            | Ok () -> [SagaEvent.Backwarded]
                            | Error backwardError -> [SagaEvent.BackwardFailed backwardError]

                        return! loop (sagaEvents |> playEvents state)

                    | SagaState.BackwardAbort error ->
                        mailbox.Sender() <! SagaResponse.BackwardAbort error

                    | SagaState.ForwardComplete ->
                        mailbox.Sender() <! SagaResponse.ForwardComplete

                    | SagaState.BackwardComplete error ->
                        mailbox.Sender() <! SagaResponse.BackwardComplete error

                    | SagaState.Stop ->
                        mailbox.Sender() <! SagaResponse.Stop
                }
            
                loop SagaState.Empty)