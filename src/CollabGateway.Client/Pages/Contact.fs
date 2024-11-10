module CollabGateway.Client.Pages.Contact

open System
open Browser.Dom
open Feliz

open Elmish
open CollabGateway.Client.Server
open CollabGateway.Client.ViewMsg
open CollabGateway.Shared.API
open UseElmish

type private State = {
    IsFormSubmitComplete: bool
    IsEmailVerified: bool
    ContactForm: ContactForm
    ResponseMessage: string
    InFormMessage: string
    Errors: Map<string, string>
    IsEmailValid: bool option
    IsWebmailDomain: bool option
    IsProcessing: bool
    IsSubmitActive: bool
    CurrentStep: int
}

type private Msg =
    | UpdateName of string
    | UpdateEmail of string
    | UpdateMessageBody of string
    | SubmitForm
    | FormSubmitted of ServerResult<string>
    | WebmailDomainFlagged of bool
    | ParentDispatch of ViewMsg
    | CheckEmailVerification
    | EmailVerificationChecked of bool

let private checkFormSubmissionAndEmailStatus sessionToken =
    async {
        let! formSubmissionExists = service.RetrieveContactFormSubmitted sessionToken
        let! emailStatus = service.RetrieveEmailStatus sessionToken
        let isEmailVerified = emailStatus |> Option.exists (List.exists (fun (_, _, s) -> s = Verified))
        return formSubmissionExists, isEmailVerified
    }

let private init () =
    let initialContactForm = {
        Name = ""
        Email = ""
        MessageBody = ""
    }
    let initialState = { IsFormSubmitComplete = false; IsEmailVerified = false; ContactForm = initialContactForm; InFormMessage = "Feel Free To Reach Out"; ResponseMessage = ""; Errors = Map.empty; IsEmailValid = None; IsWebmailDomain = None; IsProcessing = false; IsSubmitActive = false; CurrentStep = 1 }

    let sessionToken = Guid.Parse (window.localStorage.getItem("UserStreamToken"))
    let cmd = Cmd.OfAsync.perform (fun () -> checkFormSubmissionAndEmailStatus sessionToken) () (fun (formSubmissionExists, isEmailVerified) ->
        if formSubmissionExists then
            if isEmailVerified then
                EmailVerificationChecked true
            else
                FormSubmitted (Ok "Form submission exists")
        else
            EmailVerificationChecked false
    )

    initialState, cmd

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

let private validateForm (contactForm: ContactForm) =
    let submittedEmailIsValid = isEmailValid contactForm.Email
    let cmd =
        if submittedEmailIsValid then
            Cmd.OfAsync.perform (fun () -> flaggedWebmailDomain contactForm.Email) () WebmailDomainFlagged
        else
            Cmd.none

    let errors =
        [ if String.IsNullOrWhiteSpace(contactForm.Name) then Some("Name is required") else None
          if String.IsNullOrWhiteSpace(contactForm.Email) then Some("Email is required") else None
          if String.IsNullOrWhiteSpace(contactForm.MessageBody) then Some("Message is required") else None
          if not submittedEmailIsValid then Some("Email is not valid") else None ]
        |> List.choose id

    let isSubmitActive =
        List.isEmpty errors && submittedEmailIsValid

    errors, not (List.isEmpty errors), cmd, isSubmitActive

let private checkEmailVerificationWithDelay sessionToken =
    async {
        do! Async.Sleep 2000
        return! service.RetrieveEmailStatus sessionToken
    }

let private update (msg: Msg) (model: State) (parentDispatch: ViewMsg -> unit) : State * Cmd<Msg> =
    match msg with
    | UpdateName name ->
        let newModel = { model with ContactForm = { model.ContactForm with Name = name } }
        let errors, hasErrors, cmd, isSubmitActive = validateForm newModel.ContactForm
        { newModel with Errors = errors |> List.mapi (fun i error -> (i.ToString(), error)) |> Map.ofList; IsSubmitActive = isSubmitActive }, cmd
    | UpdateEmail email ->
        let isEmailValid = isEmailValid email
        let newModel = { model with ContactForm = { model.ContactForm with Email = email }; IsEmailValid = Some isEmailValid }
        let errors, hasErrors, cmd, isSubmitActive = validateForm newModel.ContactForm
        { newModel with Errors = errors |> List.mapi (fun i error -> (i.ToString(), error)) |> Map.ofList; IsSubmitActive = isSubmitActive }, cmd
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
    | UpdateMessageBody messageBody ->
        let newModel = { model with ContactForm = { model.ContactForm with MessageBody = messageBody } }
        let errors, hasErrors, cmd, isSubmitActive = validateForm newModel.ContactForm
        { newModel with Errors = errors |> List.mapi (fun i error -> (i.ToString(), error)) |> Map.ofList; IsSubmitActive = isSubmitActive }, cmd
    | SubmitForm ->
        let errors, hasErrors, cmd, _ = validateForm model.ContactForm
        if hasErrors then
            errors |> List.iter (fun error -> parentDispatch (ShowToast (error, AlertLevel.Warning)))
            { model with Errors = errors |> List.mapi (fun i error -> (i.ToString(), error)) |> Map.ofList }, cmd
        else
            let timeStamp = DateTime.UtcNow
            let sessionToken = Guid.Parse (window.localStorage.getItem("UserStreamToken"))
            let appendEmailStatusCmd =
                if not model.IsEmailVerified then
                    let validationToken = Guid.NewGuid()
                    Cmd.OfAsync.eitherAsResult (fun _ -> service.AppendEmailStatus (timeStamp, sessionToken, validationToken, model.ContactForm.Email, Open)) (fun _ -> FormSubmitted (Ok "Email status appended"))
                else
                    Cmd.none
            let appendSubscriptionStatusCmd =
                if not model.IsEmailVerified then
                    let subscriptionStatus = SubscribeStatus.Open
                    let subscriptionToken = Guid.NewGuid()
                    Cmd.OfAsync.eitherAsResult (fun _ -> service.AppendUnsubscribeStatus (timeStamp, sessionToken, subscriptionToken, model.ContactForm.Email, subscriptionStatus)) (fun _ -> FormSubmitted (Ok "Subscription status appended"))
                else
                    Cmd.none
            let cmd = Cmd.batch [
                Cmd.OfAsync.eitherAsResult (fun _ -> service.ProcessContactForm (timeStamp, sessionToken, model.ContactForm)) FormSubmitted
                appendEmailStatusCmd
                appendSubscriptionStatusCmd
            ]
            { model with IsProcessing = true }, cmd
    | FormSubmitted (Ok response) ->
        { model with IsFormSubmitComplete = true; CurrentStep = 2; IsProcessing = false }, Cmd.ofMsg CheckEmailVerification
    | FormSubmitted (Result.Error ex) ->
        parentDispatch (ShowToast ("Failed to send message", AlertLevel.Error))
        { model with ResponseMessage = $"Failed to submit form: {ex.ToString()}"; IsProcessing = false }, Cmd.none
    | ParentDispatch viewMsg ->
        parentDispatch viewMsg
        model, Cmd.none
    | CheckEmailVerification ->
        let sessionToken = Guid.Parse (window.localStorage.getItem("UserStreamToken"))
        let cmd = Cmd.OfAsync.perform checkEmailVerificationWithDelay sessionToken (fun status -> EmailVerificationChecked (status |> Option.exists (List.exists (fun (_, _, s) -> s = Verified))))
        model, cmd
    | EmailVerificationChecked isVerified ->
        if isVerified then
            { model with IsEmailVerified = true; CurrentStep = 3 }, Cmd.none
        else
            model, Cmd.ofMsg CheckEmailVerification

[<ReactComponent>]
let IndexView (parentDispatch : ViewMsg -> unit) =
    let state, dispatch = React.useElmish((fun () -> init ()), (fun msg model -> update msg model parentDispatch), [| |])

    React.useEffectOnce(fun () ->
        parentDispatch (ProcessPageVisited ContactPage)
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

    let renderStep1 () =
        Html.div [
            prop.className "flex flex-col p-4 space-y-4 transition-opacity duration-900 ease-in-out w-full md:w-4/5  mx-auto max-w-screen-xl"
            prop.children [
                Html.h1 [
                    prop.className "text-2xl font-bold mb-4 mx-auto"
                    prop.text state.InFormMessage
                ]
                Html.div [
                    prop.className "card mx-auto bg-base-200 w-4/5 mx-auto rounded-3xl"
                    prop.children [
                        Html.div [
                            prop.className "p-4 m-2 card-body mx-auto"
                            prop.text "If you're not ready to sign up on our waitlist, you can still let us know you're interested. Use the form below and someone at Rower will respond. We're always happy to hear from you."
                        ]
                    ]
                ]
                Html.input [
                    prop.className "input input-bordered rounded-lg h-10 w-2/3 md:w-1/3 shadow bg-base-300 pl-2 required"
                    prop.placeholder "Name is required"
                    prop.autoComplete "Name"
                    prop.value state.ContactForm.Name
                    prop.onChange (fun (e: Browser.Types.Event) ->
                        let target = e.target :?> Browser.Types.HTMLInputElement
                        dispatch (UpdateName target.value))
                ]
                Html.input [
                    prop.className $"input input-bordered rounded-lg h-10 w-2/3 lg:w-1/3 shadow bg-base-300 pl-2 required {emailInputClass}"
                    prop.placeholder "A non-webmail email is required"
                    prop.autoComplete "Email"
                    prop.value state.ContactForm.Email
                    prop.onChange (fun (e: Browser.Types.Event) ->
                        let target = e.target :?> Browser.Types.HTMLInputElement
                        dispatch (UpdateEmail target.value))
                ]
                Html.textarea [
                    prop.className "input input-bordered rounded-lg h-32 w-full lg:w-1/2 shadow bg-base-300 p-2 required"
                    prop.placeholder "A message is required"
                    prop.value state.ContactForm.MessageBody
                    prop.onChange (fun (e: Browser.Types.Event) ->
                        let target = e.target :?> Browser.Types.HTMLTextAreaElement
                        dispatch (UpdateMessageBody target.value))
                ]
                Html.div [
                    prop.className "flex items-center space-x-2"
                    prop.children [
                        Html.button [
                            prop.className "btn btn-primary h-10 w-full md:w-2/3 lg:w-1/3 text-gray-200 text-xl"
                            prop.text "Get In Touch!"
                            prop.type' "submit"
                            prop.disabled (not state.IsSubmitActive)
                            prop.onClick handleButtonClick
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
                    prop.className "flex items-center space-x-2 justify-center"
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
            prop.className "flex flex-col p-4 space-y-4 transition-opacity duration-900 ease-in-out w-full md:w-4/5 mx-auto max-w-screen-xl"
            prop.children [
                Html.h1 [
                    prop.className "text-2xl font-bold mb-4 mx-auto"
                    prop.text "Thank you for verifying your email"
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
                        parentDispatch (ProcessButtonClicked ContactActivityButton)
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
                        prop.className "flex items-center flex-col steps w-full md:w-3/4 duration-900 ease-in-out max-w-screen-xl lg:steps-horizontal"
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
        renderCurrentStep()
    ]