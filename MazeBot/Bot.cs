using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.InlineQueryResults;
using MazeEngine;
using MazeCreator.Core;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;

namespace MazeBot
{
    public class Bot
    {
        private const int resolution = 1024;

        private TelegramBotClient bot;

        private Dictionary<long, MazeDrawer> mazeDrawers = new Dictionary<long, MazeDrawer>();

        public void Start() => bot.StartReceiving();

        public void Stop() => bot.StopReceiving();

        public Bot(string TelegramApiToken)
        {
            bot = new TelegramBotClient(TelegramApiToken);

            bot.SetWebhookAsync("");

            bot.OnCallbackQuery += async (object updobj, CallbackQueryEventArgs cqea) =>
            {
                var msgId = cqea.CallbackQuery.Message.MessageId;

                var chatId = cqea.CallbackQuery.Message.Chat.Id;

                try
                {
                    switch (cqea.CallbackQuery.Data)
                    {
                        case "10x10":
                            newMaze(chatId, 10);
                            await bot.SendPhotoAsync(chatId, getImageStream(chatId), replyMarkup: getNavigationKeyboard());
                            break;

                        case "20x20":
                            newMaze(chatId, 20);
                            await bot.SendPhotoAsync(chatId, getImageStream(chatId), replyMarkup: getNavigationKeyboard());
                            break;

                        case "50x50":
                            newMaze(chatId, 50);
                            await bot.SendPhotoAsync(chatId, getImageStream(chatId), replyMarkup: getNavigationKeyboard());
                            break;

                        case "map":
                            updateImage(chatId, msgId, true);
                            break;

                        case "up":
                            if (getMaze(chatId).StepForward())
                                updateImage(chatId, msgId);
                            break;

                        case "down":
                            if (getMaze(chatId).StepBackward())
                                updateImage(chatId, msgId);
                            break;

                        case "left":
                            getMaze(chatId).TurnLeft();
                            updateImage(chatId, msgId);
                            break;

                        case "right":
                            getMaze(chatId).TurnRight();
                            updateImage(chatId, msgId);
                            break;
                    }
                }
                catch (Exception e)
                {
                    await bot.AnswerCallbackQueryAsync(cqea.CallbackQuery.Id, e.InnerException.Message);
                }
            };

            bot.OnMessage += async (object updobj, MessageEventArgs mea) =>
            {
                if (mea.Message.Text == null)
                    return;

                var chatId = mea.Message.Chat.Id;

                string command = mea.Message.Text.ToLower().Replace("@mazeukrbot", "").Replace("/", "");

                switch (command)
                {
                    case "start":
                        await bot.SendTextMessageAsync(chatId, "Вітаю! Я @MazeUkrBot!\nЯкий лабіринт ви подужаєте?", replyMarkup: new InlineKeyboardMarkup(new[]
                            {
                                InlineKeyboardButton.WithCallbackData("10x10", "10x10"),
                                InlineKeyboardButton.WithCallbackData("20x20", "20x20"),
                                InlineKeyboardButton.WithCallbackData("50x50", "50x50"),
                            }));
                        break;
                }
            };
        }

        private void newMaze(long id, int mazeSize) => mazeDrawers[id] = 
            new MazeDrawer(resolution, mazeSize)
            {
                emptyFrame = Color.Gray,
                startBorder = Color.YellowGreen,
                startSurface = Color.YellowGreen,
                finishBorder = Color.DeepPink,
                finishSurface = Color.DeepPink,
                wallBorder = Color.DarkGreen,
                wallSurface = Color.LightGreen,
                ceilingBorder = Color.SteelBlue,
                ceilingSurface = Color.LightSteelBlue,
                floorBorder = Color.DarkBlue,
                floorSurface = Color.Blue
            };

        private MazeDrawer getMaze(long id)
        {
            if (mazeDrawers.ContainsKey(id))
                return mazeDrawers[id];

            newMaze(id, 10);

            return mazeDrawers[id];
        }

        private InlineKeyboardMarkup getNavigationKeyboard() => new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("⬅️", "left"),
                InlineKeyboardButton.WithCallbackData("⬆️", "up"),
                InlineKeyboardButton.WithCallbackData("🗺", "map"),
                InlineKeyboardButton.WithCallbackData("⬇️", "down"),
                InlineKeyboardButton.WithCallbackData("➡️", "right"),
            });

        private Stream getImageStream(long id, bool imageModeSwitch = false)
        {
            var stream = new MemoryStream();

            getMaze(id).GetImage(imageModeSwitch).Save(stream, ImageFormat.Jpeg);

            stream.Position = 0;

            return stream;
        }

        private void updateImage(long id, int messageId, bool imageModeSwitch = false)
        {
            var m = new InputMediaPhoto(new InputMedia(getImageStream(id, imageModeSwitch), "RenderedImage"));

            bot.EditMessageMediaAsync(id, messageId, m, replyMarkup: getNavigationKeyboard()).Wait();
        }
    }
}
