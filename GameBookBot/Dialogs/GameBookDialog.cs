using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace GameBookBot.Dialogs
{
    [Serializable]
    public class GameBookDialog : IDialog<object>
    {
        protected GameContext gameContext { get; set; }

        public async Task StartAsync(IDialogContext context)
        {
            gameContext = new GameContext();
            var game = (await new GameRepository().GetGames()).First();
            await context.PostAsync(game.Title + " を開始します。");
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            var game = (await new GameRepository().GetGames()).First();
            var paragraph = game.NextParagraph(gameContext, message.Text);
            await context.PostAsync(paragraph.GetMessage(gameContext));
            if (paragraph.IsGameOver(gameContext))
            {
                gameContext = new GameContext();
            }

            context.Wait(MessageReceivedAsync);
        }
    }
}