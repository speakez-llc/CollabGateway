module CollabGateway.Client.Pages.Overview

open System
open Feliz
open Feliz.Recharts
open Feliz.PigeonMaps
open Elmish
open CollabGateway.Client.Server
open CollabGateway.Shared.API
open CollabGateway.Client.ViewMsg
open Feliz.Recharts.legend
open UseElmish

type State = {
    Grain: Grain
    OverviewTotals: OverviewTotalsProjection list
    OverviewSeries: OverviewTotalsProjection list
    GeoInfo: (string * float * float * int) list
}

type Msg =
    | FetchOverviewTotals
    | OverviewTotalsReceived of OverviewTotalsProjection list
    | FetchOverviewTotalsFailed of exn
    | OverviewSeriesReceived of OverviewTotalsProjection list
    | FetchOverviewSeriesFailed of exn
    | GeoInfoReceived of (string * float * float * int) list
    | FetchGeoInfoFailed of exn
    | SetOverviewPeriod of (int * Grain)

let init () =
    let initialState = {
        Grain = Grain.Day
        OverviewTotals = []
        OverviewSeries = []
        GeoInfo = []
    }

    let fetchOverviewTotalsCmd =
        Cmd.OfAsync.perform (fun () -> service.RetrieveOverviewTotals None) () OverviewTotalsReceived

    let fetchOverviewSeriesCmd =
        Cmd.OfAsync.perform (fun () -> service.RetrieveOverviewTotals (Some (7, Grain.Day))) () OverviewSeriesReceived

    let fetchGeoInfoCmd =
        Cmd.OfAsync.perform (fun () -> service.RetrieveClientIPLocations ()) () GeoInfoReceived

    initialState, Cmd.batch [fetchOverviewTotalsCmd; fetchOverviewSeriesCmd; fetchGeoInfoCmd]

let update (msg: Msg) (model: State) : State * Cmd<Msg> =
    match msg with
    | FetchOverviewTotals ->
        model, Cmd.OfAsync.perform (fun () -> service.RetrieveOverviewTotals None) () OverviewTotalsReceived
    | OverviewTotalsReceived overview ->
        Console.WriteLine overview
        { model with OverviewTotals = overview }, Cmd.none
    | FetchOverviewTotalsFailed _ ->
        model, Cmd.none
    | OverviewSeriesReceived series ->
        { model with OverviewSeries = series }, Cmd.none
    | FetchOverviewSeriesFailed _ ->
        model, Cmd.none
    | GeoInfoReceived geoInfo ->
        { model with GeoInfo = geoInfo }, Cmd.none
    | FetchGeoInfoFailed _ ->
        model, Cmd.none
    | SetOverviewPeriod (period, grain) ->
        { model with Grain = grain }, Cmd.OfAsync.perform (fun () -> service.RetrieveOverviewTotals (Some (period, grain))) () OverviewSeriesReceived

let renderStatItem (title: string) (value: int) (description: string) =
    Html.div [
        prop.className "stat"
        prop.children [
            Html.div [
                prop.className "stat-title text-center"
                prop.text title
            ]
            Html.div [
                prop.className "stat-value text-gold text-center"
                prop.text value
            ]
            Html.div [
                prop.className "stat-desc text-center"
                prop.text description
            ]
        ]
    ]

[<ReactComponent>]
let OverviewStats (overviewTotals: OverviewTotalsProjection) =
    Html.div [
        prop.className "flex flex-col items-center space-y-14"
        prop.children [
            Html.div [
                prop.className "stats stats-vertical lg:stats-horizontal shadow"
                prop.children [
                    renderStatItem "New Users" overviewTotals.OverviewTotals.TotalNewUserStreams "Total number of user streams"
                    renderStatItem "Sign Ups Sent" overviewTotals.OverviewTotals.TotalSignUpFormsUsed "Total sign up forms used"
                    renderStatItem "Smart Form Used" overviewTotals.OverviewTotals.TotalSmartFormUsers "Total smart form \n sent per user"
                    renderStatItem "Contact Sent" overviewTotals.OverviewTotals.TotalContactFormsUsed "Total contact forms used"
                ]
            ]
            Html.div [
                prop.className "stats stats-vertical lg:stats-horizontal shadow"
                prop.children [
                    renderStatItem "Email Verifications" overviewTotals.OverviewTotals.TotalEmailVerifications "Total email verifications"
                    renderStatItem "Email Unsubscribes" overviewTotals.OverviewTotals.TotalEmailUnsubscribes "Total email unsubscribes"
                    renderStatItem "Smart Form Limit" overviewTotals.OverviewTotals.TotalUsersWhoReachedSmartFormLimit "Limit of 5 attempts max"
                    renderStatItem "Policy Declines" overviewTotals.OverviewTotals.TotalDataPolicyDeclines "Total data policy declines"
                ]
            ]
        ]
    ]
type OverviewDataPoint = {
    name: string
    userStreams: int
    dataPolicyDeclines: int
    contactFormsUsed: int
    smartFormUsers: int
    signUpFormsUsed: int
    emailVerifications: int
    emailUnsubscribes: int
    usersSmartFormLimit: int
}

[<ReactComponent>]
let OverviewLineChart (overviewSeries: OverviewTotalsProjection list, grain: Grain, dispatch: Msg -> unit) =
    let data =
        overviewSeries
        |> List.map (fun projection ->
            {
                name =
                    match projection.IntervalEnd with
                    | Some date when grain = Grain.Hour -> date.ToLocalTime().ToString("HH:mm")
                    | Some date -> date.ToString("MM/dd")
                    | None -> ""
                userStreams = projection.OverviewTotals.TotalNewUserStreams
                dataPolicyDeclines = projection.OverviewTotals.TotalDataPolicyDeclines
                contactFormsUsed = projection.OverviewTotals.TotalContactFormsUsed
                smartFormUsers = projection.OverviewTotals.TotalSmartFormUsers
                signUpFormsUsed = projection.OverviewTotals.TotalSignUpFormsUsed
                emailVerifications = projection.OverviewTotals.TotalEmailVerifications
                emailUnsubscribes = projection.OverviewTotals.TotalEmailUnsubscribes
                usersSmartFormLimit = projection.OverviewTotals.TotalUsersWhoReachedSmartFormLimit
            })

    let handleRadioChange (e: Browser.Types.Event) =
        let target = e.target :?> Browser.Types.HTMLInputElement
        match target.value with
        | "1 Day (hourly)" -> dispatch (SetOverviewPeriod (24, Grain.Hour))
        | "1 Week (daily)" -> dispatch (SetOverviewPeriod (7, Grain.Day))
        | "1 Month (weekly)" -> dispatch (SetOverviewPeriod (4, Grain.Week))
        | "1 Month (daily)" -> dispatch (SetOverviewPeriod (30, Grain.Day))
        | _ -> ()

    let tickMargin =
        match grain with
        | Grain.Hour -> 15
        | _ -> 10

    let responsiveChart =
        Recharts.lineChart [
            lineChart.data data
            lineChart.margin(top=10, right=50, bottom=50)
            lineChart.children [
                Recharts.cartesianGrid [ cartesianGrid.strokeDasharray(3, 3) ]
                Recharts.xAxis [
                    xAxis.dataKey (_.name)
                    xAxis.angle -45
                    xAxis.padding 4
                    xAxis.tickMargin tickMargin
                ]
                Recharts.yAxis [ ]
                Recharts.tooltip [
                    tooltip.formatter (fun (value: obj) _ _ ->
                        match value with
                        | :? int as intValue -> sprintf "%d" intValue
                        | _ -> ""
                    )
                    tooltip.labelStyle [ style.color.black; style.backgroundColor.whiteSmoke; style.paddingBottom 5 ]
                    tooltip.wrapperStyle [ style.border(8, borderStyle.solid, "white")
                                           style.borderRadius 8 ]
                ]
                Recharts.legend [
                    verticalAlign.top
                    legend.wrapperStyle [ style.paddingBottom 10 ]
                ]
                Recharts.line [
                    line.monotone
                    line.dataKey (_.userStreams)
                    line.name "New Users"
                    line.stroke "steelblue"
                ]
                Recharts.line [
                    line.monotone
                    line.dataKey (_.signUpFormsUsed)
                    line.name "Sign Up Forms Sent"
                    line.stroke "teal"
                    line.strokeWidth 4
                ]
                Recharts.line [
                    line.monotone
                    line.dataKey (_.smartFormUsers)
                    line.name "Smart Form Used"
                    line.stroke "orangered"
                ]
                Recharts.line [
                    line.monotone
                    line.dataKey (_.contactFormsUsed)
                    line.name "Contact Forms Sent"
                    line.stroke "darkgreen"
                ]
                Recharts.line [
                    line.monotone
                    line.dataKey (_.emailVerifications)
                    line.name "Email Verifications"
                    line.stroke "teal"
                ]
                Recharts.line [
                    line.monotone
                    line.dataKey (_.emailUnsubscribes)
                    line.name "Email Unsubscribes"
                    line.stroke "MediumVioletRed"
                ]
                Recharts.line [
                    line.monotone
                    line.dataKey (_.usersSmartFormLimit)
                    line.name "Smart Form Limit"
                    line.stroke "darkred"
                    line.strokeWidth 2
                ]
                Recharts.line [
                    line.monotone
                    line.dataKey (_.dataPolicyDeclines)
                    line.name "Data Policy Declines"
                    line.stroke "blueviolet"
                ]
            ]
        ]

    Html.div [
        prop.className "pb-4"
        prop.children [
            Html.div [
                prop.className "radio-buttons space-x-2 pb-4"
                prop.children [
                    Html.label [
                        prop.className "space-x-1"
                        prop.children [
                            Html.input [
                                prop.type' "radio"
                                prop.name "overviewPeriod"
                                prop.value "1 Day (hourly)"
                                prop.onChange handleRadioChange
                            ]
                            Html.span "1 Day (hourly)"
                        ]
                    ]
                    Html.label [
                        prop.className "space-x-1"
                        prop.children [
                            Html.input [
                                prop.type' "radio"
                                prop.name "overviewPeriod"
                                prop.value "1 Week (daily)"
                                prop.defaultChecked true
                                prop.onChange handleRadioChange
                            ]
                            Html.span "1 Week (daily)"
                        ]
                    ]
                    Html.label [
                        prop.className "space-x-1"
                        prop.children [
                            Html.input [
                                prop.type' "radio"
                                prop.name "overviewPeriod"
                                prop.value "1 Month (weekly)"
                                prop.onChange handleRadioChange
                            ]
                            Html.span "1 Month (weekly)"
                        ]
                    ]
                    Html.label [
                        prop.className "space-x-1"
                        prop.children [
                            Html.input [
                                prop.type' "radio"
                                prop.name "overviewPeriod"
                                prop.value "1 Month (daily)"
                                prop.onChange handleRadioChange
                            ]
                            Html.span "1 Month (daily)"
                        ]
                    ]
                ]
            ]
            Recharts.responsiveContainer [
                responsiveContainer.width (length.percent 100)
                responsiveContainer.height 400
                responsiveContainer.chart responsiveChart
            ]
        ]
    ]

type City = {
    Name: string
    Latitude: float
    Longitude: float
    UserCount: int
}

type MarkerProps = {
    City: City
    Hovered: bool
}

let markerWithPopover (marker: MarkerProps)  =
    Html.div [
        prop.style [
            style.position.relative
            style.display.inlineBlock
        ]
        prop.children [
            Html.i [
                prop.key marker.City.Name
                prop.className [ "fa"; "fa-map-pin" ]
                prop.style [
                    style.color.darkBlue
                ]
                if marker.Hovered then prop.style [
                    style.cursor.pointer
                    style.color.darkOrange
                ]
            ]
            if marker.Hovered then
                Html.div [
                    prop.style [
                        style.visibility.visible
                        style.backgroundColor.dimGray
                        style.color.lightGreen
                        style.textAlign.center
                        style.borderRadius 5
                        style.padding 10
                        style.position.absolute
                        style.marginLeft -60
                    ]
                    prop.text (sprintf "%s (%d)" marker.City.Name marker.City.UserCount)
                ]
        ]
    ]

let renderMarker (city: City) =
    PigeonMaps.marker [
        marker.anchor(city.Latitude, city.Longitude)
        marker.offsetLeft 15
        marker.offsetTop 30
        marker.render (fun marker -> [
            markerWithPopover {
                City = city
                Hovered = marker.hovered
            }
        ])
    ]

[<ReactComponent>]
let GeoMap (geoInfo: (string * float * float * int) list) =
    let cities = geoInfo |> List.map (fun (name, lat, lng, userCount) -> { Name = name; Latitude = lat; Longitude = lng; UserCount = userCount })
    let initialCenter = (39.8283, -98.5795)

    let zoom, setZoom = React.useState 3
    let center, setCenter = React.useState initialCenter

    Html.div [
        prop.style [
            style.borderRadius 12
            style.overflow.hidden
        ]
        prop.children [
            PigeonMaps.map [
                map.center center
                map.zoom zoom
                map.height 500
                map.zoomSnap true
                map.onBoundsChanged (fun args -> setZoom (int args.zoom); setCenter args.center)
                map.markers [ for city in cities -> renderMarker city ]
            ]
        ]
    ]

[<ReactComponent>]
let IndexView (isAdmin: bool, parentDispatch: ViewMsg -> unit) =
    let state, dispatch = React.useElmish(init, update, [| |])

    Html.div [
        prop.className "flex justify-center items-center"
        prop.children [
            Html.div [
                prop.className "overview-page w-full max-w-4xl"
                prop.children [
                    Html.h1 [
                        prop.className "page-title text-center text-2xl font-bold my-4"
                        prop.text "Overview"
                    ]
                    if isAdmin then
                        if List.isEmpty state.OverviewTotals then
                            Html.div [
                                prop.className "flex items-center space-x-2 justify-center"
                                prop.children [
                                    Html.div [
                                        prop.className "loading loading-dots loading-xl text-warning"
                                    ]
                                    Html.span [
                                        prop.className "text-warning text-xl"
                                        prop.text "Loading..."
                                    ]
                                ]
                            ]
                        else
                        Html.div [
                            prop.className "space-y-24"
                            prop.children [
                               OverviewStats state.OverviewTotals[0]
                               OverviewLineChart (state.OverviewSeries, state.Grain, dispatch)
                               GeoMap state.GeoInfo
                            ]
                        ]
                ]
            ]
        ]
    ]