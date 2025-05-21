namespace OrleansSimplestThing

open System.Runtime.CompilerServices
open Orleans.Serialization.Configuration

#nowarn "20"

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Orleans
open Orleans.Configuration
open Orleans.Hosting
open Orleans.Runtime
open Orleans.Serialization
open System.Text.Json
open System.Text.Json.Serialization

[<assembly: Orleans.ApplicationPartAttribute("OrleansCodegen")>]

[<InternalsVisibleTo("Host")>]
do ()

module Program =
    let exitCode = 0

    /// The universal serialization options for F#-specific data types in Grace.
    ///
    /// See https://github.com/Tarmil/FSharp.SystemTextJson/blob/master/docs/Customizing.md for more information about these options.
    let private jsonFSharpOptions =
        JsonFSharpOptions
            .Default()
            .WithAllowNullFields(true)
            .WithUnionFieldsName("value")
            .WithUnionTagNamingPolicy(JsonNamingPolicy.CamelCase)
            .WithUnionTagCaseInsensitive(true)
            .WithUnionEncoding(
                JsonUnionEncoding.ExternalTag
                ||| JsonUnionEncoding.UnwrapFieldlessTags
                ||| JsonUnionEncoding.UnwrapSingleFieldCases
                ||| JsonUnionEncoding.UnwrapSingleCaseUnions
                ||| JsonUnionEncoding.NamedFields
            )
            .WithUnwrapOption(true)

    /// The universal JSON serialization options for Grace.
    let public JsonSerializerOptions = JsonSerializerOptions()
    JsonSerializerOptions.Converters.Add(JsonFSharpConverter(jsonFSharpOptions))
    JsonSerializerOptions.Converters.Add(JsonStringEnumConverter(JsonNamingPolicy.CamelCase))
    JsonSerializerOptions.AllowTrailingCommas <- true
    JsonSerializerOptions.DefaultBufferSize <- 64 * 1024
    JsonSerializerOptions.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingDefault // JsonSerializerOptions.IgnoreNullValues is deprecated. This is the new way to say it.
    JsonSerializerOptions.IndentSize <- 2
    JsonSerializerOptions.MaxDepth <- 64 // Default is 64, and I'm assuming this setting would need to change if there were a directory depth greater than 64 in a repo.
    JsonSerializerOptions.NumberHandling <- JsonNumberHandling.AllowReadingFromString
    JsonSerializerOptions.PropertyNameCaseInsensitive <- true // Case sensitivity is from the 1970's. We should let it go.
    //JsonSerializerOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    JsonSerializerOptions.ReadCommentHandling <- JsonCommentHandling.Skip
    JsonSerializerOptions.ReferenceHandler <- ReferenceHandler.IgnoreCycles
    JsonSerializerOptions.RespectNullableAnnotations <- true
    JsonSerializerOptions.UnknownTypeHandling <- JsonUnknownTypeHandling.JsonElement
    JsonSerializerOptions.WriteIndented <- true


    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)

        builder.Services.AddControllers()

        builder.Host.UseOrleans(fun siloBuilder ->
            siloBuilder.UseLocalhostClustering().AddMemoryGrainStorageAsDefault()

            siloBuilder.Services.AddSerializer(fun serializerBuilder ->
                serializerBuilder
                    .AddJsonSerializer(
                        isSupported = (fun t -> true),
                        //isSupported =
                        //    (fun t ->
                        //        not <| String.IsNullOrEmpty(t.Namespace)
                        //        && t.Namespace.StartsWith("Grains", StringComparison.InvariantCulture)
                        //        || t.Namespace.StartsWith("OrleansCodegen", StringComparison.InvariantCulture)),
                        jsonSerializerOptions = JsonSerializerOptions
                    )
                    .Configure(fun (xx: TypeManifestOptions) ->
                        xx.AllowAllTypes <- true
                        xx.EnableConfigurationAnalysis <- true)
                |> ignore)

            |> ignore)

        let app = builder.Build()

        app.UseHttpsRedirection()

        app.UseAuthorization()
        app.MapControllers()

        let (analyzer: SerializerConfigurationAnalyzer) =
            app.Services.GetService(typeof<SerializerConfigurationAnalyzer>) :?> SerializerConfigurationAnalyzer

        app.Run()

        exitCode
