module CollabGateway.Shared.Types.EmailAddress

type T = private EmailAddress of string

let toString (EmailAddress email) = email

let tryParse (text: string) =
    if text.Contains("@") && text.IndexOf(".") > text.IndexOf("@") then
        Ok text
    else
        Error "The e-mail address must contain a '@' symbol followed by a '.' symbol"