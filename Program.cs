using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTgInfo
{
    class Program
    {

        private static ITelegramBotClient _botClient;
        private static ReceiverOptions _receiverOptions;
        private static string apiToken = "7283410447:AAEcWC3I9IeaP-UrJlo4m69VHr7tT27mCiA";
        private static string changeToken = "2504c5fb-05a4-4e77-8241-e87582a15eac";
        private static string ChanteType = "ChangeUrl";
        private static string EuropeLink = "";
        private static string UsaLink = "";
        private static string CISLink = "";


        static async Task Main()
        {

            _botClient = new TelegramBotClient(apiToken);
            _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                UpdateType.Message,
                },
                // Параметр, отвечающий за обработку сообщений, пришедших за то время, когда ваш бот был оффлайн
                // True - не обрабатывать, False (стоит по умолчанию) - обрабаывать
                ThrowPendingUpdates = false,
            };

            using var cts = new CancellationTokenSource();

            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token); // Запускаем бота

            var me = await _botClient.GetMeAsync();
            Console.WriteLine($"{me.FirstName} запущен!");

            await Task.Delay(-1);
        }

        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            try
            {
                var a = 1;

                switch (update.Type)
                {
                    case UpdateType.Message:
                        {
                            var message = update.Message;
                            var chat = message.Chat;

                            if (message.Text == "/start")
                            {
                                var user = message.From;

                                Console.WriteLine($"{user.FirstName} ({user.Id}) написал сообщение: {message.Text}");


                                await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    "Приветствую! Для дальнейшего использования бота подтвердите свой возраст. Сколько вам полных лет?."
                                    );


                            }
                            else if (Int32.TryParse(message.Text, out int age))
                            {
                                if (age >= 21)
                                {
                                    await botClient.SendTextMessageAsync(
                                        chat.Id,
                                        text: $"Спасибо, вы подтвердили свой возраст.");
                                    SendCountryButtons(botClient, chat.Id);
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(chat.Id, text: "Извините. Этот бот недоступен для Вас.");
                                }

                            }
                            else if (message.Text == "Европа 🇪🇺")
                            {
                                await botClient.SendTextMessageAsync(
                                    chatId: chat.Id,
                                    text: $"Ваш доступ: {EuropeLink}",
                                    parseMode: ParseMode.Markdown
                                );
                            }
                            else if (message.Text == "США 🇺🇸")
                            {
                                await botClient.SendTextMessageAsync(
                                    chatId: chat.Id,
                                    text: $"Ваш доступ: {UsaLink}",
                                    parseMode: ParseMode.Markdown
                                );
                            }
                            else if (message.Text == "СНГ 🌍")
                            {
                                await botClient.SendTextMessageAsync(
                                    chatId: chat.Id,
                                    text: $"Ваш доступ: {CISLink}",
                                    parseMode: ParseMode.Markdown
                                );
                            }
                            else if (message.Text.Contains(changeToken))
                            {
                                if (message.Text.ToLower().Contains(ChanteType.ToLower()))
                                {
                                    // format ChangeUrl::token::country::newUrl
                                    var changeUrlArr = message.Text.Split("::");
                                    if (changeUrlArr.Length < 4)
                                    {
                                        await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    text: "неверный формат смены url, formatL: ChangeUrl::token::country::newUrl. Counties: europe, usa, cis."
                                     );
                                    }
                                    else
                                    {
                                        if (changeUrlArr[2].ToLower() == "europe")
                                        {
                                            EuropeLink = changeUrlArr[3];
                                        }
                                        else if (changeUrlArr[2].ToLower() == "usa")
                                        {
                                            UsaLink = changeUrlArr[3];
                                        }
                                        else if (changeUrlArr[2].ToLower() == "cis")
                                        {
                                            CISLink = changeUrlArr[3];
                                        }
                                        else
                                        {
                                            await botClient.SendTextMessageAsync(chat.Id, text: $"Не удалось распознать страну. Counties format: europe, usa, cis.");
                                            return;
                                        }
                                        await botClient.SendTextMessageAsync(chat.Id, text: $"Url для страны {changeUrlArr[2].ToLower()} был успешно сменён на {changeUrlArr[3].ToLower()}.");
                                    }

                                }
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    text: "Извините, я не понял вашего сообщения, возможно Вы ввели некорректно Ваш возраст."
                                );
                            }
                            return;
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                RestartBotReceiving(botClient);
            }
        }

        private static async Task SendCountryButtons(ITelegramBotClient botClient, long chatId)
        {

            var replyKeyboard = new ReplyKeyboardMarkup(
                                    new List<KeyboardButton[]>()
                                    {
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("Европа 🇪🇺"),
                                        },
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("США 🇺🇸")
                                        },
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("СНГ 🌍")
                                        }
                                    })
            {

                ResizeKeyboard = true,
            };

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Выберите ваш регион проживания:",
                replyMarkup: replyKeyboard
            );
        }
        private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            // Тут создадим переменную, в которую поместим код ошибки и её сообщение 
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
        private static void RestartBotReceiving(ITelegramBotClient botClient)
        {
            // Restart bot receiving here
            botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, new CancellationTokenSource().Token);
        }
    }
}