using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Playgrounds;

public sealed class TelegramPlayground
{
    public static async Task Run()
    {
        var accessToken = "6088434129:AAGakHx5A6G7PqEOgQFbCdGfdJkrbh_Rhf4";
        var client = new TelegramBotClient(accessToken);
        var me = await client.GetMeAsync();
        Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

        var cts = new CancellationTokenSource();
        client.StartReceiving(
            updateHandler: UpdateHandler!,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            },
            cts.Token);

        Console.ReadLine();
        await cts.CancelAsync();
    }

    private static async Task UpdateHandler(
        ITelegramBotClient client,
        Update update,
        CancellationToken cancellationToken)
    {
        // Only process Message updates: https://core.telegram.org/bots/api#message
        if ( update.Message is not { } message )
            return;
        // Only process text messages
        if ( message.Text is not { } messageText )
            return;

        var chatId = message.Chat.Id;

        Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

        // Echo received message text
        var sentMessage = await client.SendTextMessageAsync(
            chatId: chatId,
            text: "You said:\n" + messageText,
            cancellationToken: cancellationToken);
    }

    private static Task HandlePollingErrorAsync(ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken)
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
}