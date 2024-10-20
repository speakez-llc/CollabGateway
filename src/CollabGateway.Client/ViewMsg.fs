module CollabGateway.Client.ViewMsg

open Router
open CollabGateway.Shared.API


type ViewMsg =
    | ShowToast of Toast
    | HideToast of Toast
    | UrlChanged of Page
    | ProcessPageVisited of PageName
    | ProcessButtonClicked of ButtonName
    | ProcessSession
    | ProcessSessionClose
    | ProcessUserClientIP
    | ResetCookiePolicy