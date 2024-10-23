﻿module CollabGateway.Client.Pages.SignUp

open System
open Fable.Import
open Newtonsoft.Json
open Feliz
open Feliz.DaisyUI
open Elmish
open CollabGateway.Client.Server
open CollabGateway.Client.ViewMsg
open CollabGateway.Shared.API
open UseElmish
open Browser.Dom
open Browser.Navigator

type private State = {
    Message : string
    ResponseMessage: string
    Accordion1Open : bool
    Accordion2Open : bool
    Accordion3Open : bool
    SignUpForm : SignUpForm
    IsProcessing: bool
}

type private Msg =
    | AskForMessage of bool
    | UpdateName of string
    | UpdateEmail of string
    | UpdateJobTitle of string
    | UpdatePhone of string
    | UpdateDepartment of string
    | UpdateCompany of string
    | UpdateStreetAddress1 of string
    | UpdateStreetAddress2 of string
    | UpdateCity of string
    | UpdateStateProvince of string
    | UpdateCountry of string
    | UpdatePostCode of string
    | UpdateSignUpForm of SignUpForm
    | MessageReceived of ServerResult<string>
    | ToggleAccordion1
    | ToggleAccordion2
    | ToggleAccordion3
    | SubmitForm
    | ClearForm
    | FormSubmitted of ServerResult<string>
    | FormProcessed of ServerResult<string>
    | ProcessSmartFormRawContent of string
    | SmartFormProcessed of ServerResult<SignUpForm>
    | NotifyClipboardError of string

let private validateForm (contactForm: SignUpForm) =
    let errors =
        [ if String.IsNullOrWhiteSpace(contactForm.Email) then Some("Email is required") else None
          if String.IsNullOrWhiteSpace(contactForm.Name) then Some("Name is required") else None ]
        |> List.choose id
    errors, not (List.isEmpty errors)

let private init () = { Message = "Take The First Step!"
                        IsProcessing = false
                        ResponseMessage = ""
                        Accordion1Open = false
                        Accordion2Open = false
                        Accordion3Open = false
                        SignUpForm = { Name = ""; Email = ""
                                       JobTitle = ""; Phone = ""
                                       Department = ""; Company = ""
                                       StreetAddress1 = ""; StreetAddress2 = ""
                                       City = ""; StateProvince = ""
                                       Country = ""; PostCode = "" } }, Cmd.none

let private closeAccordion label model =
    match label with
    | "Accordion1" -> { model with Accordion1Open = false }
    | "Accordion2" -> { model with Accordion2Open = false }
    | "Accordion3" -> { model with Accordion3Open = false }
    | _ -> model

let private update (msg: Msg) (model: State) (parentDispatch: ViewMsg -> unit) : State * Cmd<Msg> =
    match msg with
    | AskForMessage success -> model, Cmd.OfAsync.eitherAsResult (fun _ -> service.GetMessage success) MessageReceived
    | UpdateName name -> { model with State.SignUpForm.Name = name }, Cmd.none
    | UpdateEmail email -> { model with State.SignUpForm.Email = email }, Cmd.none
    | UpdateJobTitle jobTitle -> { model with State.SignUpForm.JobTitle = jobTitle }, Cmd.none
    | UpdatePhone phone -> { model with State.SignUpForm.Phone = phone }, Cmd.none
    | UpdateDepartment department -> { model with State.SignUpForm.Department = department }, Cmd.none
    | UpdateCompany company -> { model with State.SignUpForm.Company = company }, Cmd.none
    | UpdateStreetAddress1 streetAddress1 -> { model with State.SignUpForm.StreetAddress1 = streetAddress1 }, Cmd.none
    | UpdateStreetAddress2 streetAddress2 -> { model with State.SignUpForm.StreetAddress2 = streetAddress2 }, Cmd.none
    | UpdateCity city -> { model with State.SignUpForm.City = city }, Cmd.none
    | UpdateStateProvince stateProvince -> { model with State.SignUpForm.StateProvince = stateProvince }, Cmd.none
    | UpdateCountry country -> { model with State.SignUpForm.Country = country }, Cmd.none
    | UpdatePostCode postCode -> { model with State.SignUpForm.PostCode = postCode }, Cmd.none
    | UpdateSignUpForm signUpForm -> { model with SignUpForm = signUpForm }, Cmd.none
    | MessageReceived (Ok msg) -> { model with Message = $"Information Successfully Pasted" }, Cmd.none
    | MessageReceived (Result.Error error) -> { model with Message = $"Message Received!" }, Cmd.none
    | ToggleAccordion1 ->
        let model = closeAccordion "Accordion2" model |> closeAccordion "Accordion3"
        { model with Accordion1Open = not model.Accordion1Open }, Cmd.none
    | ToggleAccordion2 ->
        let model = closeAccordion "Accordion1" model |> closeAccordion "Accordion3"
        { model with Accordion2Open = not model.Accordion2Open }, Cmd.none
    | ToggleAccordion3 ->
        let model = closeAccordion "Accordion1" model |> closeAccordion "Accordion2"
        { model with Accordion3Open = not model.Accordion3Open }, Cmd.none
    | SubmitForm ->
        let errors, hasErrors = validateForm model.SignUpForm
        if hasErrors then
            errors |> List.iter (fun error -> parentDispatch (ShowToast { Message = error; Level = AlertLevel.Warning }))
            model, Cmd.none
        else
            let timeStamp = DateTime.UtcNow
            let sessionToken = Guid.Parse (window.localStorage.getItem("UserStreamToken"))
            let cmd = Cmd.OfAsync.eitherAsResult (fun _ -> service.ProcessSignUpForm (sessionToken, timeStamp, model.SignUpForm)) FormSubmitted
            { model with IsProcessing = true }, cmd
    | FormSubmitted (Ok response) ->
        parentDispatch (ShowToast { Message = "Contact form sent"; Level = AlertLevel.Success })
        { model with SignUpForm = { model.SignUpForm with Email = ""; Name = ""; JobTitle = ""; Phone = ""; Department = ""; Company = ""; StreetAddress1 = ""; StreetAddress2 = ""; City = ""; StateProvince = ""; PostCode = ""; Country = ""
                                     }; ResponseMessage = $"Got success response: {response}"; IsProcessing = false }, Cmd.none
    | FormSubmitted (Result.Error ex) ->
        parentDispatch (ShowToast { Message = "Failed to send contact form"; Level = AlertLevel.Error })
        { model with ResponseMessage = $"Failed to submit form: {ex.ToString()}"; IsProcessing = false }, Cmd.none
    | ProcessSmartFormRawContent clipboardText ->
        let sessionToken = Guid.Parse (window.localStorage.getItem("UserStreamToken"))
        let cmd = Cmd.OfAsync.eitherAsResult (fun _ -> service.ProcessSmartForm (sessionToken, DateTime.UtcNow, clipboardText)) SmartFormProcessed
        { model with IsProcessing = true }, cmd
    | FormProcessed (Ok response) ->
        let parsedForm = JsonConvert.DeserializeObject<SignUpForm>(response)
        { model with SignUpForm = parsedForm; IsProcessing = false }, Cmd.none
    | FormProcessed (Result.Error ex) ->
        parentDispatch (ShowToast { Message = $"Failed to process smart form: {ex}"; Level = AlertLevel.Error })
        { model with IsProcessing = false }, Cmd.none
    | SmartFormProcessed (Ok response) ->
        parentDispatch (ShowToast { Message = "Smart Form Processing Completed"; Level = AlertLevel.Info })
        { model with SignUpForm = response; IsProcessing = false }, Cmd.none
    | SmartFormProcessed (Result.Error ex) ->
        parentDispatch (ShowToast { Message = $"Failed to process smart form: {ex}"; Level = AlertLevel.Error })
        { model with IsProcessing = false }, Cmd.none
    | NotifyClipboardError s ->
        parentDispatch (ShowToast { Message = $"Error in processing clipboard: {s}"; Level = AlertLevel.Warning })
        { model with IsProcessing = false }, Cmd.none
    | ClearForm ->
        { model with SignUpForm = { Name = ""; Email = ""; JobTitle = ""; Phone = ""; Department = ""; Company = ""; StreetAddress1 = ""; StreetAddress2 = ""; City = ""; StateProvince = ""; PostCode = ""; Country = "" } }, Cmd.none

[<ReactComponent>]
let IndexView (parentDispatch : ViewMsg -> unit) =
    let state, dispatch = React.useElmish((fun () -> init ()), (fun msg model -> update msg model parentDispatch), [| |])

    React.useEffectOnce(fun () ->
        parentDispatch (ProcessPageVisited SignUpPage)
    )

    let handleButtonClick (e: Browser.Types.Event) =
        e.preventDefault()
        dispatch SubmitForm
        ()

    let getClipboardText () =
        promise {
            try
                let! text = navigator.clipboard.Value.readText()
                return Some text
            with
            | ex ->
                parentDispatch (ShowToast { Message = $"Failed to read clipboard content: {ex.Message}"; Level = AlertLevel.Warning })
                return None
        }

    let handleSmartForm (e: Browser.Types.Event) =
        e.preventDefault()
        promise {
            let! clipboardTextOption = getClipboardText()
            match clipboardTextOption with
            | Some clipboardText -> dispatch (ProcessSmartFormRawContent clipboardText)
            | None -> parentDispatch (ShowToast { Message = "No text in clipboard"; Level = AlertLevel.Info })
        } |> ignore

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
                    prop.className "card mx-auto bg-base-200 rounded-3xl"
                    prop.children [
                        Html.div [
                            prop.className "p-4 m-2 card-body mx-auto"
                            prop.text "Let's start the conversation. Sending this info allows the helpful experts at Rower to contact you and set up a convenient time for a guided tour. And we've also added a small 'AI' feature to help the process. You can choose to fill out the form manually, or use standard browser autofill feature. But the 'Easter egg' here is the SpeakEZ Smart Form feature. You can simply copy your contact info from an email or app and paste it into the form. The AI will do its best to fill out the form for you. Then click 'Send Your Info' and someone will be in touch with you soon."
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
                                            prop.className "card bg-base-200 shadow-lg p-4 rounded-3xl"
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
                                                            prop.text "SpeakEZ Smart Form"
                                                        ]
                                                    ]
                                                    prop.onClick (fun _ -> dispatch ToggleAccordion3)
                                                ]
                                                Daisy.collapseContent [
                                                    prop.text "This is an early glimpse at an 'AI' feature. You can copy your email signature with your contact info - or - copy your contact info if you have it in a contact management app. Any place where that info is in regular text. Then use the 'Smart Paste' button to send the clipboard text to the AI systems in SpeakEZ's Lab. It will do its best to parse the text and fill out the form for you. Then after verifying the fields are correct click 'Send Your Info'."
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
                                                            prop.value state.SignUpForm.Name
                                                            prop.onChange (fun (e: Browser.Types.Event) ->
                                                                let target = e.target :?> Browser.Types.HTMLInputElement
                                                                dispatch (UpdateName target.value))
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
                                                            prop.value state.SignUpForm.Email
                                                            prop.onChange (fun (e: Browser.Types.Event) ->
                                                                let target = e.target :?> Browser.Types.HTMLInputElement
                                                                dispatch (UpdateEmail target.value))
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
                                                            prop.value state.SignUpForm.JobTitle
                                                            prop.onChange (fun (e: Browser.Types.Event) ->
                                                                let target = e.target :?> Browser.Types.HTMLInputElement
                                                                dispatch (UpdateJobTitle target.value))
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
                                                            prop.value state.SignUpForm.Phone
                                                            prop.onChange (fun (e: Browser.Types.Event) ->
                                                                let target = e.target :?> Browser.Types.HTMLInputElement
                                                                dispatch (UpdatePhone target.value))
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
                                                            prop.autoComplete "division"
                                                            prop.value state.SignUpForm.Department
                                                            prop.onChange (fun (e: Browser.Types.Event) ->
                                                                let target = e.target :?> Browser.Types.HTMLInputElement
                                                                dispatch (UpdateDepartment target.value))
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
                                                            prop.value state.SignUpForm.Company
                                                            prop.onChange (fun (e: Browser.Types.Event) ->
                                                                let target = e.target :?> Browser.Types.HTMLInputElement
                                                                dispatch (UpdateCompany target.value))
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
                                                            prop.value state.SignUpForm.StreetAddress1
                                                            prop.onChange (fun (e: Browser.Types.Event) ->
                                                                let target = e.target :?> Browser.Types.HTMLInputElement
                                                                dispatch (UpdateStreetAddress1 target.value))
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
                                                            prop.value state.SignUpForm.StreetAddress2
                                                            prop.onChange (fun (e: Browser.Types.Event) ->
                                                                let target = e.target :?> Browser.Types.HTMLInputElement
                                                                dispatch (UpdateStreetAddress2 target.value))
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
                                                            prop.value state.SignUpForm.City
                                                            prop.onChange (fun (e: Browser.Types.Event) ->
                                                                let target = e.target :?> Browser.Types.HTMLInputElement
                                                                dispatch (UpdateCity target.value))
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
                                                            prop.value state.SignUpForm.StateProvince
                                                            prop.onChange (fun (e: Browser.Types.Event) ->
                                                                let target = e.target :?> Browser.Types.HTMLInputElement
                                                                dispatch (UpdateStateProvince target.value))
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
                                                            prop.value state.SignUpForm.Country
                                                            prop.onChange (fun (e: Browser.Types.Event) ->
                                                                let target = e.target :?> Browser.Types.HTMLInputElement
                                                                dispatch (UpdateCountry target.value))
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
                                                            prop.value state.SignUpForm.PostCode
                                                            prop.onChange (fun (e: Browser.Types.Event) ->
                                                                let target = e.target :?> Browser.Types.HTMLInputElement
                                                                dispatch (UpdatePostCode target.value))
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                        // Button group
                                        Html.div [
                                            prop.className "flex items-center space-x-2"
                                            prop.children [
                                                Html.button [
                                                    prop.className "btn bg-orange-500 h-10 w-full md:w-2/3 lg:w-1/3 text-gray-200 text-xl"
                                                    prop.text "Use Smart Form"
                                                    prop.type' "submit"
                                                    prop.onClick handleSmartForm
                                                ]
                                                Html.button [
                                                    prop.className "btn bg-primary h-10 w-full md:w-2/3 lg:w-1/3 text-gray-200 text-xl"
                                                    prop.text "Sign Up Now!"
                                                    prop.type' "submit"
                                                    prop.onClick handleButtonClick
                                                ]
                                                if state.IsProcessing = false then
                                                    Html.button [
                                                        prop.className "btn bg-secondary h-10 w-full md:w-2/3 lg:w-1/3 text-gray-200 text-xl"
                                                        prop.text "Clear Form"
                                                        prop.type' "submit"
                                                        prop.onClick (fun (e: Browser.Types.MouseEvent) ->
                                                            dispatch ClearForm)
                                                    ]
                                                if state.IsProcessing then
                                                    Html.div [
                                                        prop.className "flex items-center space-x-2"
                                                        prop.children [
                                                            Html.div [
                                                                prop.className "loading loading-ring loading-md text-warning animate-spin"
                                                                prop.style [
                                                                    style.fontSize (length.px 24)
                                                                    style.marginLeft (length.px 10)
                                                                ]
                                                            ]
                                                            Html.span [
                                                                prop.className "text-warning"
                                                                prop.text "Processing"
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