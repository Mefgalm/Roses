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
    | Backward of Currect: BackwardCommand * Remains: BackwardCommand list * Completed: BackwardCommand list
    | BackwardAbort of Remains: BackwardCommand list * Completed: BackwardCommand list
    | ForwardComplete of Completed: ForwardCommand list
    | BackwardComplete of Completed: BackwardCommand list
    | Stop
    
    
[<RequireQualifiedAccessAttribute>]
type SagaEvent =
    | SagaForwardCommandAdded of ForwardCommand
    | Started
    | Forwarded
    | ForwardFailed
    | Backwarded
    | BackwardFailed
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
    | BackwardComplete
    | BackwardAbort of string
    | Stop of string

let forwardToBackward (Forward (command, compensation)) = Backward (command, compensation)

let applyEvent state event =
    printfn "Current state is: %A and event %A" state event

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

    | SagaState.Forward (current, [], completed), SagaEvent.Forwarded ->
        SagaState.ForwardComplete (current::completed)
    
    | SagaState.Forward (current, _, completed), SagaEvent.ForwardFailed ->
        SagaState.Backward (current |> forwardToBackward, completed |> List.map forwardToBackward, [])

    | SagaState.Backward (current, next::remains, completed), SagaEvent.Backwarded ->
        SagaState.Backward (next, remains, current::completed)

    | SagaState.Backward (current, [], completed), SagaEvent.Backwarded ->
        SagaState.BackwardComplete (current::completed)

    | SagaState.Backward (_, remains, completed), SagaEvent.BackwardFailed ->
        SagaState.BackwardAbort (remains, completed)

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
                            | Error _ -> [SagaEvent.ForwardFailed]                            

                        return! loop (sagaEvents |> playEvents state)
                    | SagaState.Backward (Backward (_, compensation), _, _) ->
                        let sagaEvents =
                            match handleCommand compensation with
                            | Ok () -> [SagaEvent.Backwarded]
                            | Error _ -> [SagaEvent.BackwardFailed]

                        return! loop (sagaEvents |> playEvents state)

                    | SagaState.BackwardAbort (remains, completed) ->
                        mailbox.Sender() <! SagaResponse.BackwardAbort "BackwardAbort"

                    | SagaState.ForwardComplete completed ->
                        mailbox.Sender() <! SagaResponse.ForwardComplete

                    | SagaState.BackwardComplete completed ->
                        mailbox.Sender() <! SagaResponse.BackwardComplete

                    | SagaState.Stop ->
                        mailbox.Sender() <! SagaResponse.Stop
                }
            
                loop SagaState.Empty)