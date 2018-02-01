namespace Arcane.Space.Back

open System
open System.Collections.Generic

module LiftManagement =
    type Passenger = {
        Id: Guid
    }    

    type LiftNumber = LiftNumber of int
    type FloorNumber = FloorNumber of int
    type Velocity = Velocity of int

    type MovingState =
        | Empty
        | NotEmpty

    type MovingStateData = {
        StartFloor: FloorNumber
        FinishFloor: FloorNumber
        State: MovingState
    }

    type State =
        | Idle of FloorNumber
        | Moving of MovingStateData

    type Lift = {
        Number: LiftNumber
        State: State
        Velocity: Velocity
        Passengers: Passenger[]
    }

    type LiftCluster = {
        Lifts: Lift[]
        MaxFloor: FloorNumber
        AwaitingFloors: HashSet<FloorNumber>
    }

    type Result<'T> =
        | Success of 'T
        | Fail of string

    

    let createCluster (floorCount: int, liftCount: int, liftVelocity: int) : Result<LiftCluster> =
        let createLift (liftNumber : int) : Lift =
            { 
                Number = liftNumber |> LiftNumber
                State = 1 |> FloorNumber |> State.Idle
                Velocity = liftVelocity |> Velocity
                Passengers = [||]
            }

        let lifts : Lift[] =
            Seq.init liftCount (fun index -> index + 1)
            |> Seq.map createLift
            |> Seq.toArray

        match floorCount, liftCount, liftVelocity with
        | value, _, _ when value <= 0 -> "Floor count must be positive" |> Fail
        | _, value, _ when value <= 0 -> "Lift count must be positive" |> Fail
        | _, _, value when value <= 0 -> "Lift velocity must be positive" |> Fail
        | _ ->             
            {
                Lifts = lifts
                MaxFloor = floorCount |> FloorNumber
                AwaitingFloors = new HashSet<FloorNumber>()
            } |> Success
        
    let moveLift (lift: Lift) (targetFloor: FloorNumber) : Result<State> =
        let (FloorNumber targetFloor') = targetFloor

        match lift.State with
        | Idle (FloorNumber floor) -> 
            match floor - targetFloor' with
            | 0 -> lift.State |> Success
            | _ ->
                {  
                    StartFloor = floor |> FloorNumber
                    FinishFloor = targetFloor
                    State = Empty
                } |> Moving |> Success
        | Moving _ -> sprintf "Can't move busy lift %A" (lift, targetFloor) |> Fail


    let callLift (sourceFloor: FloorNumber) (targetFloor: FloorNumber) (cluster: LiftCluster) : Result<LiftCluster> =                  
        let (FloorNumber sourceFloor') = sourceFloor                

        let sortedByPriorityLifts = cluster.Lifts
                                    |> Array.sortBy 
                                        (fun lift ->
                                            match lift.State with
                                            | Idle (FloorNumber floorNumber) -> Math.Abs(floorNumber - sourceFloor')
                                            | Moving _ -> System.Int32.MaxValue
                                        )
                                    |> Array.toList

        match sortedByPriorityLifts with
        | [] -> "Can't call lift, because there are no lifts" |> Fail
        | nearestFreeLift::restLifts -> 
            let moveResult = moveLift nearestFreeLift targetFloor
            match moveResult with
            | Success state -> 
                let lifts = { nearestFreeLift with State = state }::restLifts
                {cluster with Lifts = lifts |> List.toArray} |> Success
            | Fail _ ->
                cluster.AwaitingFloors.Add(targetFloor) |> ignore
                cluster |> Success