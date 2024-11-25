module CollabGateway.Client.Components.IndustryModal

open CollabGateway.Shared.API
open Feliz

type Props = {
    GicsTaxonomy: GicsTaxonomy[]
    OnSectorChange: string -> unit
}

let IndustryModal (props: Props) =
    let selectedSector, setSelectedSector = React.useState<string option>(None)

    let sectors = props.GicsTaxonomy |> Array.map (fun g -> g.SectorName) |> Array.distinct

    Html.div [
        prop.className "fixed inset-0 flex items-center justify-center bg-gray-800 bg-opacity-75 z-50 pointer-events-auto"
        prop.children [
            Html.div [
                prop.className "bg-base-100 p-6 rounded-lg shadow-lg w-5/6 md:w-1/2 max-h-screen overflow-y-auto"
                prop.children [
                    Html.h2 [
                        prop.className "text-xl font-bold mb-4"
                        prop.text "Select Industry"
                    ]
                    Html.select [
                        prop.value (selectedSector |> Option.defaultValue "")
                        prop.onChange (fun (ev: Browser.Types.Event) ->
                            let target = ev.target :?> Browser.Types.HTMLSelectElement
                            setSelectedSector (Some target.value)
                            props.OnSectorChange target.value
                        )
                        prop.children (
                            Html.option [ prop.value ""; prop.text "Select Sector" ] ::
                            (sectors |> Array.map (fun sector -> Html.option [ prop.value sector; prop.text sector ]) |> Array.toList)
                        )
                    ]
                ]
            ]
        ]
    ]