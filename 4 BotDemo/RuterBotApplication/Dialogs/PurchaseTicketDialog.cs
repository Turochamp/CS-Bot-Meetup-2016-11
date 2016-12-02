using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace RuterBotApplication.Dialogs
{
    [Serializable]
    public class PurchaseTicketDialog : IDialog<object>
    {
        public PurchaseTicketDialog()
        {
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        const string WelcomeOption1 = "Yes, need ticket";
        const string WelcomeOption2 = "No, already have one";
        const string TravelOption1 = "Within Oslo";
        const string TravelOption2 = "Elsewhere";
        const string PurchaseOption1 = "Purchase ticket";
        const string PurchaseOption2 = "Cancel";

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            var reply = context.MakeMessage();
            switch (message.Text)
            {
                case WelcomeOption1:
                {
                    reply.AddHeroCard("Sure thing. Where are you traveling?", new string[] { TravelOption1, TravelOption2 });
                    break;
                }
                case TravelOption1:
                {
                    reply.AddHeroCard(
                        "Best ticket then is 'Single ticket for Zone 1' which is 32,-\nYou can pay with your Visa 6137.",
                        new string[] { PurchaseOption1, PurchaseOption2 });
                    break;
                }
                case PurchaseOption1:
                {
                    FormatReceiptReply(reply);

                    /*
                    reply.Text = string.Format(
                        "Ticket purchased. Your ticket is valid to {0}",
                        DateTime.Now.AddMinutes(60));
                    */
                    break;
                }
                default:
                {
                    reply.AddHeroCard(
                        "Hello Ruter Traveller!",
                        "Could it be that you need a ticket?",
                        new string[] { WelcomeOption1, WelcomeOption2 },
                        new[] { "https://lh3.ggpht.com/lVttUHYvJByHDpFhdgkfx_7hR-7KqnArrhkgCYRJ-m1Ge4f2UXSXgHeG1spQF8ESZ-Q=w200" });
                    break;
                }
            }

            await context.PostAsync(reply);
            context.Wait(this.MessageReceivedAsync);
        }

        // Borrowed from https://docs.botframework.com/en-us/csharp/builder/sdkreference/attachments.html
        private void FormatReceiptReply(IMessageActivity reply)
        {
            reply.Attachments = new List<Attachment>();
            ReceiptItem lineItem1 = new ReceiptItem()
            {
                Subtitle = "Single Zone 1",
                Quantity = "1",
                Price = "32,-"
            };
            List<ReceiptItem> receiptList = new List<ReceiptItem>();
            receiptList.Add(lineItem1);
            ReceiptCard plCard = new ReceiptCard()
            {
                Title = string.Format(
                    "Ticket purchased. Your ticket is valid to {0}",
                    DateTime.Now.AddMinutes(60)),
                Items = receiptList,
                Total = "32,-",
                Tax = "0,-"
            };
            Attachment plAttachment = plCard.ToAttachment();
            reply.Attachments.Add(plAttachment);
        }
    }
}