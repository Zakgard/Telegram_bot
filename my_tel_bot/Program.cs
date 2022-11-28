using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace my_tel_bot
{
    class Program
    {
        private static string _token { get; set; } = "5846404784:AAH2BLzWT9qSp86N8lcVjRnvkwbAKAYzHp8";
        
        static void Main(string[] args)
        {
            using var cts = new CancellationTokenSource();
            TelegramBotClient _client = new TelegramBotClient(_token);
            _client.StartReceiving(UpdateHandler,ErrorHabdler,
                cancellationToken: cts.Token);
            Console.WriteLine("Bot is running");
            Console.ReadLine();  
        }

        async private static Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken token)
        {           
            var message = update.Message;
            if(message != null) {
                Console.WriteLine(message.Type);
                switch (message.Type.ToString())
                {
                    case "Text":
                        AnswerTheText(message, botClient);
                        break;
                    case "Document":
                        AnswerTheDocument(message, botClient, update);
                        break;
                    case "Video":
                        AnswerTheVideo(message, botClient);
                        break;
                    case "Photo":
                        AnswerThePhoto(message, botClient);
                        break;
                    case "Poll":
                        AnswerThePoll(message, botClient);
                        break;
                    default:
                        break;
                }
            }           
        }

        private static Task ErrorHabdler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        async private static void AnswerTheText(Message message, ITelegramBotClient botClient)
        {
            string answerMessage = string.Empty;
            string messageText= message.Text.ToLower();
            switch (messageText)
            {
                case string a when a.ToLower().Contains("прив"): 
                    answerMessage = "Приветсвую!";
                    SendSticker(message, botClient, 1);
                    break;
                case string a when a.ToLower().Contains("кто ты"):
                    answerMessage = "Я интеллектуальный саморазвивающийся бот-визитка одного талантливого разработчика)";
                    SendSticker(message, botClient, 0);
                    break;
                case "кто ты?":
                    answerMessage = "Я интеллектуальный саморазвивающийся бот-визитка одного талантливого разработчика)";
                    break;
                case "что ты умеешь?":
                    answerMessage = "Могу рассказать о себе, дать контакты";
                    break;
                case "дай-ка контакты для связи":
                    answerMessage = "Почта: Mr.Zakhar21@gmail.com";
                    break;
                case "как зовут создателя?":
                    answerMessage = "Я называю его мастером, вам лучше обращать к нему - Пирогов Захар";
                    break;
                case "хочу оставить отзыв!":
                    answerMessage = @"Вот форма для отбратной связи /|\";
                    ShowTheFeedbackForm(message, botClient);
                    break;
                default:
                    answerMessage = "Я пока еще не умею отвечать на такое( однако в будущем обязательно научусь!";
                    SendSticker(message, botClient, 2);
                    break;
            }
            await botClient.SendTextMessageAsync(message.Chat.Id, answerMessage, replyMarkup: GetButtons(message, botClient));
        }

        async private static void AnswerTheDocument(Message message, ITelegramBotClient botClient, Update update)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Прекрасно, сейчас посмотрим, что ты прислал=)");
            var field = update.Message.Document.FileId;
            var fileInfo = await botClient.GetFileAsync(field);
            var filePath = fileInfo.FilePath;
            string destinationFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\Telegram\{message.Document.FileName}";
            await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
            await botClient.DownloadFileAsync(
                filePath: filePath,
                destination: fileStream);
            fileStream.Close();
            return;           
        }

        async private static void AnswerTheVideo(Message message, ITelegramBotClient botClient)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "О, видео, сча глянем");
            return;
        }

        async private static void AnswerThePhoto(Message message, ITelegramBotClient botClient)
        {           
            await botClient.SendTextMessageAsync(message.Chat.Id, "Фото хорошее, но давай в формате документа=)");
            return;         
        }

        async private static void AnswerThePoll(Message message, ITelegramBotClient botClient)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Я робот, и не могу отвечать на голосование(");
            return;
        }

        private static IReplyMarkup GetButtons(Message message, ITelegramBotClient botClient)
        {
            return new ReplyKeyboardMarkup(new[] {
                new KeyboardButton[] {"Кто ты?", "Что ты умеешь?"},
                new KeyboardButton[] {"Дай-ка контакты для связи", "Как зовут создателя?"},
                new KeyboardButton[] { "Хочу оставить отзыв!" }
            })
            {
                ResizeKeyboard = true
            };
        }

        async private static void SendSticker(Message message, ITelegramBotClient botClient, int stickerIndex)
        {
            string[] stickerURLs = new string[] { 
            "https://tlgrm.eu/_/stickers/eb5/41e/eb541eba-3be4-3bea-bd7f-5e487503be39/96/11.webp",
            "https://tlgrm.eu/_/stickers/c2b/583/c2b583cc-71f2-3f42-935b-9a9c7ac16fc5/192/1.webp",
            "https://tlgrm.eu/_/stickers/c2b/583/c2b583cc-71f2-3f42-935b-9a9c7ac16fc5/192/2.webp"
            };
            var sticker = await botClient.SendStickerAsync(
                chatId: message.Chat.Id,
                sticker: $"{stickerURLs[stickerIndex]}");
        }       

        async private static void SaveTheFeedback(Message message, ITelegramBotClient botClient, Message pollMessage)
        {            
            Poll poll = await botClient.StopPollAsync(
                chatId: pollMessage.Chat.Id,
                messageId: pollMessage.MessageId);
        }

        async private static void ShowTheFeedbackForm(Message message, ITelegramBotClient botClient)
        {
            Message pollMessage = await botClient.SendPollAsync(
                chatId: message.Chat.Id,
                question: "Насколько вас устравивает функционал от о до 5?",
                options: new[]
                {
                    "5 - все прекрасно!",
                    "4 - хорошо!",
                    "3 - ну работает и ладно!",
                    "2 - хромает...",
                    "1 - очень плохо!",
                    "0 - это худшее, что я видел в своей жизни!"
                }
                );           
           // SaveTheFeedback();
        }
    }
}
