﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using RuterBot_LUIS.Dialogs;

namespace Ruterbot_LUIS
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        internal const string WelcomeOption1 = "Yes, need ticket";
        internal const string WelcomeOption2 = "No, already have one";

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            Activity reply = null;

            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new PurchaseTicketLuisDialog());
            }
            else if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels

                if (activity.MembersAdded.Any())
                {
                    reply = activity.CreateReply();
                    reply.AddHeroCard(
                        "Hello Ruter Traveller!",
                        "Could it be that you need a ticket?",
                        new string[] { WelcomeOption1, WelcomeOption2 },
                        new[] { "https://lh3.ggpht.com/lVttUHYvJByHDpFhdgkfx_7hR-7KqnArrhkgCYRJ-m1Ge4f2UXSXgHeG1spQF8ESZ-Q=w200" });

                    await Conversation.SendAsync(activity, () => new PurchaseTicketLuisDialog());
                }
            }
            else
            {
                reply = HandleSystemMessage(activity);
            }

            if (reply != null)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                await connector.Conversations.ReplyToActivityAsync(reply);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels

                if (message.MembersAdded.Any())
                {
                    /*
                    var reply = message.CreateReply(
                        "Hello!\nCould it be that you need a ticket? It's important that everyone contributes.");
                    return reply;
                    */
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}