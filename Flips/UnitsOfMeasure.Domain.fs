﻿namespace Flips.UnitsOfMeasure

open Flips
open Flips.UnitsOfMeasure.Types

[<RequireQualifiedAccess>]
module Decision =

    /// <summary>Create a Decision with a given Name, Type, and UnitOfMeasure</summary>
    /// <remarks>This function is here for completeness. It is recommend to use the functions for the specific decision types.</remarks>
    /// <typeparam name="'Measure">The Unit of Measure for the Decision</typeparam>
    /// <param name="decisionName">The unique identifier for the Decision</param>
    /// <param name="decitionType">The type of the decision</param>
    /// <returns>A new Decision with the given DecisionType</returns
    let create<[<Measure>] 'Measure> decisionName decisionType =
        let d = Decision.create decisionName decisionType
        Decision<'Measure>.Value d

    /// <summary>Create a Boolean type of decision with a Unit of Measure</summary>
    /// <remarks>These types of decisions are meant to represent True/False, Yes/No types of decisions. They map to 0.0 and 1.0 in the mathematical representation.</remarks>
    /// <typeparam name="'Measure">The Unit of Measure for the Decision</typeparam>
    /// <param name="decisionName">The unique identifier for the decision</param>
    /// <returns>A new Decision with a type of Boolean</returns
    let createBoolean<[<Measure>] 'Measure> decisionName =
        let d = Decision.createBoolean decisionName
        Decision<'Measure>.Value d

    /// <summary>Create an Integer type of decision with a Unit of Measure</summary>
    /// <remarks>These types of decisions will take on whole values and are bounded by the Lower and Upper bounds, inclusive.</remarks>
    /// <typeparam name="'Measure">The Unit of Measure for the Decision</typeparam>
    /// <param name="decisionName">The unique identifier for the decision</param>
    /// <param name="lowerBound">The smallest value the decision is allowed to take on</param>
    /// <param name="upperBound">The largest value the decision is allowed to take on</param>
    /// <returns>A new Decision with a type of Integer</returns
    let createInteger<[<Measure>] 'Measure> decisionName (lowerBound:float<'Measure>) (upperBound:float<'Measure>) =
        let d = Decision.createInteger decisionName (float lowerBound) (float upperBound)
        Decision<'Measure>.Value d


    /// <summary>Create an Continuous type of decision with a Unit of Measure</summary>
    /// <remarks>These types of decisions will take on any value within the Lower and Upper bounds, inclusive.</remarks>
    /// <typeparam name="'Measure">The Unit of Measure for the Decision</typeparam>
    /// <param name="decisionName">The unique identifier for the decision</param>
    /// <param name="lowerBound">The smallest value the decision is allowed to take on</param>
    /// <param name="upperBound">The largest value the decision is allowed to take on</param>
    /// <returns>A new Decision with a type of Continuous</returns
    let createContinuous<[<Measure>] 'Measure> decisionName (lowerBound:float<'Measure>) (upperBound:float<'Measure>) =
        let d = Decision.createContinuous decisionName (float lowerBound) (float upperBound)
        Decision<'Measure>.Value d


[<RequireQualifiedAccess>]
module Objective =
    
    /// <summary>Create an Objective for an optimization model</summary>
    /// <param name="objectiveName">The name which describes the goal of the objective function</param>
    /// <param name="objectiveSense">The goal of the objective: Maximize or Minimize</param>
    /// <param name="objectiveExpression">The Linear Expression which describes the goal of the model</param>
    /// <returns>A new Objective</returns
    let create objectiveName sense (Value expr:LinearExpression<'Measure>) =
        Objective.create objectiveName sense expr


[<RequireQualifiedAccess>]
module Solution =

    /// <summary>A function for taking the initial set of Decisions and returning the values the solver found</summary>
    /// <param name="solution">The solution that is used to look up the solver values</param>
    /// <param name="decisions">A Map<'Key, Decision<'Measure>> that will be used for the lookups</param>
    /// <returns>A new Map<'Key,float<'Mesure>> where the values are the recommendations from the solver</returns>
    let getValues (s:Types.Solution) (m:Map<_,Decision<'Measure>>) =
        let getWithDefault _ (Decision.Value d:Decision<'Measure>) =
            match Map.tryFind d s.DecisionResults with
            | Some v -> FSharp.Core.LanguagePrimitives.FloatWithMeasure<'Measure> v
            | None -> FSharp.Core.LanguagePrimitives.FloatWithMeasure<'Measure> 0.0

        m |> Map.map getWithDefault

    /// <summary>A function for evaluating the resulting value of a LinearExpression after solving the model</summary>
    /// <param name="solution">The solution used for lookup up the results of Decisions</param>
    /// <param name="expression">The LinearExpression with a Unit of Measure to evaluate the resulting value for</param>
    /// <returns>A float with a Unit of Measure which is the simplification of the LinearExpression</returns>
    let evaluate (solution:Types.Solution) (Value expression:LinearExpression<'Measure>) =
        Flips.Types.LinearExpression.Evaluate solution.DecisionResults expression
        |> FSharp.Core.LanguagePrimitives.FloatWithMeasure<'Measure>


[<AutoOpen>]
module Builders =

    /// <summary>A Computation Expression for creating tuples of type ('Key * Decision<'Measure>)</summary>
    /// <typeparam name="'Measure">The Unit of Measure for the Decisions</typeparam>
    /// <param name="decisionSetPrefix">The prefix used for naming the Decisions</param>
    /// <returns>A seq of type ('Key * Decision<'Measure>). The result is typically used to create a Map or SliceMap</returns
    type DecisionBuilder<[<Measure>] 'Measure> (decisionSetPrefix:string) =

        let createDecision indices decisionType =
            let name = namer decisionSetPrefix indices
            let decision = Decision.create<'Measure> name decisionType
            indices, decision

        member this.Yield (decisionType:DecisionType<'Measure>) =
            match decisionType with
            | DecisionType.Boolean -> Flips.Types.DecisionType.Boolean
            | DecisionType.Integer (lb, ub) -> Flips.Types.DecisionType.Integer (float lb, float ub)
            | DecisionType.Continuous (lb, ub) -> Flips.Types.DecisionType.Continuous (float lb, float ub)

        member this.For(source:seq<'a>, body:'a -> 'b) =
            source |> Seq.map (fun x -> x, body x)

        member this.Run(a:seq<_*seq<_*seq<_*seq<_*seq<_*seq<_*seq<_*seq<_*Flips.Types.DecisionType>>>>>>>>) =
            a |> Seq.collect (fun (a, b) ->
            b |> Seq.collect (fun (b, c) ->
            c |> Seq.collect (fun (c, d) ->
            d |> Seq.collect (fun (d, e) ->
            e |> Seq.collect (fun (e, f) ->
            f |> Seq.collect (fun (f, g) ->
            g |> Seq.collect (fun (g, h) ->
            h |> Seq.map (fun (h, i) -> createDecision (a,b,c,d,e,f,g,h) i))))))))

        member this.Run(a:seq<_*seq<_*seq<_*seq<_*seq<_*seq<_*seq<_*Flips.Types.DecisionType>>>>>>>) =
            a |> Seq.collect (fun (a, b) ->
            b |> Seq.collect (fun (b, c) ->
            c |> Seq.collect (fun (c, d) ->
            d |> Seq.collect (fun (d, e) ->
            e |> Seq.collect (fun (e, f) ->
            f |> Seq.collect (fun (f, g) ->
            g |> Seq.map (fun (g, h) -> createDecision (a,b,c,d,e,f,g) h)))))))

        member this.Run(a:seq<_*seq<_*seq<_*seq<_*seq<_*seq<_*Flips.Types.DecisionType>>>>>>) =
            a |> Seq.collect (fun (a, b) ->
            b |> Seq.collect (fun (b, c) ->
            c |> Seq.collect (fun (c, d) ->
            d |> Seq.collect (fun (d, e) ->
            e |> Seq.collect (fun (e, f) ->
            f |> Seq.map (fun (f, g) -> createDecision (a,b,c,d,e,f) g))))))

        member this.Run(a:seq<_*seq<_*seq<_*seq<_*seq<_*Flips.Types.DecisionType>>>>>) =
            a |> Seq.collect (fun (a, b) ->
            b |> Seq.collect (fun (b, c) ->
            c |> Seq.collect (fun (c, d) ->
            d |> Seq.collect (fun (d, e) ->
            e |> Seq.map (fun (e, f) -> createDecision (a,b,c,d,e) f)))))

        member this.Run(a:seq<_*seq<_*seq<_*seq<_*Flips.Types.DecisionType>>>>) =
            a |> Seq.collect (fun (a, b) ->
            b |> Seq.collect (fun (b, c) ->
            c |> Seq.collect (fun (c, d) ->
            d |> Seq.map (fun (d, e) -> createDecision (a,b,c,d) e))))

        member this.Run(a:seq<_*seq<_*seq<_*Flips.Types.DecisionType>>>) =
            a |> Seq.collect (fun (a, b) ->
            b |> Seq.collect (fun (b, c) ->
            c |> Seq.map (fun (c, d) -> createDecision (a,b,c) d)))

        member this.Run(a:seq<_*seq<_*Flips.Types.DecisionType>>) =
            a |> Seq.collect (fun (a, b) ->
            b |> Seq.map (fun (b, c) -> createDecision (a, b) c))

        member this.Run(a:seq<_*Flips.Types.DecisionType>) = 
            a |> Seq.map (fun (a, b) -> createDecision a b)


[<AutoOpen>]
module Sum =

    open Flips.SliceMap

    type internal Summer () =

        static member inline Sum (x:ISliceData<_,_>) : Flips.UnitsOfMeasure.Types.LinearExpression<_> =
          TryFind.sum x.Keys x.TryFind

        static member Sum (x:ISliceData<_,Flips.UnitsOfMeasure.Types.Decision<_>>) : Flips.UnitsOfMeasure.Types.LinearExpression<_> =
          let newTryFind = x.TryFind >> Option.map (fun v -> 1.0 * v)
          TryFind.sum x.Keys newTryFind


    /// <summary>A function which sums the values contained in a SliceMap</summary>
    /// <param name="x">An instance of ISliceData</param>
    /// <returns>A LinearExpression with a Unit of Measure</returns
    let inline sum (x:ISliceData<'Key, 'Value>) : Flips.UnitsOfMeasure.Types.LinearExpression<_> =
        TryFind.sum x.Keys x.TryFind

    /// <summary>A function which sums a sequence of SliceMaps</summary>
    /// <param name="x">A sequence of SliceMaps</param>
    /// <returns>A LinearExpression with a Unit of Measure</returns
    let inline sumAll< ^a, ^b when ^a: (static member Sum: ^a -> Flips.Types.LinearExpression) 
                              and ^a: (static member (+): ^a * ^a -> ^a)
                              and ^a: (static member Zero: ^a)> (k1: ^a seq) : Flips.Types.LinearExpression = 
        let r = Seq.sum k1
        ((^a) : (static member Sum: ^a -> Flips.Types.LinearExpression) r)