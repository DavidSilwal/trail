namespace Library1

open System
open BlazorRedux

type WeatherForecast() =
    member val Date = DateTime.MinValue with get, set
    member val TemperatureC = 0 with get, set
    member val TemperatureF = 0 with get, set
    member val Summary = "" with get, set

type MyModel = {
    Location : string
    Count : int
    Forecasts : WeatherForecast[] option
}

type MyMsg =
    | IncrementByOne
    | IncrementByValue of n : int
    | ClearWeather
    | ReceiveWeather of r : WeatherForecast[]

module ActionCreators =
    open System.Net.Http
    open System.Threading.Tasks
    open FSharp.Control.Tasks
    open Microsoft.AspNetCore.Blazor

    let LoadWeather (dispatch: Dispatcher<MyMsg>, http: HttpClient) =
        task {
            dispatch.Invoke(MyMsg.ClearWeather) |> ignore
            let! forecasts = http.GetJsonAsync<WeatherForecast[]>("/sample-data/weather.json")
            dispatch.Invoke(MyMsg.ReceiveWeather forecasts) |> ignore
        } :> Task

module Store =
    [<CompiledName("Reducer")>]
    let reducer state action =
        match action with
        | IncrementByOne -> { state with Count = state.Count + 1 }
        | IncrementByValue n -> { state with Count = state.Count + n }
        | ClearWeather -> { state with Forecasts = None }
        | ReceiveWeather f -> { state with Forecasts = Some f }
    
    [<CompiledName("LocationReducer")>]
    let locationReducer state (action:LocationAction) =
        match action with
        | :? NewLocationAction as a -> { state with Location = a.Location }
        | _ -> state

[<AbstractClass>]
type MyAppComponent() =
    inherit Trail.ReduxComponent<MyModel, MyMsg>()
