namespace OrleansSimplestThing.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Grains.Simple
open Orleans
open System.Threading.Tasks

[<ApiController>]
[<Route("math")>]
type MathController(log: ILogger<MathController>, grainFactory: IGrainFactory) =
    inherit ControllerBase()

    [<HttpPost("add")>]
    member _.Add([<FromBody>] x: int64) =
        task {
            log.LogInformation("About to call grain with value {x}.", x)
            let grain = grainFactory.GetGrain<ISimpleGrain>(26L)
            let! result = grain.Add(x)
            log.LogInformation("Called grain with value {x}; result: {result}.", x, result)
            return result
        }

    [<HttpPost("subtract")>]
    member _.Subtract([<FromBody>] x: int64) =
        task {
            log.LogInformation("About to call grain with value {x}.", x)
            let grain = grainFactory.GetGrain<ISimpleGrain>(26L)
            let! result = grain.Subtract(x)
            log.LogInformation("Called grain with value {x}; result: {result}.", x, result)
            return result
        }

    [<HttpPost("useDiscriminatedUnion")>]
    member _.UseDiscriminatedUnion() =
        task {
            let command = Add 23L
            log.LogInformation("About to call grain with command {command}.", command)
            let grain = grainFactory.GetGrain<ISimpleGrain>(26L)
            let! result = grain.UseDiscriminatedUnion(command)
            log.LogInformation("Called grain with command {command}; result: {result}.", command, result)
            return result
        }
