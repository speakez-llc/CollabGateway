module CollabGateway.Server.SendGridHelpers

open System
open System.Net
open CollabGateway.Shared.API
open CollabGateway.Shared.Errors
open SendGrid
open SendGrid.Helpers.Mail

let transmitContactForm (contactForm: ContactForm) =
    task {
        try
            let apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
            if String.IsNullOrEmpty(apiKey) then
                return! ServerError.failwith (ServerError.Exception "SendGrid API key is not set.")
            else
                let client = SendGridClient(apiKey)
                let from = EmailAddress("engineering@speakez.net", "Engineering Team")
                let subject = "New Contact Form Submission"
                let toAddress = EmailAddress("engineering@speakez.net", "Engineering Team")
                let plainTextContent = $"Name: %s{contactForm.Name}\nEmail: %s{contactForm.Email}\nMessage: %s{contactForm.MessageBody}"
                let htmlContent = $"<strong>Name:</strong> %s{contactForm.Name}<br><strong>Email:</strong> %s{contactForm.Email}<br><strong>Message:</strong> %s{contactForm.MessageBody}"
                let msg = MailHelper.CreateSingleEmail(from, toAddress, subject, plainTextContent, htmlContent)
                let! response = client.SendEmailAsync(msg)
                if response.StatusCode = HttpStatusCode.OK || response.StatusCode = HttpStatusCode.Accepted then
                    return "Email sent successfully"
                else
                    return! ServerError.failwith (ServerError.Exception $"Failed to send email: {response.StatusCode}")
        with
        | ex ->
            return! ServerError.failwith (ServerError.Exception $"Failed to send email: {ex.Message}")
    }
    |> Async.AwaitTask

let transmitSignUpForm (contactForm: SignUpForm) =
    task {
        try
            let apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
            if String.IsNullOrEmpty(apiKey) then
                return! ServerError.failwith (ServerError.Exception "SendGrid API key is not set.")
            else
                let client = SendGridClient(apiKey)
                let from = EmailAddress("engineering@speakez.net", "Engineering Team")
                let subject = "New SignUp Form Submission"
                let toAddress = EmailAddress("engineering@speakez.net", "Engineering Team")
                let plainTextContent = $"Name: %s{contactForm.Name}\n
                                            Email: %s{contactForm.Email}\n
                                            Job Title: %s{contactForm.JobTitle}\n
                                            Phone: %s{contactForm.Phone}\n
                                            Department: %s{contactForm.Department}\n
                                            Company: %s{contactForm.Company}\n
                                            Street Address 1: %s{contactForm.StreetAddress1}\n
                                            Street Address 2: %s{contactForm.StreetAddress2}\n
                                            City: %s{contactForm.City}\n
                                            State/Province: %s{contactForm.StateProvince}\n
                                            Post Code: %s{contactForm.PostCode}\n
                                            Country: %s{contactForm.Country}"
                let htmlContent = $"<strong>Name:</strong> %s{contactForm.Name}<br>
                                    <strong>Email:</strong> %s{contactForm.Email}<br>
                                    <strong>Job Title:</strong> %s{contactForm.JobTitle}<br>
                                    <strong>Phone:</strong> %s{contactForm.Phone}<br>
                                    <strong>Department:</strong> %s{contactForm.Department}<br>
                                    <strong>Company:</strong> %s{contactForm.Company}<br>
                                    <strong>Street Address 1:</strong> %s{contactForm.StreetAddress1}<br>
                                    <strong>Street Address 2:</strong> %s{contactForm.StreetAddress2}<br>
                                    <strong>City:</strong> %s{contactForm.City}<br>
                                    <strong>State/Province:</strong> %s{contactForm.StateProvince}<br>
                                    <strong>Post Code:</strong> %s{contactForm.PostCode}<br>
                                    <strong>Country:</strong> %s{contactForm.Country}"
                let msg = MailHelper.CreateSingleEmail(from, toAddress, subject, plainTextContent, htmlContent)
                let! response = client.SendEmailAsync(msg)
                if response.StatusCode = HttpStatusCode.OK || response.StatusCode = HttpStatusCode.Accepted then
                    return "Email sent successfully"
                else
                    return! ServerError.failwith (ServerError.Exception $"Failed to send email: {response.StatusCode}")
        with
        | ex ->
            return! ServerError.failwith (ServerError.Exception $"Failed to send email: {ex.Message}")
    }
    |> Async.AwaitTask