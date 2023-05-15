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
        public static async Task<Message> ReceiveMessageAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, SessionUser User)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new()
        {
            await Task.CompletedTask;

            var SessionUser = User;
            if (SessionUser.IsGetMessageState || SessionUser.IsGetCallbackDataState)
                throw new Exception("Already receiving message!");

            SessionUser.IsGetMessageState = true;

            while (SessionUser.GetMessageStateMessage is null)
				await Task.Delay(25);

			Message Message = SessionUser.GetMessageStateMessage;
            SessionUser.GetMessageStateMessage = null;
            SessionUser.IsGetMessageState = false;

            return Message;
        }
        public static async Task<PhotoSize[]?> ReceivePhotoAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, SessionUser User)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new() => (await Bot.ReceiveMessageAsync(User)).Photo;
        public static async Task<Video?> ReceiveVideoAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, SessionUser User)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new() => (await Bot.ReceiveMessageAsync(User)).Video;
        public static async Task<Audio?> ReceiveAudioAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, SessionUser User)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new() => (await Bot.ReceiveMessageAsync(User)).Audio;
        public static async Task<Sticker?> ReceiveStickerAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, SessionUser User)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new() => (await Bot.ReceiveMessageAsync(User)).Sticker;
        public static async Task<Document?> ReceiveDocumentAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, SessionUser User)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new() => (await Bot.ReceiveMessageAsync(User)).Document;
        public static async Task<Animation?> ReceiveAnimationAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, SessionUser User)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new() => (await Bot.ReceiveMessageAsync(User)).Animation;
        public static async Task<Location?> ReceiveLocationAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, SessionUser User)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new() => (await Bot.ReceiveMessageAsync(User)).Location;
        public static async Task<string?> ReceiveStringAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, SessionUser User)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new()
        {
            Message RecievedMessage = await Bot.ReceiveMessageAsync(User);
            return RecievedMessage.Text ?? RecievedMessage.Caption;
        }
        public static async Task<Num?> ReceiveNumberAsync<Num, TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, SessionUser User)
            where Num : struct, INumber<Num>
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new()
        {
            if (Num.TryParse(await Bot.ReceiveStringAsync(User), CultureInfo.InvariantCulture.NumberFormat, out Num Number))
                return Number;
            return null;
        }
        /// <summary>
        /// Waits for user to input CallbackData. Await it or otherwise it will not work.
        /// </summary>
        /// <typeparam name="TDB"></typeparam>
        /// <typeparam name="TCredits"></typeparam>
        /// <param name="Bot">Bot</param>
        /// <param name="User">User</param>
        /// <returns>CallbackQuery from user, if already waiting for callback returns null.</returns>
        public static async Task<CallbackQuery?> ReceiveCallbackAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, SessionUser User)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new()
        {
            await Task.CompletedTask;

            if (User.IsGetMessageState || User.IsGetCallbackDataState)
				throw new Exception("Already receiving message!");

			var SessionUser = User;

            SessionUser.IsGetCallbackDataState = true;

            while (SessionUser.GetCallbackStateQuery is null)
                await Task.Delay(25);

            CallbackQuery CallbackQuery = SessionUser.GetCallbackStateQuery;
            SessionUser.GetCallbackStateQuery = null;
            SessionUser.IsGetCallbackDataState = false;

            return CallbackQuery;
        }
        #endregion

        #region Send
        public static async Task<Message> SendMessageAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, SessionUser User, string Text, int? MessageThreadID = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? Entities = default, bool? DisableWebPagePreview = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new()
        {
            Message? Msg = null;
            long ChatId = User.UserID;
            SessionUser SessionUser = User;
            if (Bot.EditMessagesMode)
            {
                if (SessionUser.LastMessage is not null)
                {
                    try
                    {
                        await Bot.API.DeleteMessageAsync(ChatId, SessionUser.LastMessage.MessageId, CancellationToken);
                    }
                    catch { }
                }

                switch (SessionUser.LastMessageFromBot?.Type)
                {
                    case MessageType.Text:
                        {
                            try
                            {
                                Msg = await Bot.API.EditMessageTextAsync(
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
                                await Bot.API.DeleteMessageAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, CancellationToken);
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
        public static async Task<Message> SendMessageAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, ChatId ChatID, string Text, int? MessageThreadID = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? Entities = default, bool? DisableWebPagePreview = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new()
        {
            return await Bot.API.SendTextMessageAsync(ChatID, Text, MessageThreadID, ParseMode, Entities, DisableWebPagePreview, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
        }
        public static async Task<Message> SendPhotoAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, SessionUser User, IInputFile Media, int? MessageThreadID = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? HasSpoiler = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new()
        {
            Message Msg;
            long ChatId = User.UserID;
            SessionUser SessionUser = User;
            if (Bot.EditMessagesMode)
            {
                if (SessionUser.LastMessage is not null)
                    await Bot.API.DeleteMessageAsync(ChatId, SessionUser.LastMessage.MessageId, CancellationToken);
                switch (SessionUser.LastMessageFromBot?.Type)
                {
                    case MessageType.Photo:
                    case MessageType.Video:
                    case MessageType.Document:
                    case MessageType.Voice:
                    case MessageType.Audio:
                        {
                            Msg = await Bot.API.EditMessageMediaAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, (InputMedia)Media, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            if (Caption is not null)
                                await Bot.API.EditMessageCaptionAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, Caption, ParseMode, CaptionEntities, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            break;
                        }
                    default:
                        {
                            Msg = await Bot.SendPhotoAsync(ChatId, Media, MessageThreadID, Caption, ParseMode, CaptionEntities, HasSpoiler, true, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
                            if (SessionUser.LastMessageFromBot is not null)
                                await Bot.API.DeleteMessageAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, CancellationToken);
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
        public static async Task<Message> SendPhotoAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, ChatId ChatID, IInputFile Media, int? MessageThreadID = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? HasSpoiler = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new()
        {
            return await Bot.API.SendPhotoAsync(ChatID, Media, MessageThreadID, Caption, ParseMode, CaptionEntities, HasSpoiler, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
        }
        public static async Task<Message> SendVideoAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, SessionUser User, IInputFile Video, int? MessageThreadID = default, int? Duration = default, int? Width = default, int? Height = default, IInputFile? Thumb = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? HasSpoiler = default, bool? SupportsStreaming = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new()
        {
            Message Msg;
            long ChatId = User.UserID;
            SessionUser SessionUser = User;
            if (Bot.EditMessagesMode)
            {
                if (SessionUser.LastMessage is not null)
                    await Bot.API.DeleteMessageAsync(ChatId, SessionUser.LastMessage.MessageId, CancellationToken);
                switch (SessionUser.LastMessageFromBot?.Type)
                {
                    case MessageType.Photo:
                    case MessageType.Video:
                    case MessageType.Document:
                    case MessageType.Voice:
                    case MessageType.Audio:
                        {
                            Msg = await Bot.API.EditMessageMediaAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, (InputMedia)Video, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            if (Caption is not null)
                                await Bot.API.EditMessageCaptionAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, Caption, ParseMode, CaptionEntities, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            break;
                        }
                    default:
                        {
                            Msg = await Bot.SendVideoAsync(ChatId, Video, MessageThreadID, Duration, Width, Height, Thumb, Caption, ParseMode, CaptionEntities, SupportsStreaming, HasSpoiler, true, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
                            if (SessionUser.LastMessageFromBot is not null)
                                await Bot.API.DeleteMessageAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, CancellationToken);
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
        public static async Task<Message> SendVideoAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, ChatId ChatID, IInputFile Video, int? MessageThreadID = default, int? Duration = default, int? Width = default, int? Height = default, IInputFile? Thumb = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? SupportsStreaming = default, bool? HasSpoiler = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new()
        {
            return await Bot.API.SendVideoAsync(ChatID, Video, MessageThreadID, Duration, Width, Height, Thumb, Caption, ParseMode, CaptionEntities, HasSpoiler, SupportsStreaming, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
        }
        public static async Task<Message> SendAnimationAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, SessionUser User, IInputFile Animation, int? MessageThreadID = default, int? Duration = default, int? Width = default, int? Height = default, IInputFile? Thumb = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? HasSpoiler = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new()
        {
            Message Msg;
            long ChatId = User.UserID;
            SessionUser SessionUser = User;
            if (Bot.EditMessagesMode)
            {
                if (SessionUser.LastMessage is not null)
                    await Bot.API.DeleteMessageAsync(ChatId, SessionUser.LastMessage.MessageId, CancellationToken);
                switch (SessionUser.LastMessageFromBot?.Type)
                {
                    case MessageType.Photo:
                    case MessageType.Video:
                    case MessageType.Document:
                    case MessageType.Voice:
                    case MessageType.Audio:
                        {
                            Msg = await Bot.API.EditMessageMediaAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, (InputMedia)Animation, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            if (Caption is not null)
                                await Bot.API.EditMessageCaptionAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, Caption, ParseMode, CaptionEntities, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            break;
                        }
                    default:
                        {
                            Msg = await Bot.SendAnimationAsync(ChatId, Animation, MessageThreadID, Duration, Width, Height, Thumb, Caption, ParseMode, CaptionEntities, HasSpoiler, true, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
                            if (SessionUser.LastMessageFromBot is not null)
                                await Bot.API.DeleteMessageAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, CancellationToken);
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
        public static async Task<Message> SendAnimationAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, ChatId ChatID, IInputFile Animation, int? MessageThreadID = default, int? Duration = default, int? Width = default, int? Height = default, IInputFile? Thumb = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? HasSpoiler = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new()
        {
            return await Bot.API.SendAnimationAsync(ChatID, Animation, MessageThreadID, Duration, Width, Height, Thumb, Caption, ParseMode, CaptionEntities, HasSpoiler, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
        }
        public static async Task SendChatActionAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, SessionUser User, ChatAction ChatAction, int? MessageThreadID = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new() => await Bot.API.SendChatActionAsync(User.UserID, ChatAction, MessageThreadID, CancellationToken);
        public static async Task SendChatActionAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, ChatId ChatID, ChatAction ChatAction, int? MessageThreadID = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new() => await Bot.API.SendChatActionAsync(ChatID, ChatAction, MessageThreadID, CancellationToken);
        public static async Task<Message> SendDocumentAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, SessionUser User, IInputFile Document, int? MessageThreadID = default, IInputFile? Thumb = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? DisableContentTypeDetection = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new()
        {
            Message Msg;
            long ChatId = User.UserID;
            SessionUser SessionUser = User;
            if (Bot.EditMessagesMode)
            {
                if (SessionUser.LastMessage is not null)
                    await Bot.API.DeleteMessageAsync(ChatId, SessionUser.LastMessage.MessageId, CancellationToken);
                switch (SessionUser.LastMessageFromBot?.Type)
                {
                    case MessageType.Photo:
                    case MessageType.Video:
                    case MessageType.Document:
                    case MessageType.Voice:
                    case MessageType.Audio:
                        {
                            Msg = await Bot.API.EditMessageMediaAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, (InputMedia)Document, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            if (Caption is not null)
                                await Bot.API.EditMessageCaptionAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, Caption, ParseMode, CaptionEntities, (InlineKeyboardMarkup?)ReplyMarkup, CancellationToken);
                            break;
                        }
                    default:
                        {
                            Msg = await Bot.SendDocumentAsync(ChatId, Document, MessageThreadID, Thumb, Caption, ParseMode, CaptionEntities, DisableContentTypeDetection, true, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
                            if (SessionUser.LastMessageFromBot is not null)
                                await Bot.API.DeleteMessageAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, CancellationToken);
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
        public static async Task<Message> SendDocumentAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, ChatId ChatID, IInputFile Document, int? MessageThreadID = default, IInputFile? Thumb = default, string? Caption = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? CaptionEntities = default, bool? DisableContentTypeDetection = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new()
        {
            return await Bot.API.SendDocumentAsync(ChatID, Document, MessageThreadID, Thumb, Caption, ParseMode, CaptionEntities, DisableContentTypeDetection, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, ReplyMarkup, CancellationToken);
        }
        public static async Task<Message[]> SendMediaGroupAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, ChatId ChatID, IEnumerable<IAlbumInputMedia> MediaAlbum, int? MessageThreadID = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new()
        {
            return await Bot.API.SendMediaGroupAsync(ChatID, MediaAlbum, MessageThreadID, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, CancellationToken);
        }
        public static async Task<Message[]> SendMediaGroupAsync<TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, SessionUser User, IEnumerable<IAlbumInputMedia> MediaAlbum, int? MessageThreadID = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new()
        {
            Message[] Msg;
            long ChatId = User.UserID;
            SessionUser SessionUser = User;
            if (Bot.EditMessagesMode)
            {
                if (SessionUser.LastMessage is not null)
                    await Bot.API.DeleteMessageAsync(ChatId, SessionUser.LastMessage.MessageId, CancellationToken);
                Msg = await Bot.SendMediaGroupAsync(ChatId, MediaAlbum, MessageThreadID, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, CancellationToken);
                if (SessionUser.LastMessageFromBot is not null)
                    await Bot.API.DeleteMessageAsync(ChatId, SessionUser.LastMessageFromBot.MessageId, CancellationToken);
            }
            else
            {
                Msg = await Bot.SendMediaGroupAsync(ChatId, MediaAlbum, MessageThreadID, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, CancellationToken);
            }

            SessionUser.LastMessageFromBot = Msg.Last();
            return Msg;
        }
        #endregion

        #region Send-Recieve
        /// <summary>
        /// Sends message with option buttons.
        /// </summary>
        /// <typeparam name="TDB"></typeparam>
        /// <typeparam name="TCredits"></typeparam>
        /// <typeparam name="T">The option type, make sure to override ToString() method, because it displays text on buttons using ToString(), or just define DisplayName param. And if names are gonna to repeat, use UniqueIdentifier to idetify them.</typeparam>
        /// <returns>Choosen option by user.</returns>
        public static async Task<T?> SendMessageWithOptionButtonsAsync<T, TDB, TCredits, TUser>(this TelegramBot<TDB, TCredits, TUser> Bot, SessionUser User, string Text, IEnumerable<T> Options, uint InRow = 3, Func<T, string>? DisplayName = default, Func<T, string>? UniqueIdentifier = default, int? MessageThreadID = default, ParseMode? ParseMode = default, IEnumerable<MessageEntity>? Entities = default, bool? DisableWebPagePreview = default, bool? DisableNotification = default, bool? ProtectContent = default, int? ReplyToMessageId = default, bool? AllowSendingWithoutReply = default, IReplyMarkup? ReplyMarkup = default, CancellationToken CancellationToken = default)
            where TDB : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new()
        {
			if (User.IsGetMessageState || User.IsGetCallbackDataState)
				throw new Exception("Already receiving message!");

			DisplayName ??= x => x.ToString()!;
            UniqueIdentifier ??= DisplayName;

            var OptionButtons = Options
                                    .Select(x => InlineKeyboardButton.WithCallbackData(DisplayName(x), UniqueIdentifier(x)))
                                    .ToArray();

            int SubArrays = (int)Math.Ceiling((double)OptionButtons.Length / InRow);
            InlineKeyboardButton[][] ButtonsJagged = new InlineKeyboardButton[SubArrays][];

            for (int i = 0; i < SubArrays; i++)
            {
                int Start = (int)(i * InRow);
                int End = (int)Math.Min((i + 1) * InRow, OptionButtons.Length);
                ButtonsJagged[i] = new InlineKeyboardButton[End - Start];
                Array.Copy(OptionButtons, Start, ButtonsJagged[i], 0, End - Start);
            }

            var InlineKeyboard = new InlineKeyboardMarkup(ButtonsJagged);

            await Bot.SendMessageAsync(User, Text, MessageThreadID, ParseMode, Entities, DisableWebPagePreview, DisableNotification, ProtectContent, ReplyToMessageId, AllowSendingWithoutReply, InlineKeyboard, CancellationToken);

            var ChoosenOption = await Bot.ReceiveCallbackAsync(User);
            return Options.First(x => UniqueIdentifier(x) == ChoosenOption!.Data);
        }
        #endregion

        #region Misc
        public static async Task<User> GetUserAsync<TDBContext, TCredits, TUser>(this SessionUser User, TelegramBot<TDBContext, TCredits, TUser> Bot)
            where TDBContext : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new() => (await Bot.API.GetChatMemberAsync(User.UserID, User.UserID)).User;
        public static async Task<bool> IsInChatAsync<TDBContext, TCredits, TUser>(this SessionUser User, TelegramBot<TDBContext, TCredits, TUser> Bot, ChatId Chat)
            where TDBContext : TelegramDBContext<TUser>, IDBContext<TCredits>
            where TCredits : struct, IDBCredentials
            where TUser : CustomUser, new()
        {
            ChatMember ChatMember = await Bot.API.GetChatMemberAsync(Chat, User.UserID);
            return ChatMember.Status != ChatMemberStatus.Left && ChatMember.Status != ChatMemberStatus.Kicked;
        }
        #endregion
    }
}
