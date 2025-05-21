namespace OrleansSimplestThing.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open OrleansSimplestThing
//open OrleansSimplestThing.OrleansStuff
open Orleans

[<ApiController>]
[<Route("[controller]")>]
type WeatherForecastController(log: ILogger<WeatherForecastController>, grainFactory: IGrainFactory) =
    inherit ControllerBase()

    let summaries =
        [| "Freezing"
           "Bracing"
           "Chilly"
           "Cool"
           "Mild"
           "Warm"
           "Balmy"
           "Hot"
           "Sweltering"
           "Scorching" |]

    [<HttpGet>]
    member _.Get() =
        let rng = Random.Shared

        [| for index in 0..4 ->
               { Date = DateTime.Now.AddDays(float index)
                 TemperatureC = rng.Next(-20, 55)
                 Summary = summaries.[rng.Next(summaries.Length)] } |]
