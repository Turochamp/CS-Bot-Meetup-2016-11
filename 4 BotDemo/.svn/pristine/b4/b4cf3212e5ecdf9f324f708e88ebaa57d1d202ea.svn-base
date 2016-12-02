using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace RuterBot_LUIS.Dialogs
{
    [LuisModel("f0af214e-cfc0-4b43-8c06-6517cee99305", "b39807c6060549749b12449ff3eaf1c2")]
    [Serializable]
    public class PurchaseTicketLuisDialog : LuisDialog<object>
    {
        delegate Task LuisIntentHandler(IDialogContext context, LuisResult result);

        public PurchaseTicketLuisDialog()
        {
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry I did not understand: " + string.Join(", ", result.Intents.Select(i => i.Intent));
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("ContinueTicketPurchase")]
        public async Task ContinueTicketPurchase(IDialogContext context, LuisResult result)
        {
            var m = context.MakeMessage();

            await context.PostAsync("Sure thing. Where are you traveling?");
            context.Wait(MessageReceived);
        }

        [LuisIntent("CancelTicketPurchase")]
        public async Task CancelTicketPurchase(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Ok, ask me for updated directions and other cool things. Enjoy your trip :-)");
            context.Wait(MessageReceived);
        }

        [LuisIntent("BuyTicket")]
        public async Task BuyTicket(IDialogContext context, LuisResult result)
        {
            string destination = GetStringValue(context.ConversationData, "destination");
            string zone = GetStringValue(context.ConversationData, "zone");

            if (string.IsNullOrEmpty(destination) && string.IsNullOrEmpty(zone))
            {
                await context.PostAsync("Sure thing. Where are you traveling?");
                context.Wait(MessageReceived);
            }
            else
            {
                PromptDialog.Confirm(context, AfterConfirming_BuyTicket, "Purchase ticket?", promptStyle: PromptStyle.None);
            }
        }

        public async Task AfterConfirming_BuyTicket(IDialogContext context, IAwaitable<bool> confirmation)
        {
            if (await confirmation)
            {
                var message = context.MakeMessage();
                FormatReceiptReply(message);
                await context.PostAsync(message);
            }
            else
            {
                await context.PostAsync("Ok! No ticket purchased");
            }

            context.Wait(MessageReceived);
        }

        private string GetStringValue(IBotDataBag dataBag, string key)
        {
            string value;
            return dataBag.TryGetValue(key, out value) ? value : null;
        }

        [LuisIntent("SetDestination")]
        public async Task SetDestination(IDialogContext context, LuisResult result)
        {
            EntityRecommendation er;
            if (result.TryFindEntity("Destination", out er))
            {
                // Add destination to context
                context.ConversationData.SetValue("destination", er.Entity);
                var text = string.Format("Best ticket then is 'Single ticket to {0}' which is 32,-", er.Entity);
                await ConfirmBuyTicket(context, text);
            }
            else
            {
                await context.PostAsync("Didn't understand. Which destination?");
                context.Wait(MessageReceived);
            }
        }

        [LuisIntent("SetZone")]
        public async Task SetZone(IDialogContext context, LuisResult result)
        {
            EntityRecommendation er;
            if (result.TryFindEntity("Zone", out er))
            {
                // Add zone to context
                context.ConversationData.SetValue("zone", er.Entity);
                var text = string.Format("To {0}. Best ticket then is 'Single ticket for <zone>' which is 32,-", er.Entity);
                await ConfirmBuyTicket(context, text);
            }
            else
            {
                await context.PostAsync("Didn't understand. Which zone?");
                context.Wait(MessageReceived);
            }
        }

        private async Task ConfirmBuyTicket(IDialogContext context, string text)
        {
            await context.PostAsync(text + "\nYou can pay with your Visa 6137.");
            PromptDialog.Confirm(context, AfterConfirming_BuyTicket, "Purchase ticket?", promptStyle: PromptStyle.Auto);
        }

        [LuisIntent("Appreciation")]
        public async Task Appreciation(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Happy to help. Wanna play a game...");
            context.Wait(MessageReceived);
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