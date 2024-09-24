module CollabGateway.Client.Pages.SignUp

open Feliz
open Feliz.DaisyUI
open Elmish
open CollabGateway.Client.Server
open UseElmish

type private State = {
    Message : string
    Accordion1Open : bool
    Accordion2Open : bool
    Accordion3Open : bool
    ShowTermsModal : bool
}

type private Msg =
    | AskForMessage of bool
    | MessageReceived of ServerResult<string>
    | ToggleAccordion1
    | ToggleAccordion2
    | ToggleAccordion3
    | ShowTermsModal
    | HideTermsModal

let private init () = { Message = "This is the SignUp page"; Accordion1Open = false; Accordion2Open = false; Accordion3Open = false; ShowTermsModal = false }, Cmd.none

let private closeAccordion label model =
    match label with
    | "Accordion1" -> { model with Accordion1Open = false }
    | "Accordion2" -> { model with Accordion2Open = false }
    | "Accordion3" -> { model with Accordion3Open = false }
    | _ -> model

let private update (msg:Msg) (model:State) : State * Cmd<Msg> =
    match msg with
    | AskForMessage success -> model, Cmd.OfAsync.eitherAsResult (fun _ -> service.GetMessage success) MessageReceived
    | MessageReceived (Ok msg) -> { model with Message = $"Information Successfully Pasted" }, Cmd.none
    | MessageReceived (Error error) -> { model with Message = $"Message Received!" }, Cmd.none
    | ToggleAccordion1 ->
        let model = closeAccordion "Accordion2" model |> closeAccordion "Accordion3"
        { model with Accordion1Open = not model.Accordion1Open }, Cmd.none
    | ToggleAccordion2 ->
        let model = closeAccordion "Accordion1" model |> closeAccordion "Accordion3"
        { model with Accordion2Open = not model.Accordion2Open }, Cmd.none
    | ToggleAccordion3 ->
        let model = closeAccordion "Accordion1" model |> closeAccordion "Accordion2"
        { model with Accordion3Open = not model.Accordion3Open }, Cmd.none
    | ShowTermsModal -> { model with ShowTermsModal = true }, Cmd.none
    | HideTermsModal -> { model with ShowTermsModal = false }, Cmd.none

[<ReactComponent>]
let IndexView () =
    let state, dispatch = React.useElmish(init, update, [| |])

    React.fragment [
        Html.div [
            prop.className "flex flex-col p-4 space-y-4 transition-all duration-300 ease-in-out w-4/5 mx-auto"
            prop.children [
                Html.div [
                    prop.className "text-2xl font-bold mb-4 mx-auto"
                    prop.children [
                        // Header with the message
                        Html.h1 [
                            prop.className "text-2xl font-bold mb-4 mx-auto"
                            prop.text state.Message
                        ]
                    ]
                ]
                Html.div [
                    prop.children [
                        // Wrapper div for the accordion and form
                        Html.div [
                            prop.className "flex flex-col md:flex-row gap-4 w-full"
                            prop.children [
                                // Left-most area with accordion controls
                                Html.div [
                                    prop.className "flex flex-col w-full md:w-1/3"
                                    prop.children [
                                        Html.h1 [
                                            prop.className "text-xl font-bold mx-auto"
                                            prop.text "Methods of Entry"
                                        ]
                                        Daisy.collapse [
                                            prop.className (if state.Accordion1Open then "collapse-open collapse-arrow" else "collapse-close collapse-arrow")
                                            prop.children [
                                                Daisy.collapseTitle [
                                                    prop.className "text-l font-bold mx-auto"
                                                    prop.text "Type info directly"
                                                    prop.onClick (fun _ -> dispatch ToggleAccordion1)
                                                ]
                                                Daisy.collapseContent [
                                                    prop.text "Boring, but it works. Just type your information in the form fields."
                                                ]
                                            ]
                                        ]
                                        Daisy.collapse [
                                            prop.className (if state.Accordion2Open then "collapse-open collapse-arrow" else "collapse-close collapse-arrow")
                                            prop.children [
                                                Daisy.collapseTitle [
                                                    prop.className "text-l font-bold mx-auto"
                                                    prop.text "Use Form Autofill"
                                                    prop.onClick (fun _ -> dispatch ToggleAccordion2)
                                                ]
                                                Daisy.collapseContent [
                                                    prop.text "It's a modern browser feature that fills out forms for you. Just click into the field and use the autofill feature that prompts you with the appropriate values. If you've entered your business information on a site before it should populate most of the fields with one click."
                                                ]
                                            ]
                                        ]
                                        Daisy.collapse [
                                            prop.className (if state.Accordion3Open then "collapse-open collapse-arrow" else "collapse-close collapse-arrow")
                                            prop.children [
                                                Daisy.collapseTitle [
                                                    prop.className "text-l font-bold mx-auto flex items-center"
                                                    prop.children [
                                                        Html.i [
                                                            prop.className "fas fa-star text-orange-700 mr-2"
                                                        ]
                                                        Html.span [
                                                            prop.text "SpeakEZ Smart Paste"
                                                        ]
                                                    ]
                                                    prop.onClick (fun _ -> dispatch ToggleAccordion3)
                                                ]
                                                Daisy.collapseContent [
                                                    prop.text "This is an early glimpse at an 'AI' feature that uses a combination of machine learning and natural language processing. It could be the fastest way to fill out the form. Just find your email signature or contact info and copy it. Then use the 'Smart Paste' button to send the text to our AI systems in SpeakEZ's Lab. It will do its best to parse the text and fill out the form for you. Then after verifying the fields are correct click 'Send Your Info' and onto verifying your email!"
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                                // Form area
                                Html.div [
                                    prop.className "flex flex-col w-full md:w-2/3"
                                    prop.children [
                                        // Full Name, Email Address
                                        Html.div [
                                            prop.className "flex flex-col md:flex-row gap-4 mb-4 w-full"
                                            prop.children [
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.label [
                                                            prop.className "absolute top-50 left-0 px-1"
                                                            prop.style [ style.zIndex 1 ]
                                                            prop.text "Full Name"
                                                        ]
                                                        Html.div [
                                                            prop.className "skeleton rounded-lg h-10 w-full"
                                                        ]
                                                    ]
                                                ]
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.label [
                                                            prop.className "absolute top-50 left-0 px-1"
                                                            prop.style [ style.zIndex 1 ]
                                                            prop.text "Email Address"
                                                        ]
                                                        Html.div [
                                                            prop.className "skeleton rounded-lg h-10 w-full"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                        // Job Title, Phone Number
                                        Html.div [
                                            prop.className "flex flex-col md:flex-row gap-4 mb-4 w-full"
                                            prop.children [
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.label [
                                                            prop.className "absolute top-50 left-0 px-1"
                                                            prop.style [ style.zIndex 1 ]
                                                            prop.text "Job Title"
                                                        ]
                                                        Html.div [
                                                            prop.className "skeleton rounded-lg h-10 w-full"
                                                        ]
                                                    ]
                                                ]
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.label [
                                                            prop.className "absolute top-50 left-0 px-1"
                                                            prop.style [ style.zIndex 1 ]
                                                            prop.text "Phone Number"
                                                        ]
                                                        Html.div [
                                                            prop.className "skeleton rounded-lg h-10 w-full"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                        // Department/Division, Company Name
                                        Html.div [
                                            prop.className "flex flex-col md:flex-row gap-4 mb-4 w-full"
                                            prop.children [
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.label [
                                                            prop.className "absolute top-50 left-0 px-1"
                                                            prop.style [ style.zIndex 1 ]
                                                            prop.text "Department/Division"
                                                        ]
                                                        Html.div [
                                                            prop.className "skeleton rounded-lg h-10 w-full"
                                                        ]
                                                    ]
                                                ]
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.label [
                                                            prop.className "absolute top-50 left-0 px-1"
                                                            prop.style [ style.zIndex 1 ]
                                                            prop.text "Company Name"
                                                        ]
                                                        Html.div [
                                                            prop.className "skeleton rounded-lg h-10 w-full"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                        // Street Address, Street Address 2
                                        Html.div [
                                            prop.className "flex flex-col md:flex-row gap-4 mb-4 w-full"
                                            prop.children [
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.label [
                                                            prop.className "absolute top-50 left-0 px-1"
                                                            prop.style [ style.zIndex 1 ]
                                                            prop.text "Street Address"
                                                        ]
                                                        Html.div [
                                                            prop.className "skeleton rounded-lg h-10 w-full"
                                                        ]
                                                    ]
                                                ]
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.label [
                                                            prop.className "absolute top-50 left-0 px-1"
                                                            prop.style [ style.zIndex 1 ]
                                                            prop.text "Street Address 2"
                                                        ]
                                                        Html.div [
                                                            prop.className "skeleton rounded-lg h-10 w-full"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                        // City, State/Province
                                        Html.div [
                                            prop.className "flex flex-col md:flex-row gap-4 mb-4 w-full"
                                            prop.children [
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.label [
                                                            prop.className "absolute top-50 left-0 px-1"
                                                            prop.style [ style.zIndex 1 ]
                                                            prop.text "City"
                                                        ]
                                                        Html.div [
                                                            prop.className "skeleton rounded-lg h-10 w-full"
                                                        ]
                                                    ]
                                                ]
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.label [
                                                            prop.className "absolute top-50 left-0 px-1"
                                                            prop.style [ style.zIndex 1 ]
                                                            prop.text "State/Province"
                                                        ]
                                                        Html.div [
                                                            prop.className "skeleton rounded-lg h-10 w-full"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                        // Country, PostCode
                                        Html.div [
                                            prop.className "flex flex-col md:flex-row gap-4 mb-4 w-full"
                                            prop.children [
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.label [
                                                            prop.className "absolute top-50 left-0 px-1"
                                                            prop.style [ style.zIndex 1 ]
                                                            prop.text "Country"
                                                        ]
                                                        Html.div [
                                                            prop.className "skeleton rounded-lg h-10 w-full"
                                                        ]
                                                    ]
                                                ]
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.label [
                                                            prop.className "absolute top-50 left-0 px-1"
                                                            prop.style [ style.zIndex 1 ]
                                                            prop.text "PostCode"
                                                        ]
                                                        Html.div [
                                                            prop.className "skeleton rounded-lg h-10 w-full"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                        // Add the checkbox and hyperlink text
                                        Html.div [
                                            prop.className "relative flex flex-col space-y-2 w-full md:w-1/2 mt-4"
                                            prop.children [
                                                Html.div [
                                                    prop.className "flex items-center"
                                                    prop.children [
                                                        Html.input [
                                                            prop.type' "checkbox"
                                                            prop.className "mr-2"
                                                        ]
                                                        Html.span [
                                                            prop.children [
                                                                Html.text "You Agree to Our "
                                                                Html.a [
                                                                    prop.className "text-blue-500 underline cursor-pointer"
                                                                    prop.text "Terms of Service"
                                                                    prop.onClick (fun _ -> dispatch ShowTermsModal)
                                                                ]
                                                            ]
                                                        ]
                                                    ]
                                                ]
                                                // Button group
                                                Daisy.join [
                                                    Daisy.button.button [
                                                        join.item
                                                        button.success
                                                        prop.className "bg-orange-700 hover:bg-green-700 text-base-300"
                                                        prop.text "Smart Paste"
                                                        prop.onClick (fun _ -> true |> AskForMessage |> dispatch)
                                                    ]
                                                    Daisy.button.button [
                                                        join.item
                                                        button.primary
                                                        prop.text "Send Your Info"
                                                        prop.onClick (fun _ -> false |> AskForMessage |> dispatch)
                                                    ]
                                                ]
                                            ]
                                        ]

                                        // Modal component for Terms of Service
                                        Html.div [
                                            prop.className (if state.ShowTermsModal then "modal modal-open" else "modal")
                                            prop.children [
                                                Html.div [
                                                    prop.className "modal-box"
                                                    prop.children [
                                                        Html.h2 [
                                                            prop.className "text-xl font-bold"
                                                            prop.text "Terms of Service"
                                                        ]
                                                        Html.p [
                                                            prop.text "Here are the terms of service..."
                                                        ]
                                                        Daisy.button.button [
                                                            button.secondary
                                                            prop.className "mt-4"
                                                            prop.text "Close"
                                                            prop.onClick (fun _ -> dispatch HideTermsModal)
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]