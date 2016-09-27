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
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            var title = message.Text;
            var repo = new GameRepository();

            var gameList = await repo.GetGameList();
            var gameSummary = gameList.GetGameSummary(title);
            if (gameSummary != null)
            {
                context.Call(new GameBookPlayDialog(gameSummary), AfterPlayAsync);
            }
            else
            {
                var gameListText = string.Join(Environment.NewLine, gameList.Games.Select(x => $"- {x.Title}"));
                await context.PostAsync($"どのゲームをプレイする？\n\n{ gameListText }");
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task AfterPlayAsync(IDialogContext context, IAwaitable<object> argument)
        {
            await context.PostAsync("サンキューフォープレイング。");
            context.Wait(MessageReceivedAsync);
        }
    }

    [Serializable]
    public class GameBookPlayDialog : IDialog<object>
    {
        public GameBookPlayDialog(GameSummary gameSummary)
        {
            gameContext = new GameContext()
            {
                GameSummary = gameSummary
            };
        }

        protected GameContext gameContext { get; set; }

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(gameContext.GameSummary.Title + " を開始します。");
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            var command = message.Text;

            var game = await new GameRepository().GetGame(gameContext.GameSummary.Title);
            var paragraph = game.NextParagraph(gameContext, command);
            await context.PostAsync(paragraph.GetMessage(gameContext));
            if (paragraph.IsGameOver(gameContext))
            {
                context.Done(new object());
            }
            else
            {
                context.Wait(MessageReceivedAsync);
            }
        }
    }
}