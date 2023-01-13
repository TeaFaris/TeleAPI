using System.Globalization;
using System.Numerics;
using TeleAPI.Bot.DataBase;
using TeleAPI.Bot.DataBase.Models;
using TeleAPI.Bot.Extentions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace TeleAPI.Bot.Extentions
{
    public static class TelegramBotExtentions
    {
        #region Receive
        public static async Task<Message> ReceiveMessageAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            await Task.CompletedTask;

            var SessionUser = User;
            if (SessionUser.IsGetMessageState)
                throw new Exception("Already receiving message!");

            SessionUser.IsGetMessageState = true;

            while (SessionUser.GetMessageStateMessage is null) ;

            Message Message = SessionUser.GetMessageStateMessage;
            SessionUser.GetMessageStateMessage = null;
            SessionUser.IsGetMessageState = false;

            return Message;
        }
        public static async Task<PhotoSize[]?> ReceivePhotoAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials => (await Bot.ReceiveMessageAsync(User)).Photo;
        public static async Task<Video?> ReceiveVideoAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials => (await Bot.ReceiveMessageAsync(User)).Video;
        public static async Task<Audio?> ReceiveAudioAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials => (await Bot.ReceiveMessageAsync(User)).Audio;
        public static async Task<Sticker?> ReceiveStickerAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials => (await Bot.ReceiveMessageAsync(User)).Sticker;
        public static async Task<Document?> ReceiveDocumentAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials => (await Bot.ReceiveMessageAsync(User)).Document;
        public static async Task<Animation?> ReceiveAnimationAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials => (await Bot.ReceiveMessageAsync(User)).Animation;
        public static async Task<Location?> ReceiveLocationAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials => (await Bot.ReceiveMessageAsync(User)).Location;
        public static async Task<string?> ReceiveStringAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            Message RecievedMessage = await Bot.ReceiveMessageAsync(User);
            return RecievedMessage.Text ?? RecievedMessage.Caption;
        }
        public static async Task<Num?> ReceiveNumberAsync<TDB, TCredits, Num>(this TelegramBot<TDB, TCredits> Bot, SessionUser User) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials where Num : INumber<Num>
        {
            if (Num.TryParse(await Bot.ReceiveStringAsync(User), CultureInfo.InvariantCulture.NumberFormat, out Num Number))
                return Number;
            return default;
        }
        #endregion

        #region Send
        public static async Task<Message> SendMessageAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User, string Text, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? Entities = default, bool? DisableWebPagePreview = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            Message? Msg = null;
            long ChatId = User.UserID;
            SessionUser SessionUser = User;
            if (Bot.EditMessagesMode)
            {
                if (SessionUser.LastMessage is not null)
                {
                    await Bot.Api.DeleteMessageAsync(ChatId, SessionUser.LastMessage.MessageId, CancellationToken);
                }

                switch (SessionUser.LastMessageFromBot?.Type)
                {
                    case MessageType.Text:
                        {
                            try
                            {
                                Msg = await Bot.Api.EditMessageTextAsync(
                                chatId: ChatId,
                                messageId: SessionUser.LastMessageFromBot.MessageId,
                                text: Text,
                                parseMode: ParseMode,
                                entities: Entities,
                                disableWebPagePreview: DisableWebPagePreview,
                                replyMarkup: (InlineKeyboardMarkup?)ReplyMarkup,
                                cancellationToken: CancellationToken);
                            }
                            catch { }
                            break;
                        }
                    default:
                        {
                            Msg = await Bot.SendMessageAsync(ChatId, Text, ParseMode, Entities, DisableWebPagePreview, true, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
                            if (SessionUser.LastMessageFromBot is not null)
                                await Bot.Api.DeleteMessageAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, CancellationToken);
                            break;
                        }
                }
            }
            else
            {
                Msg = await Bot.SendMessageAsync(ChatId, Text, ParseMode, Entities, DisableWebPagePreview, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
            }

            if (Msg is not null)
            {
                SessionUser.LastMessageFromBot = Msg;
            }
            return SessionUser.LastMessageFromBot!;
        }
        public static async Task<Message> SendMessageAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, ChatId ChatID, string Text, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? Entities = default, bool? DisableWebPagePreview = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            return await Bot.Api.SendTextMessageAsync(ChatID, Text, ParseMode, Entities, DisableWebPagePreview, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
        }
        public static async Task<Message> SendPhotoAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User, InputOnlineFile Media, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            Message Msg;
            long ChatId = User.UserID;
            SessionUser SessionUser = User;
            if (Bot.EditMessagesMode)
            {
                if (SessionUser.LastMessage is not null)
                    await Bot.Api.DeleteMessageAsync(ChatId, SessionUser.LastMessage.MessageId, CancellationToken);
                switch (SessionUser.LastMessageFromBot?.Type)
                {
                    case MessageType.Photo:
                    case MessageType.Video:
                    case MessageType.Document:
                    case MessageType.Voice:
                    case MessageType.Audio:
                        {
                            Msg = await Bot.Api.EditMessageMediaAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, new InputMediaPhoto(Media.Content is null ? new InputMedia(Media.Url) : new InputMedia(Media.Content, "photo")), (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            if (Caption is not null)
                                await Bot.Api.EditMessageCaptionAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, Caption, ParseMode, CaptionEntities, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            break;
                        }
                    default:
                        {
                            Msg = await Bot.SendPhotoAsync(ChatId, Media, Caption, ParseMode, CaptionEntities, true, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
                            if (SessionUser.LastMessageFromBot is not null)
                                await Bot.Api.DeleteMessageAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, CancellationToken);
                            break;
                        }
                }
            }
            else
            {
                Msg = await Bot.SendPhotoAsync(ChatId, Media, Caption, ParseMode, CaptionEntities, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
            }

            SessionUser.LastMessageFromBot = Msg;
            return Msg;
        }
        public static async Task<Message> SendPhotoAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, ChatId ChatID, InputOnlineFile Media, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            return await Bot.Api.SendPhotoAsync(ChatID, Media, Caption, ParseMode, CaptionEntities, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
        }
        public static async Task<Message> SendVideoAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User, InputOnlineFile Video, int? Duration = default, int? Width = default, int? Height = default, InputMedia? Thumb = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? SupportsStreaming = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            Message Msg;
            long ChatId = User.UserID;
            SessionUser SessionUser = User;
            if (Bot.EditMessagesMode)
            {
                if (SessionUser.LastMessage is not null)
                    await Bot.Api.DeleteMessageAsync(ChatId, SessionUser.LastMessage.MessageId, CancellationToken);
                switch (SessionUser.LastMessageFromBot?.Type)
                {
                    case MessageType.Photo:
                    case MessageType.Video:
                    case MessageType.Document:
                    case MessageType.Voice:
                    case MessageType.Audio:
                        {
                            Msg = await Bot.Api.EditMessageMediaAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, new InputMediaVideo(Video.Content is null ? new InputMedia(Video.Url!) : new InputMedia(Video.Content, "video")), (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            if (Caption is not null)
                                await Bot.Api.EditMessageCaptionAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, Caption, ParseMode, CaptionEntities, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            break;
                        }
                    default:
                        {
                            Msg = await Bot.SendVideoAsync(ChatId, Video, Duration, Width, Height, Thumb, Caption, ParseMode, CaptionEntities, SupportsStreaming, true, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
                            if (SessionUser.LastMessageFromBot is not null)
                                await Bot.Api.DeleteMessageAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, CancellationToken);
                            break;
                        }
                }
            }
            else
            {
                Msg = await Bot.SendVideoAsync(ChatId, Video, Duration, Width, Height, Thumb, Caption, ParseMode, CaptionEntities, SupportsStreaming, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
            }

            SessionUser.LastMessageFromBot = Msg;
            return Msg;
        }
        public static async Task<Message> SendVideoAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, ChatId ChatID, InputOnlineFile Video, int? Duration = default, int? Width = default, int? Height = default, InputMedia? Thumb = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? SupportsStreaming = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            return await Bot.Api.SendVideoAsync(ChatID, Video, Duration, Width, Height, Thumb, Caption, ParseMode, CaptionEntities, SupportsStreaming, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
        }
        public static async Task<Message> SendAnimationAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User, InputOnlineFile Animation, int? Duration = default, int? Width = default, int? Height = default, InputMedia? Thumb = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            Message Msg;
            long ChatId = User.UserID;
            SessionUser SessionUser = User;
            if (Bot.EditMessagesMode)
            {
                if (SessionUser.LastMessage is not null)
                    await Bot.Api.DeleteMessageAsync(ChatId, SessionUser.LastMessage.MessageId, CancellationToken);
                switch (SessionUser.LastMessageFromBot?.Type)
                {
                    case MessageType.Photo:
                    case MessageType.Video:
                    case MessageType.Document:
                    case MessageType.Voice:
                    case MessageType.Audio:
                        {
                            Msg = await Bot.Api.EditMessageMediaAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, new InputMediaVideo(Animation.Content is null ? new InputMedia(Animation.Url) : new InputMedia(Animation.Content, "animation")), (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            if (Caption is not null)
                                await Bot.Api.EditMessageCaptionAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, Caption, ParseMode, CaptionEntities, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            break;
                        }
                    default:
                        {
                            Msg = await Bot.SendAnimationAsync(ChatId, Animation, Duration, Width, Height, Thumb, Caption, ParseMode, CaptionEntities, true, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
                            if (SessionUser.LastMessageFromBot is not null)
                                await Bot.Api.DeleteMessageAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, CancellationToken);
                            break;
                        }
                }
            }
            else
            {
                Msg = await Bot.SendAnimationAsync(ChatId, Animation, Duration, Width, Height, Thumb, Caption, ParseMode, CaptionEntities, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
            }

            SessionUser.LastMessageFromBot = Msg;
            return Msg;
        }
        public static async Task<Message> SendAnimationAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, ChatId ChatID, InputOnlineFile Animation, int? Duration = default, int? Width = default, int? Height = default, InputMedia? Thumb = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            return await Bot.Api.SendAnimationAsync(ChatID, Animation, Duration, Width, Height, Thumb, Caption, ParseMode, CaptionEntities, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
        }
        public static async Task SendChatActionAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User, ChatAction ChatAction, CancellationToken CancellationToken = default) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials => await Bot.Api.SendChatActionAsync(User.UserID, ChatAction, CancellationToken);
        public static async Task<Message> SendDocumentAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User, InputOnlineFile Document, InputMedia? Thumb = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? DisableContentTypeDetection = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            Message Msg;
            long ChatId = User.UserID;
            SessionUser SessionUser = User;
            if (Bot.EditMessagesMode)
            {
                if (SessionUser.LastMessage is not null)
                    await Bot.Api.DeleteMessageAsync(ChatId, SessionUser.LastMessage.MessageId, CancellationToken);
                switch (SessionUser.LastMessageFromBot?.Type)
                {
                    case MessageType.Photo:
                    case MessageType.Video:
                    case MessageType.Document:
                    case MessageType.Voice:
                    case MessageType.Audio:
                        {
                            Msg = await Bot.Api.EditMessageMediaAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, new InputMediaVideo(Document.Content is null ? new InputMedia(Document.Url) : new InputMedia(Document.Content, Document.FileName ?? "document")), (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            if (Caption is not null)
                                await Bot.Api.EditMessageCaptionAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, Caption, ParseMode, CaptionEntities, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            break;
                        }
                    default:
                        {
                            Msg = await Bot.SendDocumentAsync(ChatId, Document, Thumb, Caption, ParseMode, CaptionEntities, DisableContentTypeDetection, true, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
                            if (SessionUser.LastMessageFromBot is not null)
                                await Bot.Api.DeleteMessageAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, CancellationToken);
                            break;
                        }
                }
            }
            else
            {
                Msg = await Bot.SendDocumentAsync(ChatId, Document, Thumb, Caption, ParseMode, CaptionEntities, DisableContentTypeDetection, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
            }

            SessionUser.LastMessageFromBot = Msg;
            return Msg;
        }
        public static async Task<Message> SendDocumentAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, ChatId ChatID, InputOnlineFile Document, InputMedia? Thumb = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? DisableContentTypeDetection = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            return await Bot.Api.SendDocumentAsync(ChatID, Document, Thumb, Caption, ParseMode, CaptionEntities, DisableContentTypeDetection, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
        }
        #endregion
    }
}
