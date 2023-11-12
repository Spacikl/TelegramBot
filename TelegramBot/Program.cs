using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

var forwardedMessage = 508;
var text1 = "\ud83d\udcceСтруктура дневника:\n\nВедение дневника будет подразделено на \ud83d\udcab периода: 2 Первые три дня записи будут о твоей привычной рутине (как обычно происходят отход ко сну и пробуждение).\n\n\ud83e\udd0dДалее мы расскажем тебе об одном из методов борьбы с существующей проблемой, практиковать который необходимо будет следующие четыре дня.\n\nНа этом инструктаж закончен\ud83d\ude0a\nОчень рады что ты с нами!\nУвидимся завтра.";
var text = "Привет! С завтрашнего дня начинается наше путешествие по просторам твоих снов \ud83c\udf15\n\nДля начала введем тебя в курс дела: с твоей помощью мы собираемся восстановить кусочки паззлов: как рутина, образ жизни и иные факторы влияют на нарушения сна. Что от тебя требуется? В свободной, наиболее комфортной для тебя форме поведать нам о своем опыте. \n\nКаждое утро (вс 12.11 – сб 18.11) ты будешь получать порцию открытых и закрытых вопросов, ответ на которые может быть в любом удобном виде:\n\ud83e\udd0d текст\n\ud83e\udd0d тг-кружочек\n\ud83e\udd0d видео\n\ud83e\udd0d гс\n\nВопросы не будут закреплены за каким-то определенным временем, и ты можешь спокойно отвечать на них после пробуждения.";
var uxChat = -4074534292;
var path = Path.Combine
    ("/home/spacikl/Desktop", "ChatsId.txt");

var botClient = new TelegramBotClient("6597955397:AAGbFFuEYrrtidpfSSLSIK2d2wMHixAiiO0");

using CancellationTokenSource cts = new ();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new ()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

Console.ReadLine();
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient,
    Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
        return;
    
    var chatId = message.Chat.Id;

    if (message.Text == "/start")
    {
        var uniqueChatId = await File.ReadAllLinesAsync(path);
        if (!uniqueChatId.Contains(message.Chat.Id.ToString()))
            await File.AppendAllTextAsync(path,message.Chat.Id + "\n"); 
        await SendMessagesToRespondents(forwardedMessage);
        
        return;
    }
    
    //forward message to uxChat
    if (message.Chat.Id != uxChat)
    {
        await botClient.ForwardMessageAsync(uxChat, chatId, message.MessageId);
        await botClient.SendTextMessageAsync(chatId, "Спасибо большое! ❤️");
        return;
    }
    //forward message
    if (message.Text == "/sendmessage" && message.Chat.Id == uxChat)
    {
        await SendMessagesToRespondents(message.MessageId - 1);
        return;
    }
}

async Task SendMessagesToRespondents(int messageType)
{
    var allChatId = await File.ReadAllLinesAsync(path);
        
    foreach (var id in allChatId)
        if (id != uxChat.ToString())
            await botClient.ForwardMessageAsync(id, uxChat, messageType);
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient,
    Exception exception, CancellationToken cancellationToken)
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