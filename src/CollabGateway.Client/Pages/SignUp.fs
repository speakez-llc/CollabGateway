﻿module CollabGateway.Client.Pages.SignUp

open System
open CollabGateway.Shared.Errors
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

type State = {
    IsLoading: bool
    IsFormSubmitComplete: bool
    IsEmailVerified: bool
    IsEmailValid: bool option
    IsSubscriptionTokenPresent: bool
    IsWebmailDomain: bool option
    IsProcessing: bool
    IsSubmitActive: bool
    IsIndustryModalOpen: bool
    SignUpForm : SignUpForm
    Errors: Map<string, string>
    Message : string
    ResponseMessage: string
    Accordion1Open : bool
    Accordion2Open : bool
    Accordion3Open : bool
    FormSubmittedCount: int
    CurrentStep: int
    VerificationToken: VerificationToken
    SubscriptionToken: SubscriptionToken
    GicsTaxonomy: GicsTaxonomy[] option
    GicsQuery: string
}

type Msg =
    | AskForMessage of bool
    | GicsTaxonomyLoaded of GicsTaxonomy[]
    | UpdateGicsQuery of string
    | UpdateName of string
    | UpdateEmail of string
    | UpdateJobTitle of string
    | UpdatePhone of string
    | UpdateDepartment of string
    | UpdateIndustry of string
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
    | ToggleIndustryModal
    | SubmitForm
    | ClearForm
    | FormSubmitted of ServerResult<string>
    | FormProcessed of ServerResult<string>
    | RetrieveFormSubmittedCount of ServerResult<int>
    | ProcessSmartFormRawContent of string
    | SmartFormProcessed of ServerResult<SignUpForm>
    | NotifyClipboardError of string
    | ReevaluateFormSubmittedCount
    | WebmailDomainFlagged of bool
    | CheckEmailVerification
    | EmailVerificationChecked of bool
    | UpdateVerificationToken
    | UpdateSubscriptionToken of VerificationToken
    | SendVerificationEmail of VerificationToken * SubscriptionToken



let private isEmailValid (email: string) =
    let emailRegex = System.Text.RegularExpressions.Regex("^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$")
    emailRegex.IsMatch(email)

let flaggedWebmailDomain email =
    async {
        if isEmailValid email then
            let emailDomain = email.Split('@').[1]
            let! isWebmail = service.FlagWebmailDomain emailDomain
            return isWebmail
        else
            return false
    }

let private validateForm (signupForm: SignUpForm) =
    let submittedEmailIsValid = isEmailValid signupForm.Email
    let cmd =
        if submittedEmailIsValid then
            Cmd.OfAsync.perform (fun () -> flaggedWebmailDomain signupForm.Email) () WebmailDomainFlagged
        else
            Cmd.none

    let errors =
        [ if String.IsNullOrWhiteSpace(signupForm.Email) then Some("Email is required") else None
          if String.IsNullOrWhiteSpace(signupForm.Name) then Some("Name is required") else None ]
        |> List.choose id

    let isSubmitActive =
        List.isEmpty errors && submittedEmailIsValid

    errors, not (List.isEmpty errors), cmd, isSubmitActive

let private checkFormSubmission streamToken =
    async {
        let! formSubmissionExists = service.RetrieveSignUpFormSubmitted streamToken
        return formSubmissionExists
    }

let getSubscriptionToken streamToken email =
    async {
        let! token = service.RetrieveLatestSubscriptionToken (streamToken, email)
        return token
    }

let getVerificationToken streamToken email =
    async {
        let! token = service.RetrieveLatestVerificationToken (streamToken, email)
        return token
    }

let init () : State * Cmd<Msg> =
    let streamToken = Guid.Parse (window.localStorage.getItem("UserStreamToken"))
    let initialState = {
        IsLoading = true
        Message = "Take The First Step!"
        FormSubmittedCount = 0
        IsProcessing = false
        ResponseMessage = ""
        Accordion1Open = false
        Accordion2Open = false
        Accordion3Open = false
        SignUpForm = {
            Name = ""
            Email = ""
            JobTitle = ""
            Phone = ""
            Department = ""
            Industry = ""
            StreetAddress1 = ""
            StreetAddress2 = ""
            City = ""
            StateProvince = ""
            Country = ""
            PostCode = ""
        }
        IsFormSubmitComplete = false
        IsEmailValid = None
        IsWebmailDomain = None
        IsEmailVerified = false
        IsSubscriptionTokenPresent = false
        IsIndustryModalOpen = false
        CurrentStep = 1
        Errors = Map.empty
        IsSubmitActive = false
        VerificationToken = Guid.Empty
        SubscriptionToken = Guid.Empty
        GicsTaxonomy = None
        GicsQuery = ""
    }

    let checkFormSubmissionCmd =
        Cmd.OfAsync.perform (fun () -> checkFormSubmission streamToken) () (fun formSubmissionExists ->
            if formSubmissionExists then
                FormSubmitted (Ok "FormSubmitted")
            else
                FormSubmitted (Result.Error (ServerError.Exception "Ignore"))
        )

    let checkIfEmailIsVerifiedCmd =
       Cmd.OfAsync.perform (fun () -> service.RetrieveEmailStatus streamToken) () (fun status ->
           match status with
           | Some (_, _, EmailStatus.Verified) -> EmailVerificationChecked true
           | _ -> EmailVerificationChecked false)

    let getGicsTaxonomyCmd =
        Cmd.OfAsync.perform (fun () -> service.LoadGicsTaxonomy ()) () GicsTaxonomyLoaded

    initialState, Cmd.batch [checkFormSubmissionCmd; checkIfEmailIsVerifiedCmd; getGicsTaxonomyCmd]

let private closeAccordion label model =
    match label with
    | "Accordion1" -> { model with Accordion1Open = false }
    | "Accordion2" -> { model with Accordion2Open = false }
    | "Accordion3" -> { model with Accordion3Open = false }
    | _ -> model

let isFormEmpty (form: SignUpForm) =
    String.IsNullOrWhiteSpace(form.Name) &&
    String.IsNullOrWhiteSpace(form.Email) &&
    String.IsNullOrWhiteSpace(form.JobTitle) &&
    String.IsNullOrWhiteSpace(form.Phone) &&
    String.IsNullOrWhiteSpace(form.Department) &&
    String.IsNullOrWhiteSpace(form.Industry) &&
    String.IsNullOrWhiteSpace(form.StreetAddress1) &&
    String.IsNullOrWhiteSpace(form.StreetAddress2) &&
    String.IsNullOrWhiteSpace(form.City) &&
    String.IsNullOrWhiteSpace(form.StateProvince) &&
    String.IsNullOrWhiteSpace(form.Country) &&
    String.IsNullOrWhiteSpace(form.PostCode)

let private checkEmailVerificationWithDelay streamToken =
    async {
        do! Async.Sleep 5000
        return! service.RetrieveEmailStatus streamToken
    }

let private update (msg: Msg) (model: State) (parentDispatch: ViewMsg -> unit) : State * Cmd<Msg> =
    match msg with
    | ToggleIndustryModal ->
        { model with IsIndustryModalOpen = not model.IsIndustryModalOpen }, Cmd.none
    | GicsTaxonomyLoaded taxonomy ->
        { model with GicsTaxonomy = Some taxonomy }, Cmd.none
    | UpdateGicsQuery query ->
        { model with GicsQuery = query }, Cmd.none
    | AskForMessage success -> model, Cmd.OfAsync.eitherAsResult (fun _ -> service.GetMessage (if success then "true" else "false")) MessageReceived
    | UpdateName name ->
        let newModel = { model with State.SignUpForm.Name = name }
        let errors, _, cmd, isSubmitActive = validateForm newModel.SignUpForm
        { newModel with Errors = errors |> List.mapi (fun i error -> (i.ToString(), error)) |> Map.ofList; IsSubmitActive = isSubmitActive }, cmd
    | UpdateEmail email ->
        let isEmailValid = isEmailValid email
        let cmd =
            if isEmailValid then
                Cmd.OfAsync.perform (fun () -> flaggedWebmailDomain email) () WebmailDomainFlagged
            else
                Cmd.none

        let checkEmailVerificationCmd =
            if isEmailValid then
                let streamToken = Guid.Parse (window.localStorage.getItem("UserStreamToken"))
                Cmd.OfAsync.perform (fun () -> service.RetrieveEmailStatus streamToken) () (fun status ->
                    match status with
                    | Some (_, _, EmailStatus.Verified) -> EmailVerificationChecked true
                    | _ -> EmailVerificationChecked false)
            else
                Cmd.none

        let newModel = { model with State.SignUpForm.Email = email; IsEmailValid = Some isEmailValid }
        let errors, _, _, isSubmitActive = validateForm newModel.SignUpForm
        { newModel with Errors = errors |> List.mapi (fun i error -> (i.ToString(), error)) |> Map.ofList; IsSubmitActive = isSubmitActive }, Cmd.batch [cmd; checkEmailVerificationCmd]
    | UpdateJobTitle jobTitle -> { model with State.SignUpForm.JobTitle = jobTitle }, Cmd.none
    | UpdatePhone phone -> { model with State.SignUpForm.Phone = phone }, Cmd.none
    | UpdateDepartment department -> { model with State.SignUpForm.Department = department }, Cmd.none
    | UpdateIndustry industry -> { model with State.SignUpForm.Industry = industry }, Cmd.none
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
        let errors, hasErrors, cmd, _ = validateForm model.SignUpForm
        if hasErrors then
            errors |> List.iter (fun error -> parentDispatch (ShowToast (error, AlertLevel.Warning)))
            { model with Errors = errors |> List.mapi (fun i error -> (i.ToString(), error)) |> Map.ofList }, cmd
        else
            let processCmd = Cmd.OfAsync.perform (fun () -> async {
                let! result = service.ProcessSignUpForm (DateTime.UtcNow, Guid.Parse (window.localStorage.getItem("UserStreamToken")), model.SignUpForm)
                return Ok result }) () FormSubmitted
            { model with IsProcessing = true }, processCmd
    | UpdateVerificationToken ->
        let streamToken = Guid.Parse (window.localStorage.getItem("UserStreamToken"))
        let cmd = Cmd.OfAsync.perform (fun () -> service.RetrieveLatestVerificationToken (streamToken, model.SignUpForm.Email)) () (fun token ->
            UpdateSubscriptionToken token)
        model, cmd
    | UpdateSubscriptionToken verificationToken ->
        let streamToken = Guid.Parse (window.localStorage.getItem("UserStreamToken"))
        let cmd = Cmd.OfAsync.perform (fun () -> service.RetrieveLatestSubscriptionToken (streamToken, model.SignUpForm.Email)) () (fun subscriptionToken ->
            Console.WriteLine $"Verification token: {verificationToken}, Subscription token: {subscriptionToken}"
            SendVerificationEmail (verificationToken, subscriptionToken))
        model, cmd
    | SendVerificationEmail (verificationToken, subscriptionToken) ->
        if not model.IsEmailVerified then
            let cmd = Cmd.OfAsync.perform (fun () -> service.SendEmailVerification (model.SignUpForm.Name, model.SignUpForm.Email, verificationToken, subscriptionToken)) () (fun _ -> EmailVerificationChecked false)
            { model with IsProcessing = false }, Cmd.batch [cmd; Cmd.ofMsg CheckEmailVerification]
        else
            { model with IsProcessing = false }, Cmd.none
    | FormSubmitted (Ok "FormSubmitted") ->
        let newState = { model with IsFormSubmitComplete = true; IsLoading = false }
        if newState.IsEmailVerified then
            { newState with IsProcessing = false; CurrentStep = 3 }, Cmd.none
        else
            { newState with CurrentStep = 2 }, Cmd.none
    | FormSubmitted (Ok _) ->
        let newState = { model with IsFormSubmitComplete = true; IsLoading = false }
        if newState.IsEmailVerified then
            { newState with IsProcessing = false; CurrentStep = 3 }, Cmd.none
        else
            { newState with CurrentStep = 2 }, Cmd.ofMsg UpdateVerificationToken
    | FormSubmitted (Result.Error ex) ->
        if ex = ServerError.Exception "Ignore" then
            { model with IsLoading = false }, Cmd.none
        else
            parentDispatch (ShowToast ("Failed to send contact form", AlertLevel.Error))
            { model with ResponseMessage = $"Failed to submit form: {ex.ToString()}"; IsProcessing = false; IsLoading = false }, Cmd.none
    | ProcessSmartFormRawContent clipboardText ->
        if clipboardText = "" then
            parentDispatch (ShowToast ("Clipboard is empty", AlertLevel.Warning))
            model, Cmd.none
        else
            let cmd = Cmd.OfAsync.eitherAsResult (fun _ -> service.ProcessSmartForm (DateTime.UtcNow, Guid.Parse (window.localStorage.getItem("UserStreamToken")), clipboardText)) SmartFormProcessed
            { model with IsProcessing = true }, cmd
    | FormProcessed (Ok response) ->
        let parsedForm = JsonConvert.DeserializeObject<SignUpForm>(response)
        { model with SignUpForm = parsedForm; IsProcessing = false }, Cmd.none
    | FormProcessed (Result.Error ex) ->
        parentDispatch (ShowToast ($"Failed to process form: {ex}", AlertLevel.Error ))
        { model with IsProcessing = false }, Cmd.none
    | SmartFormProcessed (Ok response) ->
        let newModel = { model with SignUpForm = response; IsProcessing = false }
        let errors, _, cmd1, isSubmitActive = validateForm newModel.SignUpForm
        let cmd2 = Cmd.OfAsync.perform (fun () -> flaggedWebmailDomain response.Email) () WebmailDomainFlagged
        { newModel with Errors = errors |> List.mapi (fun i error -> (i.ToString(), error)) |> Map.ofList; IsSubmitActive = isSubmitActive }, Cmd.batch [cmd1; cmd2]
    | SmartFormProcessed (Result.Error ex) ->
        parentDispatch (ShowToast ($"Failed to process smart form: {ex}", AlertLevel.Error ))
        { model with IsProcessing = false }, Cmd.none
    | NotifyClipboardError s ->
        parentDispatch (ShowToast ($"Error in processing clipboard: {s}", AlertLevel.Warning ))
        model, Cmd.none
    | ClearForm ->
        let newModel = {
            model with
                SignUpForm = {
                    Name = ""
                    Email = ""
                    JobTitle = ""
                    Phone = ""
                    Department = ""
                    Industry = ""
                    StreetAddress1 = ""
                    StreetAddress2 = ""
                    City = ""
                    StateProvince = ""
                    Country = ""
                    PostCode = ""
                }
                IsSubmitActive = false
                IsEmailValid = None
                IsWebmailDomain = None
            }
        newModel, Cmd.none
    | RetrieveFormSubmittedCount (Ok count) ->
        if count >= 5 then
            parentDispatch (ShowToast ("You have reached the limit of form submissions", AlertLevel.Warning))
            { model with FormSubmittedCount = count }, Cmd.none
        else
            { model with FormSubmittedCount = count }, Cmd.none
    | RetrieveFormSubmittedCount (Result.Error ex) -> { model with ResponseMessage = ex.ToString() }, Cmd.none
    | ReevaluateFormSubmittedCount ->
        let streamToken = Guid.Parse (window.localStorage.getItem("UserStreamToken"))
        let cmd = Cmd.OfAsync.eitherAsResult (fun _ -> service.RetrieveSmartFormSubmittedCount streamToken) RetrieveFormSubmittedCount
        model, cmd
    | WebmailDomainFlagged isWebmailDomain ->
        let errors =
            if isWebmailDomain then
                model.Errors |> Map.add "EmailDomain" "The email domain is not allowed"
            else
                model.Errors |> Map.remove "EmailDomain"
        let isSubmitActive =
            not isWebmailDomain && model.IsEmailValid = Some true && List.isEmpty (Map.toList errors)
        if isWebmailDomain then
            parentDispatch (ShowToast ("Webmail Domains Are Not Allowed", AlertLevel.Warning))
        { model with Errors = errors; IsWebmailDomain = Some isWebmailDomain; IsSubmitActive = isSubmitActive }, Cmd.none
    | CheckEmailVerification ->
        let streamToken = Guid.Parse (window.localStorage.getItem("UserStreamToken"))
        let cmd = Cmd.OfAsync.perform (fun () -> checkEmailVerificationWithDelay streamToken) () (fun status ->
            match status with
            | Some (_, _, EmailStatus.Verified) -> EmailVerificationChecked true
            | _ -> EmailVerificationChecked false)
        model, cmd
    | EmailVerificationChecked isVerified ->
        let newState = { model with IsEmailVerified = isVerified }
        let nextCmd =
            if not isVerified && newState.IsFormSubmitComplete then
                Cmd.ofMsg CheckEmailVerification
            else
                Cmd.none
        if newState.IsFormSubmitComplete && newState.IsEmailVerified then
            { newState with CurrentStep = 3 }, Cmd.none
        else
            newState, nextCmd

[<ReactComponent>]
let IndexView (parentDispatch : ViewMsg -> unit) =
    let state, dispatch = React.useElmish(init, (fun msg model -> update msg model parentDispatch), [| |])

    React.useEffectOnce(fun () ->
        parentDispatch (ProcessPageVisited SignUpPage)
    )

    let handleButtonClick (e: Browser.Types.Event) =
        e.preventDefault()
        dispatch SubmitForm
        ()

    let emailInputClass =
        match state.IsEmailValid, state.IsWebmailDomain with
        | Some false, _ -> "input-error"
        | _, Some true -> "input-error"
        | _ -> ""

    let getClipboardText () =
        promise {
            try
                let! text = navigator.clipboard.Value.readText()
                return Some text
            with
            | ex ->
                parentDispatch (ShowToast ($"Failed to read clipboard content: {ex.Message}", AlertLevel.Warning ))
                return None
        }

    let handleSmartForm (e: Browser.Types.Event) =
        e.preventDefault()
        promise {
            let! clipboardTextOption = getClipboardText()
            match clipboardTextOption with
            | Some clipboardText -> dispatch (ProcessSmartFormRawContent clipboardText)
            | None -> parentDispatch (ShowToast ("No text in clipboard", AlertLevel.Warning))
        } |> ignore

    let updateTaxonomy (taxonomyCode: string) =
        let selectedTaxonomy =
            state.GicsTaxonomy.Value
            |> Array.collect (fun g ->
                let sectorPath = [g.SectorName]
                let industryGroupPath = if String.IsNullOrWhiteSpace(g.IndustryGroupName) then [] else [g.IndustryGroupName]
                let industryPath = if String.IsNullOrWhiteSpace(g.IndustryName) then [] else [g.IndustryName]
                let subIndustryPath = if String.IsNullOrWhiteSpace(g.SubIndustryName) then [] else [g.SubIndustryName]
                let fullPath = sectorPath @ industryGroupPath @ industryPath @ subIndustryPath
                [|
                    (g.SectorCode, String.concat " > " sectorPath)
                    (g.IndustryGroupCode, String.concat " > " (sectorPath @ industryGroupPath))
                    (g.IndustryCode, String.concat " > " (sectorPath @ industryGroupPath @ industryPath))
                    (g.SubIndustryCode, String.concat " > " fullPath)
                |]
            )
            |> Array.tryFind (fun (code, _) -> code = taxonomyCode)

        match selectedTaxonomy with
        | Some (code, path) ->
            dispatch (UpdateIndustry code)
            dispatch ToggleIndustryModal
        | None -> ()

    let generateBreadcrumbPaths (taxonomy: GicsTaxonomy[]) =
        taxonomy
        |> Array.collect (fun g ->
            let sectorPath = [g.SectorName]
            let industryGroupPath = if String.IsNullOrWhiteSpace(g.IndustryGroupName) then [] else [g.IndustryGroupName]
            let industryPath = if String.IsNullOrWhiteSpace(g.IndustryName) then [] else [g.IndustryName]
            let subIndustryPath = if String.IsNullOrWhiteSpace(g.SubIndustryName) then [] else [g.SubIndustryName]
            let fullPath = sectorPath @ industryGroupPath @ industryPath @ subIndustryPath
            [|
                (g.SectorCode, String.concat " > " sectorPath)
                (g.IndustryGroupCode, String.concat " > " (sectorPath @ industryGroupPath))
                (g.IndustryCode, String.concat " > " (sectorPath @ industryGroupPath @ industryPath))
                (g.SubIndustryCode, String.concat " > " fullPath)
            |]
        )
        |> Array.distinctBy snd
        |> Array.sortBy fst
        |> Map.ofArray

    let filteredOptions =
        match state.GicsTaxonomy with
        | Some taxonomy ->
            taxonomy
            |> generateBreadcrumbPaths
            |> Map.toList
            |> List.filter (fun (_, path) -> path.ToLower().Contains(state.GicsQuery.ToLower()))
        | None -> []

    let renderStep1 () =
        Html.div [
            prop.className "flex flex-col p-4 space-y-4 transition-all duration-300 ease-in-out w-4/5 mx-auto max-w-screen-xl"
            prop.children [
                Html.div [
                    prop.className "text-2xl font-bold mb-4 mx-auto"
                    prop.children [
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
                            prop.text "Let's start the conversation. Sending this info allows the helpful experts at Rower to contact you and set up a convenient time for a guided tour. And we've also added a small 'AI' feature to help the process. You can choose to fill out the form manually, or use standard browser autofill feature. But the 'Easter egg' here is the SpeakEZ Smart Form feature. You can simply copy your contact info from an email or app and paste it into the form. The AI will do its best to fill out the form for you. Then click 'Sign Up Now!' and someone will be in touch with you soon."
                        ]
                    ]
                ]
                Html.div [
                    prop.children [
                        Html.div [
                            prop.className "flex flex-col md:flex-row gap-4 w-full"
                            prop.children [
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
                                                    prop.text "This is an early glimpse at an 'AI' feature. You can copy your email signature with your contact info - or - copy your contact info if you have it in a contact management app. Any place where that info is in regular text. Then use the 'Smart Paste' button to send the clipboard text to the AI systems in SpeakEZ's Lab. It will do its best to parse the text and fill out the form for you. Then after verifying the fields are correct click 'Sign Up Now!'."
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
                                Html.form [
                                    prop.className "flex flex-col w-full md:w-2/3"
                                    prop.children [
                                        Html.button [
                                            prop.className "btn bg-orange-500 h-10 w-full md:w-1/3 text-gray-200 text-xl mb-4"
                                            prop.text "Use Smart Form"
                                            prop.type' "submit"
                                            prop.disabled (state.FormSubmittedCount > 4)
                                            prop.onClick handleSmartForm
                                        ]
                                        Html.div [
                                            prop.className "flex flex-col md:flex-row gap-4 mb-4 w-full"
                                            prop.children [
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.input [
                                                            prop.className "input input-bordered h-10 w-full pl-4 bg-base-200 required"
                                                            prop.placeholder "Name is required"
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
                                                            prop.className $"input input-bordered h-10 w-full pl-4 bg-base-200 required {emailInputClass}"
                                                            prop.placeholder "A work email is required"
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
                                        Html.div [
                                            prop.className "flex flex-col md:flex-row gap-4 mb-4 w-full"
                                            prop.children [
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.input [
                                                            prop.className "input input-bordered h-10 w-full pl-4 bg-base-200"
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
                                                            prop.className "input input-bordered h-10 w-full pl-4 bg-base-200"
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
                                        Html.div [
                                            prop.className "flex flex-col md:flex-row gap-4 mb-4 w-full"
                                            prop.children [
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.input [
                                                            prop.className "input input-bordered h-10 w-full pl-4 bg-base-200"
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
                                                        Html.div [
                                                            prop.className "join flex w-full"
                                                            prop.children [
                                                                Html.input [
                                                                    prop.className "input input-bordered flex-grow gap-4 bg-base-200 join-item overflow-x-auto"
                                                                    prop.placeholder "Industry"
                                                                    prop.style [
                                                                        style.overflowX.scroll
                                                                        style.maxHeight (length.px 40)
                                                                        style.direction.rightToLeft
                                                                        style.textAlign.left
                                                                    ]
                                                                    prop.value (
                                                                        match state.GicsTaxonomy with
                                                                        | Some taxonomy ->
                                                                            let selectedPath =
                                                                                taxonomy
                                                                                |> generateBreadcrumbPaths
                                                                                |> Map.tryFind state.SignUpForm.Industry
                                                                            match selectedPath with
                                                                            | Some path -> path
                                                                            | None -> ""
                                                                        | None -> ""
                                                                    )
                                                                    prop.readOnly true
                                                                ]
                                                                Html.button [
                                                                    prop.className "btn h-10 btn-secondary btn-sm join-item"
                                                                    prop.text "Select"
                                                                    prop.type' "button"
                                                                    prop.onClick (fun _ -> dispatch ToggleIndustryModal)
                                                                ]
                                                            ]
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                        Html.div [
                                            prop.className "flex flex-col md:flex-row gap-4 mb-4 w-full"
                                            prop.children [
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.input [
                                                            prop.className "input input-bordered h-10 w-full pl-4 bg-base-200"
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
                                                            prop.className "input input-bordered h-10 w-full pl-4 bg-base-200"
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
                                        Html.div [
                                            prop.className "flex flex-col md:flex-row gap-4 mb-4 w-full"
                                            prop.children [
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.input [
                                                            prop.className "input input-bordered h-10 w-full pl-4 bg-base-200"
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
                                                            prop.className "input input-bordered h-10 w-full pl-4 bg-base-200"
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
                                        Html.div [
                                            prop.className "flex flex-col md:flex-row gap-4 mb-4 w-full"
                                            prop.children [
                                                Html.div [
                                                    prop.className "relative flex flex-col w-full"
                                                    prop.children [
                                                        Html.input [
                                                            prop.className "input input-bordered h-10 w-full pl-4 bg-base-200"
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
                                                            prop.className "input input-bordered h-10 w-full pl-4 bg-base-200"
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
                                        Html.div [
                                            prop.className "flex flex-col md:flex-row gap-4 mb-4 w-full"
                                            prop.children [
                                                Html.button [
                                                    prop.className "btn bg-primary h-10 w-full md:w-1/3 text-gray-200 text-xl"
                                                    prop.text "Sign Up Now!"
                                                    prop.type' "submit"
                                                    prop.disabled (not state.IsSubmitActive)
                                                    prop.onClick handleButtonClick
                                                ]
                                                if state.IsProcessing = false then
                                                    Html.button [
                                                        prop.className "btn bg-secondary h-10 w-full md:w-1/3 text-gray-200 text-xl"
                                                        prop.text "Clear Form"
                                                        prop.type' "submit"
                                                        prop.disabled (isFormEmpty state.SignUpForm)
                                                        prop.onClick (fun (e: Browser.Types.MouseEvent) ->
                                                            dispatch ClearForm)
                                                    ]
                                                if state.IsProcessing then
                                                    Html.div [
                                                        prop.className "flex items-center space-x-2"
                                                        prop.children [
                                                            Html.div [
                                                                prop.className "loading loading-ring loading-md text-warning animate-spin"
                                                            ]
                                                            Html.span [
                                                                prop.className "text-warning text-l"
                                                                prop.text "Processing Clipboard"
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

    let renderStep2 () =
        Html.div [
            prop.className "flex flex-col p-4 space-y-4 transition-opacity duration-900 ease-in-out w-full md:w-4/5  mx-auto max-w-screen-xl"
            prop.children [
                Html.h1 [
                    prop.className "text-2xl font-bold mb-4 mx-auto"
                    prop.text "Check your email"
                ]
                Html.div [
                    prop.className "card mx-auto bg-base-200 w-4/5 mx-auto rounded-3xl"
                    prop.children [
                        Html.div [
                            prop.className "p-4 m-2 card-body mx-auto"
                            prop.text "We have sent a verification link to your email. Please click on the link to verify your email address."
                        ]
                    ]
                ]
                Html.div [
                    prop.className "flex items-center justify-center space-x-2"
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
                            prop.text "Waiting for verification from email link"
                        ]
                    ]
                ]
            ]
        ]

    let renderStep3 () =
        Html.div [
            prop.className "flex flex-col p-4 space-y-4 transition-opacity duration-900 ease-in-out w-full md:w-4/5  mx-auto max-w-screen-xl"
            prop.children [
                Html.h1 [
                    prop.className "text-2xl font-bold mb-4 mx-auto"
                    prop.text "Thank you for signing up for our waitlist!"
                ]
                Html.div [
                    prop.className "card mx-auto bg-base-200 w-4/5 mx-auto rounded-3xl"
                    prop.children [
                        Html.div [
                            prop.className "p-4 m-2 card-body mx-auto"
                            prop.text "Someone will be in touch with you within one to two business days."
                        ]
                    ]
                ]
                Html.button [
                    prop.className "btn bg-secondary h-10 w-full md:w-2/3 lg:w-1/3 text-gray-200 text-xl mx-auto"
                    prop.text "Your Activity Summary"
                    prop.onClick (fun _ ->
                        parentDispatch (ProcessButtonClicked SignUpActivityButton)
                        window.location.href <- "/activity"
                    )
                ]
            ]
        ]

    let renderCurrentStep () =
        match state.CurrentStep with
        | 1 -> renderStep1()
        | 2 -> renderStep2()
        | 3 -> renderStep3()
        | _ -> Html.none

    React.fragment [
        Html.div [
            prop.className "flex justify-center items-center w-full p-4 rounded-3xl mx-auto"
            prop.children [
                Html.div [
                    prop.className "flex items-start flex-col steps steps-vertical w-1/2 md:w-3/4 duration-900 ease-in-out max-w-screen-xl lg:steps-horizontal"
                    prop.children [
                        Html.div [
                            prop.className (if state.CurrentStep >= 1 then "step step-primary" else "step")
                            prop.text "Submit Form"
                        ]
                        Html.div [
                            prop.className (if state.CurrentStep >= 2 then "step step-primary" else "step")
                            prop.text "Verify Email"
                        ]
                        Html.div [
                            prop.className (if state.CurrentStep >= 3 then "step step-primary" else "step")
                            prop.text "Confirmation"
                        ]
                    ]
                ]
            ]
        ]
        if state.IsLoading then
            Html.div [
                prop.className "flex items-center space-x-2 justify-center mt-6"
                prop.children [
                    Html.div [
                        prop.className "loading loading-dots loading-md text-warning"
                    ]
                    Html.span [
                        prop.className "text-warning text-xl"
                        prop.text "Loading..."
                    ]
                ]
            ]
        else
            renderCurrentStep()
        if state.IsIndustryModalOpen then
            let sectorIcons = Map.ofList [
                ("10", ("fas fa-oil-well", "Energy"))
                ("15", ("fas fa-trowel-bricks", "Materials"))
                ("20", ("fas fa-industry", "Industrials"))
                ("25", ("fas fa-car", "Consumer Discretionary"))
                ("30", ("fas fa-shopping-cart", "Consumer Staples"))
                ("35", ("fas fa-heartbeat", "Health Care"))
                ("40", ("fas fa-file-invoice-dollar", "Financials"))
                ("45", ("fas fa-computer", "Information Technology"))
                ("50", ("fas fa-tower-cell", "Communication Services"))
                ("55", ("fas fa-hand-holding-droplet", "Utilities"))
                ("60", ("fas fa-home", "Real Estate"))
                ("90", ("fas fa-handshake-angle", "Non-Profit & NGOs"))
                ("95", ("fas fa-university", "Public Sector"))
            ]

            let getSectorIcon sectorCode =
                match Map.tryFind sectorCode sectorIcons with
                | Some (icon, _) -> icon
                | None -> "fas fa-question-circle"

            let getSectorName sectorCode =
                match Map.tryFind sectorCode sectorIcons with
                | Some (_, name) -> name
                | None -> "Unknown Sector"

            Html.div [
                prop.className "fixed inset-0 flex items-center justify-center bg-gray-800 bg-opacity-75 z-50 pointer-events-auto"
                prop.children [
                    Html.div [
                        prop.className "bg-base-100 p-6 rounded-lg shadow-lg w-full max-w-7xl mx-auto max-h-screen overflow-y-auto"
                        prop.children [
                            Html.h2 [
                                prop.className "text-xl font-bold mb-4"
                                prop.text "Select Industry"
                            ]
                            Html.div [
                                prop.className "card bg-base-200 shadow-lg p-4 rounded-3xl mb-4"
                                prop.children [
                                    Html.h3 [
                                        prop.className "text-lg font-bold"
                                        prop.text "GICS Standard with Additional Sectors"
                                    ]
                                    Html.p [
                                        prop.text "We use the GICS standard plus two additional sectors for Non-Profits & NGOs and Public Sector. Use the search field below to narrow your choices."
                                    ]
                                ]
                            ]
                            Html.div [
                                prop.className "flex flex-wrap gap-x-4 justify-around"
                                prop.children (
                                    sectorIcons
                                    |> Map.toList
                                    |> List.map (fun (sectorCode, _) ->
                                        Html.div [
                                            prop.className "avatar flex flex-col items-center group"
                                            prop.children [
                                                Html.div [
                                                    prop.className "rounded-full w-12 h-12 bg-orange-500 mb-6 relative"
                                                    prop.style [
                                                        style.display.flex
                                                        style.alignItems.center
                                                        style.justifyContent.center
                                                    ]
                                                    prop.title (getSectorName sectorCode)
                                                    prop.onClick (fun _ ->
                                                        let sectorName = getSectorName sectorCode
                                                        dispatch (UpdateGicsQuery sectorName)
                                                        let selectElement = document.querySelector("select") :?> Browser.Types.HTMLSelectElement
                                                        selectElement.size <- selectElement.options.length
                                                    )
                                                    prop.children [
                                                        Html.div [
                                                            prop.className "absolute inset-0 rounded-full border-t-2 border-r-2 border-transparent group-hover:border-orange-300 group-hover:animate-spin"
                                                        ]
                                                        Html.i [
                                                            prop.className (getSectorIcon sectorCode)
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    )
                                )
                            ]
                            Html.div [
                                prop.className "input-group join w-full mb-4"
                                prop.children [
                                    Html.input [
                                        prop.className "input input-bordered join-item flex-grow"
                                        prop.placeholder "Search Industry"
                                        prop.value state.GicsQuery
                                        prop.onChange (fun (ev: Browser.Types.Event) ->
                                            let target = ev.target :?> Browser.Types.HTMLInputElement
                                            dispatch (UpdateGicsQuery target.value)
                                            let selectElement = document.querySelector("select") :?> Browser.Types.HTMLSelectElement
                                            let filteredOptionsCount =
                                                match state.GicsTaxonomy with
                                                | Some taxonomy ->
                                                    taxonomy
                                                    |> generateBreadcrumbPaths
                                                    |> Map.toList
                                                    |> List.filter (fun (_, path) -> path.ToLower().Contains(target.value.ToLower()))
                                                    |> List.length
                                                | None -> 0
                                            selectElement.size <- if filteredOptionsCount > 0 then filteredOptionsCount else 1
                                        )
                                    ]
                                    Html.button [
                                        prop.className "btn join-item btn-secondary"
                                        prop.text "Clear Search"
                                        prop.onClick (fun _ ->
                                            dispatch (UpdateGicsQuery "")
                                            let selectElement = document.querySelector("select") :?> Browser.Types.HTMLSelectElement
                                            selectElement.size <- 1
                                        )
                                    ]
                                ]
                            ]
                            Html.select [
                                prop.className "w-full max-h-60 overflow-y-auto breadcrumb-select"
                                prop.value state.SignUpForm.Industry
                                prop.onChange (fun (ev: Browser.Types.Event) ->
                                    let target = ev.target :?> Browser.Types.HTMLSelectElement
                                    let selectedCode = target.value
                                    updateTaxonomy selectedCode
                                )
                                prop.children (
                                    Html.option [ prop.value ""; prop.text "Select an Industry" ] ::
                                    (filteredOptions
                                     |> List.mapi (fun index (code, path) ->
                                         Html.option [
                                             prop.value code
                                             prop.text path
                                             prop.className (if index % 2 = 0 then "bg-base-100" else "bg-base-300")
                                         ]
                                     )
                                    )
                                )
                            ]
                            Html.div [
                                prop.className "flex justify-end mt-4"
                                prop.children [
                                    Html.button [
                                        prop.className "btn btn-sm btn-secondary"
                                        prop.text "Cancel"
                                        prop.onClick (fun _ -> dispatch ToggleIndustryModal)
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
    ]