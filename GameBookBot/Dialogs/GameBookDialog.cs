﻿using Microsoft.Bot.Builder.Dialogs;
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
            var repo = new GameRepository();

            var gameList = await repo.GetGameList();

            // XXX 選択NGだった場合のハンドリング
            PromptDialog.Choice(context, SelectedGameAsync, gameList.Games, "どのゲームをプレイする？");
        }

        public async Task SelectedGameAsync(IDialogContext context, IAwaitable<GameSummary> argument)
        {
            var gameSummary = await argument;
            context.Call(new GameBookPlayDialog(gameSummary), AfterPlayAsync);
        }

        public async Task AfterPlayAsync(IDialogContext context, IAwaitable<object> argument)
        {
            await context.PostToNextMessageAsync("サンキューフォープレイング。");
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
            await context.PostToNextMessageAsync(gameContext.GameSummary.Title + " を開始します。");
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            var command = message.Text;

            await processCommand(context, command);
        }

        private async Task SelectedOptionAsync(IDialogContext context, IAwaitable<Option> argument)
        {
            var option = await argument;
            var command = option.Id;
            await processCommand(context, command);
        }

        private async Task processCommand(IDialogContext context, string command)
        {
            var game = await new GameRepository().GetGame(gameContext.GameSummary.Title);
            var paragraph = game.NextParagraph(gameContext, command);

            if (paragraph.HasImage)
            {
                var imageByBing = await new BingImagesConnector().SearchImage(paragraph.Image.Depiction);
                var imageMessage = context.MakeMessage();
                if (string.IsNullOrEmpty(imageByBing))
                {
                    imageMessage.Text = paragraph.Image.Depiction;
                }
                else
                {
                    imageMessage.Attachments = new List<Attachment>();
                    imageMessage.Attachments.Add(new Attachment()
                    {
                        ContentUrl = imageByBing,
                        ContentType = "image/*",
                        Name = paragraph.Image.Depiction
                    });
                }
                await context.PostAsync(imageMessage);
            }

            if (paragraph.IsGameOver(gameContext))
            {
                await context.PostAsync(paragraph.GetMessage(gameContext));
                context.Done(new object());
            }
            else
            {
                // XXX 選択NGだった場合のハンドリング
                PromptDialog.Choice(context, SelectedOptionAsync, paragraph.GetChoosableOptions(gameContext), paragraph.GetMessage(gameContext));
            }
        }
    }

    public static class BotUtilities
    {
        public static async Task PostToNextMessageAsync(this IDialogContext context, string replyMessage)
        {
            await context.PostAsync(context.MakeToNextMessage(replyMessage));
        }
        public static IMessageActivity MakeToNextMessage(this IDialogContext context, string replyMessage)
        {
            var reply = context.MakeMessage();
            reply.AddHeroCard(replyMessage, new string[] { "▼" });
            return reply;
        }
    }
}