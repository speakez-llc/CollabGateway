﻿module CollabGateway.Client.Pages.Overview

open System
open Feliz
open Feliz.Recharts
open Feliz.PigeonMaps
open Elmish
open CollabGateway.Client.Server
open CollabGateway.Shared.API
open CollabGateway.Client.ViewMsg
open UseElmish

type State = {
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

let init () =
    let initialState = {
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
                    renderStatItem "User Streams" overviewTotals.OverviewTotals.TotalUserStreams "Total number of user streams"
                    renderStatItem "Data Policy Declines" overviewTotals.OverviewTotals.TotalDataPolicyDeclines "Total data policy declines"
                    renderStatItem "Contact Forms Used" overviewTotals.OverviewTotals.TotalContactFormsUsed "Total contact forms used"
                ]
            ]
            Html.div [
                prop.className "stats stats-vertical lg:stats-horizontal shadow"
                prop.children [
                    renderStatItem "Smart Forms" overviewTotals.OverviewTotals.TotalSmartFormUsers "Total smart form users"
                    renderStatItem "Sign Ups" overviewTotals.OverviewTotals.TotalSignUpFormsUsed "Total sign up forms used"
                    renderStatItem "Email Verifications" overviewTotals.OverviewTotals.TotalEmailVerifications "Total email verifications"
                    renderStatItem "Email Unsubscribes" overviewTotals.OverviewTotals.TotalEmailUnsubscribes "Total email unsubscribes"
                ]
            ]
        ]
    ]

type TooltipPayload = {
    name: string
    value: int
}

type OverviewDataPoint = {
    name: string
    userStreams: int
    dataPolicyDeclines: int
    contactFormsUsed: int
    smartFormUsers: int
    signUpFormsUsed: int
    emailVerifications: int
    emailUnsubscribes: int
}

let customTooltip (props: {| active: bool; payload: obj list; label: string |}) =
    if props.active && not (List.isEmpty props.payload) then
        Html.div [
            prop.className "custom-tooltip"
            prop.children [
                Html.p [
                    prop.className "label"
                    prop.text $"{props.label}"
                ]
                for item in props.payload do
                    let payloadItem = item :?> TooltipPayload
                    Html.p [
                        prop.className "intro"
                        prop.text $"{payloadItem.name} : {payloadItem.value}"
                    ]
                Html.p [
                    prop.className "desc"
                    prop.text "Additional description here."
                ]
            ]
        ]
    else
        Html.none

[<ReactComponent>]
let OverviewLineChart (overviewSeries: OverviewTotalsProjection list) =
    let data =
        overviewSeries
        |> List.map (fun projection ->
            {
                name = projection.IntervalEnd |> Option.map (_.ToString("MM/dd")) |> Option.defaultValue ""
                userStreams = projection.OverviewTotals.TotalUserStreams
                dataPolicyDeclines = projection.OverviewTotals.TotalDataPolicyDeclines
                contactFormsUsed = projection.OverviewTotals.TotalContactFormsUsed
                smartFormUsers = projection.OverviewTotals.TotalSmartFormUsers
                signUpFormsUsed = projection.OverviewTotals.TotalSignUpFormsUsed
                emailVerifications = projection.OverviewTotals.TotalEmailVerifications
                emailUnsubscribes = projection.OverviewTotals.TotalEmailUnsubscribes
            })

    let responsiveChart =
        Recharts.lineChart [
            lineChart.data data
            lineChart.margin(top=5, right=30)
            lineChart.children [
                Recharts.cartesianGrid [ cartesianGrid.strokeDasharray(3, 3) ]
                Recharts.xAxis [ xAxis.dataKey (fun point -> point.name) ]
                Recharts.yAxis [ ]
                Recharts.tooltip [
                ]
                Recharts.legend [
                ]
                Recharts.line [
                    line.monotone
                    line.dataKey (fun point -> point.userStreams)
                    line.name "User Streams"
                    line.stroke "#8884d8"
                ]
                Recharts.line [
                    line.monotone
                    line.dataKey (fun point -> point.dataPolicyDeclines)
                    line.name "Data Policy Declines"
                    line.stroke "#82ca9d"
                ]
                Recharts.line [
                    line.monotone
                    line.dataKey (fun point -> point.contactFormsUsed)
                    line.name "Contact Forms Used"
                    line.stroke "#ff7300"
                ]
                Recharts.line [
                    line.monotone
                    line.dataKey (fun point -> point.smartFormUsers)
                    line.name "Smart Form Users"
                    line.stroke "#387908"
                ]
                Recharts.line [
                    line.monotone
                    line.dataKey (fun point -> point.signUpFormsUsed)
                    line.name "Sign Up Forms Used"
                    line.stroke "#ff0000"
                ]
                Recharts.line [
                    line.monotone
                    line.dataKey (fun point -> point.emailVerifications)
                    line.name "Email Verifications"
                    line.stroke "#0000ff"
                ]
                Recharts.line [
                    line.monotone
                    line.dataKey (fun point -> point.emailUnsubscribes)
                    line.name "Email Unsubscribes"
                    line.stroke "#00ff00"
                ]
            ]
        ]

    Recharts.responsiveContainer [
        responsiveContainer.width (length.percent 100)
        responsiveContainer.height 300
        responsiveContainer.chart responsiveChart
    ]

type City = {
    Name: string
    Latitude: float
    Longitude: float
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
                prop.className [ "fa"; "fa-location-dot"; "fa-2x" ]
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
                        style.backgroundColor.black
                        style.color.lightGreen
                        style.textAlign.center
                        style.borderRadius 5
                        style.padding 10
                        style.position.absolute
                        style.zIndex 1
                        style.marginLeft -60
                    ]
                    prop.text marker.City.Name
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
    let cities = geoInfo |> List.map (fun (name, lat, lng, _) -> { Name = name; Latitude = lat; Longitude = lng })
    let initialCenter =
        cities
        |> List.tryHead
        |> Option.map (fun city -> city.Latitude, city.Longitude)
        |> Option.defaultValue (33.7490, -84.3880)

    let (zoom, setZoom) = React.useState 8
    let (center, setCenter) = React.useState initialCenter

    PigeonMaps.map [
        map.center center
        map.zoom zoom
        map.height 350
        map.onBoundsChanged (fun args -> setZoom (int args.zoom); setCenter args.center)
        map.markers [ for city in cities -> renderMarker city ]
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
                                prop.className "text-center"
                                prop.text "No data available."
                            ]
                        else
                            Html.div [
                                prop.className "space-y-24"
                                prop.children [
                                   OverviewStats state.OverviewTotals[0]
                                   OverviewLineChart state.OverviewSeries
                                   GeoMap state.GeoInfo
                                ]
                            ]
                ]
            ]
        ]
    ]