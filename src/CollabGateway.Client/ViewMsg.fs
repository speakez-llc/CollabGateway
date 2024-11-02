module CollabGateway.Client.ViewMsg

open CollabGateway.Shared.Events
open CollabGateway.Shared.API
open Router


type ViewMsg =
    | ShowToast of string * AlertLevel
    | HideToast of int
    | UrlChanged of Page
    | ProcessPageVisited of PageName
    | ProcessButtonClicked of ButtonName
    | ProcessStream
    | ProcessStreamClose
    | ProcessUserClientIP
    | DataPolicyChoiceRetrieved of DataPolicyChoice