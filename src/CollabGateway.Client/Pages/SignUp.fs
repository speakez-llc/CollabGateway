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

let private init () = { Message = "Let's Get Started!"; Accordion1Open = false; Accordion2Open = false; Accordion3Open = false; ShowTermsModal = false }, Cmd.none

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
    | ShowTermsModal -> { model with ShowTermsModal = not model.ShowTermsModal }, Cmd.none
    | HideTermsModal -> { model with ShowTermsModal = false }, Cmd.none

[<ReactComponent>]
let IndexView () =
    let state, dispatch = React.useElmish(init, update, [| |])

    React.fragment [
        Html.div [
            prop.className "flex flex-col p-4 space-y-4 transition-all duration-300 ease-in-out w-4/5 mx-auto max-w-screen-xl"
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
                    prop.className "card mx-auto bg-base-200"
                    prop.children [
                        Html.div [
                            prop.className "p-4 m-2 card-body mx-auto"
                            prop.text "The process we use for accessing the site include adding you as an 'external user' to our Entra ID tenant. This will allow you to access the site and use the features available to you. We will also send you an email with a link to the site. You can use this link to access the site at any time. If you have any questions or need help, please feel free to reach out to us."
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
                                        Html.div [
                                            prop.className "card bg-base-200 shadow-lg p-4"
                                            prop.children [
                                                Html.h1 [
                                                    prop.className "text-xl font-bold mx-auto"
                                                    prop.text "Choices in Entering Your Info"
                                                ]
                                                Html.p [
                                                    prop.className "mx-auto"
                                                    prop.text "There are three ways to fill out the form. Choose the one that works best for you."
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
                                                    prop.text "This is an early glimpse at an 'AI' feature that uses a combination of machine learning and natural language processing. It could be the fastest way to fill out the form. You can pick out an old email with your info in the signature - or- copy your contact info from an app. Any place where that info is in regular text. Then use the 'Smart Paste' button to send the clipboard text to our AI systems in SpeakEZ's Lab. It will do its best to parse the text and fill out the form for you. Then after verifying the fields are correct click 'Send Your Info' and onto verifying your email!"
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
                                                    prop.text "It's a standard browser feature that populates the form for you. Just click into the field and use the autofill feature that prompts you with the appropriate values. If you've entered your business information on a site before it should populate most of the fields with one click."
                                                ]
                                            ]
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
                                    ]
                                ]
                                // Form area
                                Html.form [
                                    prop.className "flex flex-col w-full md:w-2/3"
                                    prop.children [
                                        // Full Name, Email Address
                                        Html.div [
                                            prop.className "flex flex-col md:flex-row gap-4 mb-4 w-full"
                                            prop.children [
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.input [
                                                            prop.className "rounded-lg h-10 w-full pl-4 bg-base-200"
                                                            prop.placeholder "Full Name"
                                                            prop.autoComplete "name"
                                                            prop.required true
                                                        ]
                                                    ]
                                                ]
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.input [
                                                            prop.className "rounded-lg h-10 w-full pl-4 bg-base-200"
                                                            prop.placeholder "Email Address"
                                                            prop.autoComplete "email"
                                                            prop.required true
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
                                                        Html.input [
                                                            prop.className "rounded-lg h-10 w-full pl-4 bg-base-200"
                                                            prop.placeholder "Job Title"
                                                            prop.autoComplete "organization-title"
                                                        ]
                                                    ]
                                                ]
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.input [
                                                            prop.className "rounded-lg h-10 w-full pl-4 bg-base-200"
                                                            prop.placeholder "Phone Number"
                                                            prop.autoComplete "tel"
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
                                                        Html.input [
                                                            prop.className "rounded-lg h-10 w-full pl-4 bg-base-200"
                                                            prop.placeholder "Department/Division"
                                                            prop.autoComplete "organization"
                                                        ]
                                                    ]
                                                ]
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.input [
                                                            prop.className "rounded-lg h-10 w-full pl-4 bg-base-200"
                                                            prop.placeholder "Company Name"
                                                            prop.autoComplete "organization"
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
                                                        Html.input [
                                                            prop.className "rounded-lg h-10 w-full pl-4 bg-base-200"
                                                            prop.placeholder "Street Address"
                                                            prop.autoComplete "address-line1"
                                                        ]
                                                    ]
                                                ]
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.input [
                                                            prop.className "rounded-lg h-10 w-full pl-4 bg-base-200"
                                                            prop.placeholder "Street Address 2"
                                                            prop.autoComplete "address-line2"
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
                                                        Html.input [
                                                            prop.className "rounded-lg h-10 w-full pl-4 bg-base-200"
                                                            prop.placeholder "City"
                                                            prop.autoComplete "address-level2"
                                                        ]
                                                    ]
                                                ]
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.input [
                                                            prop.className "rounded-lg h-10 w-full pl-4 bg-base-200"
                                                            prop.placeholder "State/Province"
                                                            prop.autoComplete "address-level1"
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
                                                        Html.input [
                                                            prop.className "rounded-lg h-10 w-full pl-4 bg-base-200"
                                                            prop.placeholder "Country"
                                                            prop.autoComplete "country"
                                                        ]
                                                    ]
                                                ]
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.input [
                                                            prop.className "rounded-lg h-10 w-full pl-4 bg-base-200"
                                                            prop.placeholder "PostCode"
                                                            prop.autoComplete "postal-code"
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
                                                            prop.required true
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
                                                        prop.text "Use Smart Paste"
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