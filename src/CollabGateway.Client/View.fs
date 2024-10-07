module CollabGateway.Client.View

open Feliz
open Router
open Elmish
open Feliz.UseElmish
open Fable.FontAwesome
open Fable.Core.JsInterop

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

    let isMobileView () = Browser.Dom.window.innerWidth < 768.0

    let initialSidebarState =
        match Browser.Dom.window.localStorage.getItem("sidebarState") with
        | null -> not (isMobileView())
        | value -> value = "open"

    let (isOpen, setIsOpen) = React.useState initialSidebarState

    let toggleSidebar () =
        let newState = not isOpen
        setIsOpen newState
        Browser.Dom.window.localStorage.setItem("sidebarState", if newState then "open" else "closed")

    let initialTheme =
        match Browser.Dom.window.localStorage.getItem("theme") with
        | null -> "dark"
        | value -> value

    let (theme, setTheme) = React.useState initialTheme
    React.useEffectOnce(fun () ->
        let html = Browser.Dom.document.documentElement
        html.setAttribute("data-theme", initialTheme)
    )

    let toggleTheme () =
        let html = Browser.Dom.document.documentElement
        let currentTheme = html.getAttribute("data-theme")
        let newTheme =
            match currentTheme with
            | "nord" -> "business"
            | "business" -> "nord"
            | _ -> "nord"
        html.setAttribute("data-theme", newTheme)
        setTheme newTheme
        Browser.Dom.window.localStorage.setItem("theme", newTheme)

    let handleItemClick () =
        if isMobileView() then
            setIsOpen false

    let render =
        match state.Page with
        | Page.Index -> Pages.Index.IndexView ()
        | Page.Project -> Pages.Project.IndexView ()
        | Page.CMSData -> Pages.CMSData.IndexView ()
        | Page.SignUp -> Pages.SignUp.IndexView ()
        | Page.Rower -> Pages.Rower.IndexView ()
        | Page.SpeakEZ -> Pages.SpeakEZ.IndexView ()
        | Page.Contact -> Pages.Contact.IndexView ()
        | Page.Partners -> Pages.Partners.IndexView ()

    let navigationWrapper =
        Html.div [
            prop.className "flex flex-col h-screen"
            prop.children [
                // Top nav bar
                Html.div [
                    prop.className "p-5 flex items-center justify-between fixed top-0 left-0 w-full z-20 bg-base-200"
                    prop.children [
                        Html.div [
                            prop.className "flex items-center transition-all duration-500 ease-in-out ml-2"
                            prop.children [
                                // Hamburger icon
                                Html.div [
                                    prop.className "cursor-pointer mr-2"
                                    prop.onClick (fun _ -> toggleSidebar())
                                    prop.children [
                                        Fa.i [ Fa.Solid.Bars ] []
                                    ]
                                ]
                                Html.div [
                                    prop.className "cursor-pointer flex items-center transition-all duration-500 ease-in-out ml-4"
                                    prop.onClick (fun _ -> toggleSidebar())
                                    prop.children [
                                        Html.img [
                                            prop.src "/img/Rower_Icon_Gold_t.svg"
                                            prop.alt "Sidebar Menu Control"
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
                            ]
                        ]
                        Html.div [
                            prop.className "flex items-center mr-2 text-xl"
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
                    prop.className "flex flex-1 pt-16"
                    prop.children [
                        // Sidebar
                        Html.div [
                            prop.className (sprintf "transition-all duration-500 ease-in-out %s z-10 fixed top-16 left-0 h-[calc(100vh-4rem)] flex flex-col" (if isOpen then "w-64" else if isMobileView() then "w-0" else "w-16"))
                            prop.style [ style.width (if isOpen then length.rem 16 else if isMobileView() then length.rem 0 else length.rem 4) ]
                            prop.children [
                                if isOpen || not (isMobileView()) then
                                    Html.ul [
                                        prop.className (sprintf "menu flex-1 bg-base-200 text-base-content text-lg font-semibold transition-opacity duration-900 ease-in-out %s" (if isOpen || not (isMobileView()) then "opacity-100" else "opacity-0"))
                                        prop.children [
                                            Html.li [
                                                prop.children [
                                                    Html.a [
                                                        prop.href "#"
                                                        prop.title "Home"
                                                        prop.onClick (fun e -> handleItemClick(); Router.goToUrl(e))
                                                        prop.children [
                                                            Fa.i [ Fa.Solid.Home ] []
                                                            if isOpen then Html.span "Welcome" else Html.none
                                                        ]
                                                    ]
                                                ]
                                            ]
                                            Html.li [
                                                prop.children [
                                                    Html.a [
                                                        prop.href "project"
                                                        prop.title "About This Project"
                                                        prop.onClick (fun e -> handleItemClick(); Router.goToUrl(e))
                                                        prop.children [
                                                            Fa.i [ Fa.Solid.ChartArea ] []
                                                            if isOpen then Html.span "The Project" else Html.none
                                                        ]
                                                    ]
                                                ]
                                            ]
                                            Html.li [
                                                prop.children [
                                                    Html.a [
                                                        prop.href "cmsdata"
                                                        prop.title "About The Data"
                                                        prop.onClick (fun e -> handleItemClick(); Router.goToUrl(e))
                                                        prop.children [
                                                            Fa.i [ Fa.Solid.Sitemap ] []
                                                            if isOpen then Html.span "The Data" else Html.none
                                                        ]
                                                    ]
                                                ]
                                            ]
                                            Html.li [
                                                prop.children [
                                                    Html.a [
                                                        prop.href "signup"
                                                        prop.title "The Waitlist"
                                                        prop.onClick (fun e -> handleItemClick(); Router.goToUrl(e))
                                                        prop.children [
                                                            Fa.i [ Fa.Solid.FileSignature ] []
                                                            if isOpen then Html.span "Join Our Waitlist" else Html.none
                                                        ]
                                                    ]
                                                ]
                                            ]
                                            Html.li [
                                                prop.children [
                                                    Html.a [
                                                        prop.href "rower"
                                                        prop.onClick (fun e -> handleItemClick(); Router.goToUrl(e))
                                                        prop.children [
                                                            Html.img [
                                                                prop.title "About Rower Consulting"
                                                                prop.src "/img/Rower_Icon_Gold_t.svg"
                                                                prop.alt "Rower Icon"
                                                                prop.style [
                                                                    style.width (length.px 24)
                                                                    style.height (length.px 24)
                                                                    style.marginRight (length.px 0)
                                                                    style.marginLeft (length.px -3)
                                                                ]
                                                            ]
                                                            if isOpen then Html.span "About Rower" else Html.none
                                                        ]
                                                    ]
                                                ]
                                            ]
                                            Html.li [
                                                prop.children [
                                                    Html.a [
                                                        prop.href "speakez"
                                                        prop.title "About SpeakEZ.ai"
                                                        prop.onClick (fun e -> handleItemClick(); Router.goToUrl(e))
                                                        prop.children [
                                                            Html.img [
                                                                prop.src "/img/SpeakEZ_RowerGold_Icon.svg"
                                                                prop.alt "SpeakEZ Icon"
                                                                prop.style [
                                                                    style.width (length.px 24)
                                                                    style.height (length.px 24)
                                                                    style.marginRight (length.px 0)
                                                                    style.marginLeft (length.px -3)
                                                                ]
                                                            ]
                                                            if isOpen then Html.span "About SpeakEZ" else Html.none
                                                        ]
                                                    ]
                                                ]
                                            ]
                                            Html.li [
                                                prop.children [
                                                    Html.a [
                                                        prop.style [
                                                            style.marginLeft (length.px -2)
                                                        ]
                                                        prop.href "contact"
                                                        prop.title "Contact Us"
                                                        prop.onClick (fun e -> handleItemClick(); Router.goToUrl(e))
                                                        prop.children [
                                                            Fa.i [
                                                                Fa.Solid.Envelope
                                                            ] []
                                                            if isOpen then Html.span "Contact Us" else Html.none
                                                        ]
                                                    ]
                                                ]
                                            ]
                                            Html.li [
                                                prop.children [
                                                    Html.a [
                                                        prop.style [
                                                            style.marginLeft (length.px -2)
                                                        ]
                                                        prop.href "partners"
                                                        prop.title "Partner Info"
                                                        prop.onClick (fun e -> handleItemClick(); Router.goToUrl(e))
                                                        prop.children [
                                                            Fa.i [
                                                                Fa.Solid.InfoCircle
                                                            ] []
                                                            if isOpen then Html.span "Partner Info" else Html.none
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                else Html.none
                                // Add the copyright blurb here
                                if isOpen || not (isMobileView()) then
                                    Html.div [
                                        prop.className "mt-auto text-center text-sm p-4 bg-base-200"
                                        prop.children [
                                            if isOpen then
                                                Html.span [
                                                    prop.dangerouslySetInnerHTML "&copy; 2024 SpeakEZ Platform Services. All Rights Reserved."
                                                ]
                                            else
                                                Html.span [
                                                    prop.dangerouslySetInnerHTML "&copy;"
                                                ]
                                        ]
                                    ]
                                else Html.none
                            ]
                        ]
                        // Main content area
                        Html.div [
                            prop.className (sprintf "flex flex-col flex-1 overflow-hidden transition-all duration-500 ease-in-out %s" (if isOpen then "md:ml-64" else "md:ml-16"))
                            prop.children [
                                Html.div [
                                    prop.key (state.Page.ToString())
                                    prop.className "flex-1 overflow-auto p-4 animate-fade"
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