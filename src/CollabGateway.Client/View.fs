module CollabGateway.Client.View

open Feliz
open Router
open Elmish
open Feliz.UseElmish
open Fable.FontAwesome

type private Msg =
    | UrlChanged of Page

type private State = {
    Page : Page
}

let private init () =
    let nextPage = Router.currentPath() |> Page.parseFromUrlSegments
    { Page = nextPage }, Cmd.navigatePage nextPage

let private update (msg:Msg) (state:State) : State * Cmd<Msg> =
    match msg with
    | UrlChanged page -> { state with Page = page }, Cmd.none

[<ReactComponent>]
let AppView () =
    let state, dispatch = React.useElmish(init, update)

    let (isOpen, setIsOpen) = React.useState false
    let toggleSidebar () = setIsOpen (not isOpen)

    let (theme, setTheme) = React.useState "dark"
    React.useEffectOnce(fun () ->
        let html = Browser.Dom.document.documentElement
        html.setAttribute("data-theme", "dark")
    )

    let toggleTheme () =
        let html = Browser.Dom.document.documentElement
        let currentTheme = html.getAttribute("data-theme")
        let newTheme =
            match currentTheme with
            | "fantasy" -> "dark"
            | "dark" -> "fantasy"
            | _ -> "fantasy"
        html.setAttribute("data-theme", newTheme)
        setTheme newTheme

    let render =
        match state.Page with
        | Page.Index -> Pages.Index.IndexView ()
        | Page.About -> Html.text "Rower & SpeakEZ Gateway"

    let navigationWrapper =
        Html.div [
            prop.className "flex flex-col h-screen"
            prop.children [
                // Top nav bar
                Html.div [
                    prop.className "p-4 flex items-center justify-between fixed top-0 left-0 w-full z-10"
                    prop.children [
                        Html.div [
                            prop.className "cursor-pointer flex items-center transition-all duration-300 ease-in-out"
                            prop.onClick (fun _ -> toggleSidebar())
                            prop.children [
                                Html.img [
                                    prop.src "/img/Rower_Icon_Gold_t.svg" 
                                    prop.alt "Toggle Sidebar"
                                    prop.style [
                                        style.width (length.px 24) 
                                        style.height (length.px 24) 
                                    ]
                                ]
                                Html.h1 [
                                    prop.className "ml-2 text-lg font-semibold"
                                    prop.text "Collab Gateway"
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "flex items-center"
                            prop.children [
                                Html.i [
                                    prop.className "cursor-pointer"
                                    prop.onClick (fun _ -> toggleTheme())
                                    prop.children [
                                        if theme = "dark" then
                                            Fa.i [ Fa.Solid.Sun ] []
                                        else
                                            Fa.i [ Fa.Solid.Moon ] []
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
                // Sidebar and main content wrapper
                Html.div [
                    prop.className "flex flex-1 pt-16" // Add padding-top to account for the fixed top nav bar
                    prop.children [
                        // Sidebar
                        Html.div [
                            prop.className (sprintf "transition-all duration-300 ease-in-out %s" (if isOpen then "w-64" else "w-0"))
                            prop.style [ style.width (if isOpen then length.rem 16 else length.rem 0) ]
                            prop.children [
                                // Sidebar content here
                                Html.ul [
                                    prop.className (sprintf "menu p-4 min-h-full bg-base-200 text-base-content text-lg font-semibold transition-opacity duration-300 ease-in-out %s" (if isOpen then "opacity-100" else "opacity-0"))
                                    prop.children [
                                        Html.li [
                                            prop.className "text-primary"
                                            prop.children [ Html.a("Home", Page.Index) ]
                                        ]
                                        Html.li [
                                            prop.content "About"
                                            prop.children [ Html.a("About", Page.About) ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        // Main content area
                        Html.div [
                            prop.className "flex flex-col flex-1 overflow-hidden transition-all duration-300 ease-in-out"
                            prop.children [
                                Html.div [
                                    prop.className "flex-1 overflow-auto p-4"
                                    prop.children [ render ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

    React.router [
        router.pathMode
        router.onUrlChanged (Page.parseFromUrlSegments >> UrlChanged >> dispatch)
        router.children [ navigationWrapper ]
    ]