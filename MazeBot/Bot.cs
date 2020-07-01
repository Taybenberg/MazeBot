using System;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.InlineQueryResults;

namespace MazeBot
{
    public class Bot
    {
        private TelegramBotClient bot;

        public void Start() => bot.StartReceiving();

        public void Stop() => bot.StopReceiving();

        public Bot(string TelegramApiToken)
        {
            bot = new TelegramBotClient(TelegramApiToken);

            bot.SetWebhookAsync("");

            bot.OnInlineQuery += async (object updobj, InlineQueryEventArgs iqea) =>
            {

            };

            bot.OnMessage += async (object updobj, MessageEventArgs mea) =>
            {

            };
        }
    }
}
