module CollabGateway.Client.Components.Carousel

open Feliz

[<ReactComponent>]
let Carousel (images: string seq) =
    let currentIndex, setCurrentIndex = React.useState(0)
(*
    React.useEffectOnce(fun () ->
        let intervalId = Browser.Dom.window.setInterval((fun () ->
            setCurrentIndex(fun prevIndex -> (prevIndex + 1) % images.Length)
        ), 3000)
        fun () -> Browser.Dom.window.clearInterval(intervalId) |> ignore
    )
*)
    Html.div [
        prop.className "carousel w-full"
        prop.children [
            for image in images do
                Html.div [
                    //prop.className (if images.[currentIndex] = image then "carousel-item block" else "carousel-item hidden")
                    prop.children [
                        Html.img [
                            prop.src image
                            prop.className "w-full"
                        ]
                    ]
                ]
        ]
    ]