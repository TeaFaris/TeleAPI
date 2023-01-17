using System.Globalization;
using System.Numerics;
using TeleAPI.Bot.DataBase;
using TeleAPI.Bot.DataBase.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TeleAPI.Bot.Extentions
{
    public static class TelegramBotExtentions
    {
        #region Receive
        public static async Task<Message> ReceiveMessageAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User)
            where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
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
        public static async Task<PhotoSize[]?> ReceivePhotoAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User) where TDB : TelegramDBContext, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials => (await Bot.ReceiveMessageAsync(User)).Photo;
        public static async Task<Video?> ReceiveVideoAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User) where TDB : TelegramDBContext, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials => (await Bot.ReceiveMessageAsync(User)).Video;
        public static async Task<Audio?> ReceiveAudioAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User) where TDB : TelegramDBContext, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials => (await Bot.ReceiveMessageAsync(User)).Audio;
        public static async Task<Sticker?> ReceiveStickerAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User) where TDB : TelegramDBContext, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials => (await Bot.ReceiveMessageAsync(User)).Sticker;
        public static async Task<Document?> ReceiveDocumentAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User) where TDB : TelegramDBContext, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials => (await Bot.ReceiveMessageAsync(User)).Document;
        public static async Task<Animation?> ReceiveAnimationAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User) where TDB : TelegramDBContext, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials => (await Bot.ReceiveMessageAsync(User)).Animation;
        public static async Task<Location?> ReceiveLocationAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User) where TDB : TelegramDBContext, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials => (await Bot.ReceiveMessageAsync(User)).Location;
        public static async Task<string?> ReceiveStringAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User) where TDB : TelegramDBContext, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
        {
            Message RecievedMessage = await Bot.ReceiveMessageAsync(User);
            return RecievedMessage.Text ?? RecievedMessage.Caption;
        }
        public static async Task<Num?> ReceiveNumberAsync<TDB, TCredits, Num>(this TelegramBot<TDB, TCredits> Bot, SessionUser User) where TDB : TelegramDBContext, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials where Num : INumber<Num>
        {
            if (Num.TryParse(await Bot.ReceiveStringAsync(User), CultureInfo.InvariantCulture.NumberFormat, out Num? Number))
                return Number;
            return default;
        }
        public static async Task<CallbackQuery> ReciveCallbackAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User)
            where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            await Task.CompletedTask;

            var SessionUser = User;
            if (SessionUser.IsGetCallbackDataState)
                throw new Exception("Already receiving callback query!");

            SessionUser.IsGetCallbackDataState = true;

            while (SessionUser.GetCallbackStateQuery is null) ;

            CallbackQuery CallbackQuery = SessionUser.GetCallbackStateQuery;
            SessionUser.GetCallbackStateQuery = null;
            SessionUser.IsGetCallbackDataState = false;

            return CallbackQuery;
        }
        #endregion

        #region Send
        public static async Task<Message> SendMessageAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User, string Text, int? MessageThreadID = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? Entities = default, bool? DisableWebPagePreview = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
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
                            Msg = await Bot.SendMessageAsync(ChatId, Text, MessageThreadID, ParseMode, Entities, DisableWebPagePreview, true, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
                            if (SessionUser.LastMessageFromBot is not null)
                                await Bot.Api.DeleteMessageAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, CancellationToken);
                            break;
                        }
                }
            }
            else
            {
                Msg = await Bot.SendMessageAsync(ChatId, Text, MessageThreadID, ParseMode, Entities, DisableWebPagePreview, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
            }

            if (Msg is not null)
            {
                SessionUser.LastMessageFromBot = Msg;
            }
            return SessionUser.LastMessageFromBot!;
        }
        public static async Task<Message> SendMessageAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, ChatId ChatID, string Text, int? MessageThreadID = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? Entities = default, bool? DisableWebPagePreview = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            return await Bot.Api.SendTextMessageAsync(ChatID, Text, MessageThreadID, ParseMode, Entities, DisableWebPagePreview, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
        }
        public static async Task<Message> SendPhotoAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User, InputMediaPhoto Media, int? MessageThreadID = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? HasSpoiler = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
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
                            Msg = await Bot.Api.EditMessageMediaAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, Media, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            if (Caption is not null)
                                await Bot.Api.EditMessageCaptionAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, Caption, ParseMode, CaptionEntities, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            break;
                        }
                    default:
                        {
                            Msg = await Bot.SendPhotoAsync(ChatId, Media, MessageThreadID, Caption, ParseMode, CaptionEntities, HasSpoiler, true, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
                            if (SessionUser.LastMessageFromBot is not null)
                                await Bot.Api.DeleteMessageAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, CancellationToken);
                            break;
                        }
                }
            }
            else
            {
                Msg = await Bot.SendPhotoAsync(ChatId, Media, MessageThreadID, Caption, ParseMode, CaptionEntities, HasSpoiler, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
            }

            SessionUser.LastMessageFromBot = Msg;
            return Msg;
        }
        public static async Task<Message> SendPhotoAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, ChatId ChatID, InputMediaPhoto Media, int? MessageThreadID = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? HasSpoiler = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            return await Bot.Api.SendPhotoAsync(ChatID, (IInputFile)Media, MessageThreadID, Caption, ParseMode, CaptionEntities, HasSpoiler, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
        }
        public static async Task<Message> SendVideoAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User, InputMediaVideo Video, int? MessageThreadID = default, int? Duration = default, int? Width = default, int? Height = default, IInputFile? Thumb = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? HasSpoiler = default, bool? SupportsStreaming = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
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
                            Msg = await Bot.Api.EditMessageMediaAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, Video, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            if (Caption is not null)
                                await Bot.Api.EditMessageCaptionAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, Caption, ParseMode, CaptionEntities, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            break;
                        }
                    default:
                        {
                            Msg = await Bot.SendVideoAsync(ChatId, Video, MessageThreadID, Duration, Width, Height, Thumb, Caption, ParseMode, CaptionEntities, SupportsStreaming, HasSpoiler, true, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
                            if (SessionUser.LastMessageFromBot is not null)
                                await Bot.Api.DeleteMessageAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, CancellationToken);
                            break;
                        }
                }
            }
            else
            {
                Msg = await Bot.SendVideoAsync(ChatId, Video, MessageThreadID, Duration, Width, Height, Thumb, Caption, ParseMode, CaptionEntities, SupportsStreaming, HasSpoiler, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
            }

            SessionUser.LastMessageFromBot = Msg;
            return Msg;
        }
        public static async Task<Message> SendVideoAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, ChatId ChatID, InputMediaVideo Video, int? MessageThreadID = default, int? Duration = default, int? Width = default, int? Height = default, IInputFile? Thumb = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? SupportsStreaming = default, bool? HasSpoiler = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            return await Bot.Api.SendVideoAsync(ChatID, (IInputFile)Video, MessageThreadID, Duration, Width, Height, Thumb, Caption, ParseMode, CaptionEntities, HasSpoiler, SupportsStreaming, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
        }
        public static async Task<Message> SendAnimationAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User, InputMediaAnimation Animation, int? MessageThreadID = default, int? Duration = default, int? Width = default, int? Height = default, IInputFile? Thumb = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? HasSpoiler = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
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
                            Msg = await Bot.Api.EditMessageMediaAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, Animation, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            if (Caption is not null)
                                await Bot.Api.EditMessageCaptionAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, Caption, ParseMode, CaptionEntities, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            break;
                        }
                    default:
                        {
                            Msg = await Bot.SendAnimationAsync(ChatId, Animation, MessageThreadID, Duration, Width, Height, Thumb, Caption, ParseMode, CaptionEntities, HasSpoiler, true, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
                            if (SessionUser.LastMessageFromBot is not null)
                                await Bot.Api.DeleteMessageAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, CancellationToken);
                            break;
                        }
                }
            }
            else
            {
                Msg = await Bot.SendAnimationAsync(ChatId, Animation, MessageThreadID, Duration, Width, Height, Thumb, Caption, ParseMode, CaptionEntities, HasSpoiler, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
            }

            SessionUser.LastMessageFromBot = Msg;
            return Msg;
        }
        public static async Task<Message> SendAnimationAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, ChatId ChatID, InputMediaAnimation Animation, int? MessageThreadID = default, int? Duration = default, int? Width = default, int? Height = default, IInputFile? Thumb = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? HasSpoiler = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default) where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            return await Bot.Api.SendAnimationAsync(ChatID, (IInputFile)Animation, MessageThreadID, Duration, Width, Height, Thumb, Caption, ParseMode, CaptionEntities, HasSpoiler, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
        }
        public static async Task SendChatActionAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User, ChatAction ChatAction, int? MessageThreadID = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials => await Bot.Api.SendChatActionAsync(User.UserID, ChatAction, MessageThreadID, CancellationToken);
        public static async Task SendChatActionAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, ChatId ChatID, ChatAction ChatAction, int? MessageThreadID = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials => await Bot.Api.SendChatActionAsync(ChatID, ChatAction, MessageThreadID, CancellationToken);
        public static async Task<Message> SendDocumentAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User, InputMediaDocument Document, int? MessageThreadID = default, IInputFile? Thumb = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? DisableContentTypeDetection = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
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
                            Msg = await Bot.Api.EditMessageMediaAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, Document, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            if (Caption is not null)
                                await Bot.Api.EditMessageCaptionAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, Caption, ParseMode, CaptionEntities, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            break;
                        }
                    default:
                        {
                            Msg = await Bot.SendDocumentAsync(ChatId, Document, MessageThreadID, Thumb, Caption, ParseMode, CaptionEntities, DisableContentTypeDetection, true, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
                            if (SessionUser.LastMessageFromBot is not null)
                                await Bot.Api.DeleteMessageAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, CancellationToken);
                            break;
                        }
                }
            }
            else
            {
                Msg = await Bot.SendDocumentAsync(ChatId, Document, MessageThreadID, Thumb, Caption, ParseMode, CaptionEntities, DisableContentTypeDetection, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
            }

            SessionUser.LastMessageFromBot = Msg;
            return Msg;
        }
        public static async Task<Message> SendDocumentAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, ChatId ChatID, InputMediaDocument Document, int? MessageThreadID = default, IInputFile? Thumb = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? DisableContentTypeDetection = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            return await Bot.Api.SendDocumentAsync(ChatID, (IInputFile)Document, MessageThreadID, Thumb, Caption, ParseMode, CaptionEntities, DisableContentTypeDetection, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
        }
        public static async Task<Message[]> SendMediaGroupAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, ChatId ChatID, IEnumerable<IAlbumInputMedia> MediaAlbum, int? MessageThreadID = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            return await Bot.Api.SendMediaGroupAsync(ChatID, MediaAlbum, MessageThreadID, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, CancellationToken);
        }
        public static async Task<Message[]> SendMediaGroupAsync<TDB, TCredits>(this TelegramBot<TDB, TCredits> Bot, SessionUser User, IEnumerable<IAlbumInputMedia> MediaAlbum, int? MessageThreadID = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext, IDBContext<TCredits> where TCredits : struct, IDBCredentials
        {
            Message[] Msg;
            long ChatId = User.UserID;
            SessionUser SessionUser = User;
            if (Bot.EditMessagesMode)
            {
                if (SessionUser.LastMessage is not null)
                    await Bot.Api.DeleteMessageAsync(ChatId, SessionUser.LastMessage.MessageId, CancellationToken);
                Msg = await Bot.SendMediaGroupAsync(ChatId, MediaAlbum, MessageThreadID, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, CancellationToken);
                if (SessionUser.LastMessageFromBot is not null)
                    await Bot.Api.DeleteMessageAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, CancellationToken);
            }
            else
            {
                Msg = await Bot.SendMediaGroupAsync(ChatId, MediaAlbum, MessageThreadID, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, CancellationToken);
            }

            SessionUser.LastMessageFromBot = Msg.Last();
            return Msg;
        }
        #endregion
    }
}
