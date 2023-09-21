using Azure.Communication.Email;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AzureTestFunc
{
    public static class Function1
    {
        private const string _emailConnection = "";
        [FunctionName("MyTestFunctionVS")]
        public static void Run([BlobTrigger("blobexample/{name}", Connection = "AzureWebJobsStorage")] BlobClient myBlob, string name, ILogger log)
        {
            BlobProperties blobProperties = myBlob.GetProperties();

            var email = blobProperties.Metadata["email"];

            SendEmail(email, log);
            log.LogInformation($"Function triggered and it's been a success");
        }

        public static void SendEmail(string email, ILogger logger)
        {
            EmailClient emailClient = new EmailClient(_emailConnection);

            EmailContent emailContent = new EmailContent("Hello user, welcome to azure communication server");
            emailContent.PlainText = "The email message is sent from Azure using Azure Funcrions and .Net sdk";
            List<EmailAddress> emailAddresses = new List<EmailAddress>() { new EmailAddress(email) };
            EmailRecipients recipients = new EmailRecipients(emailAddresses);
            EmailMessage emailMessage = new EmailMessage("DoNotReply@a1ea72b2-3f6b-4b74-a926-778e3a0ae099.azurecomm.net", recipients, emailContent);
            logger.LogInformation("Email is send to the reciver");

            try
            {
                EmailSendOperation emailSendOperation = emailClient.Send(Azure.WaitUntil.Completed, emailMessage, CancellationToken.None);
                var res = emailSendOperation.WaitForCompletion(CancellationToken.None);

                while (!emailSendOperation.HasCompleted)
                {
                    logger.LogInformation("Operation status is: " + res.Value.Status.ToString());
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Something went wrong while sending message! {0}", ex.Message);
            }

            logger.LogInformation("Email is successfuly send!");
        }
    }
}
