module CollabGateway.Client.ViewMsg

open CollabGateway.Shared.Events
open CollabGateway.Shared.API
open Router


type ViewMsg =
    | ShowToast of Toast
    | HideToast of Toast
    | UrlChanged of Page
    | ProcessPageVisited of PageName
    | ProcessButtonClicked of ButtonName
    | ProcessStream
    | ProcessStreamClose
    | ProcessUserClientIP
    | DataPolicyChoiceRetrieved of DataPolicyChoice