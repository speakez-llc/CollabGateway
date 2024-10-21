module CollabGateway.Client.DataPolicyModal

open Feliz
open CollabGateway.Client.ViewMsg
open CollabGateway.Shared.API

let closeTab () =
    Browser.Dom.window.close()

let DataPolicyModal (state: DataPolicyChoice) (parentDispatch: ViewMsg -> unit) =

    Html.div [
        prop.className "fixed inset-0 flex items-center justify-center bg-gray-800 bg-opacity-75 z-50 pointer-events-auto"
        prop.children [
            Html.div [
                prop.className "bg-base-100 p-6 rounded-lg shadow-lg w-5/6 md:w-1/2"
                prop.children [
                    Html.img [
                        prop.src "/img/Collab_Logo_narrow.svg"
                        prop.alt "Header Image"
                        prop.className "w-full md:w-3/4 h-auto mb-4 rounded-t-lg mx-auto"
                    ]
                    Html.h2 [
                        prop.className "text-xl font-bold mb-4"
                        prop.text "Data Collection & Tracking"
                    ]
                    Html.div [
                        prop.children [
                            match state with
                            | Declined ->
                                Html.p [
                                    prop.text "We're sorry we could not help you at this time, and hope that you may reconsider in the future. If you would like to continue the conversation outside of this website, please feel free to reach out any time."
                                ]
                                Html.p [
                                    prop.className "pt-2"
                                    prop.text "collab@rowerconsulting.com"
                                ]
                            | _ ->
                                Html.div [
                                    Html.p [
                                        prop.text "We use in-house telemetry systems to operate this site and manage contact with you. No third-party cookies or outside site tracking of any kind are used. Our data collection is scoped to the following:"
                                    ]
                                    Html.ul [
                                        prop.className "list-disc ml-6 pt-4 pb-4"
                                        prop.children [
                                            Html.li [ prop.text "Monitor system performance" ]
                                            Html.li [ prop.text "Enhance navigation and features" ]
                                            Html.li [ prop.text "Provide 'smart' features on this site" ]
                                            Html.li [ prop.text "Assist in communication with you" ]
                                        ]
                                    ]
                                    Html.p [
                                        prop.text "We will never provide your information to outside parties, ever. And further, we also won't share your details with technology partners listed on this site without your expressed consent. Any personal information that you may send to us in the course of using this site can be later removed from our systems permanently at your request. If you have any questions or concerns, please reach out to us directly."
                                    ]
                                    Html.p [
                                        prop.className "pt-2"
                                        prop.text "collab@rowerconsulting.com"
                                    ]
                                ]
                        ]
                    ]
                    Html.div [
                        prop.className "mt-4 flex justify-end"
                        prop.children [
                            match state with
                            | Declined ->
                                Html.button [
                                    prop.className "btn btn-primary"
                                    prop.onClick (fun _ -> parentDispatch (ProcessButtonClicked DataPolicyResetButton))
                                    prop.text "Reset"
                                ]
                            | _ ->
                                Html.button [
                                    prop.className "btn btn-secondary mr-2"
                                    prop.onClick (fun _ -> parentDispatch (ProcessButtonClicked DataPolicyDeclineButton))
                                    prop.text "Decline"
                                ]
                                Html.button [
                                    prop.className "btn btn-primary"
                                    prop.onClick (fun _ -> parentDispatch (ProcessButtonClicked DataPolicyAcceptButton))
                                    prop.text "Accept"
                                ]
                        ]
                    ]
                ]
            ]
        ]
    ]