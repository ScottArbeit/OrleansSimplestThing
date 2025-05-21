namespace Grains

open Orleans
open System.Threading.Tasks
open Orleans.Runtime
open System.Runtime.Serialization
open System.Reflection
open Microsoft.FSharp.Reflection
open Microsoft.Extensions.Logging

module Simple =
    /// Gets the cases of discriminated union for serialization.
    let GetKnownTypes<'T> () =
        typeof<'T>.GetNestedTypes(BindingFlags.Public ||| BindingFlags.NonPublic)
        |> Array.filter FSharpType.IsUnion

    let log =
        LoggerFactory
            .Create(fun builder ->
                builder.AddConsole() |> ignore
                builder.SetMinimumLevel(LogLevel.Debug) |> ignore)
            .CreateLogger("SimpleGrain")

    [<KnownType("GetKnownTypes")>]
    [<GenerateSerializer>]
    type SimpleGrainCommand =
        | Add of int64
        | Subtract of int64
        | Multiply of int64
        | Divide of float

        static member GetKnownTypes() = GetKnownTypes<SimpleGrainCommand>()

    type ISimpleGrain =
        inherit IGrainWithIntegerKey
        abstract member Add: int64 -> Task<int64>
        abstract member Subtract: int64 -> Task<int64>
        abstract member UseDiscriminatedUnion: SimpleGrainCommand -> Task<int64>

    [<GenerateSerializer>]
    type SimpleGrain([<PersistentState("SimpleGrainState", "Default")>] state: IPersistentState<int64>) =
        inherit Grain()

        member val private currentValue = 0L with get, set

        interface ISimpleGrain with
            member this.Add x =
                task {
                    this.currentValue <- this.currentValue + x
                    state.State <- this.currentValue
                    do! state.WriteStateAsync()
                    log.LogInformation("Add called with {X}. CurrentValue: {currentValue}", x, this.currentValue)
                    return this.currentValue
                }

            member this.Subtract x =
                task {
                    this.currentValue <- this.currentValue - x
                    state.State <- this.currentValue
                    do! state.WriteStateAsync()
                    log.LogInformation("Subtract called with {X}. CurrentValue: {currentValue}", x, this.currentValue)
                    return this.currentValue
                }

            member this.UseDiscriminatedUnion(simpleGrainCommand: SimpleGrainCommand) : Task<int64> =
                task {
                    log.LogInformation("UseDiscriminatedUnion called with command: {Command}.", simpleGrainCommand)
                    return this.currentValue
                }
