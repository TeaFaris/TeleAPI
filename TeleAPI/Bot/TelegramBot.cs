using C3.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using TeleAPI.Bot.DataBase;
using TeleAPI.Bot.DataBase.Existing;
using TeleAPI.Bot.DataBase.Models;
using TeleAPI.Bot.Logger;
using TeleAPI.Bot.Request;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TeleAPI.Bot
{
    public abstract class TelegramBot<TDB, TDBCredentials>
        where TDB : TelegramDBContext, IDBContext<TDBCredentials>
        where TDBCredentials : struct, IDBCredentials
    {
        protected const string DEFAULT = "_";
        [NotNull]
        protected abstract string Token { get; }
        [AllowNull]
        protected abstract TDBCredentials? Credits { get; }
        [AllowNull]
        protected abstract ILogger? Logger { get; }
        internal protected virtual bool EditMessagesMode => false;
        internal protected TelegramBotClient Api { get; private set; } = null!;
        protected User Botself { get; private set; } = null!;
        protected CancellationTokenSource CTS { get; private set; } = null!;
        /// <summary>
        /// Use in using block, or dispose it after using.
        /// </summary>
        /// <returns>Database context</returns>
        protected TDB GetDataBase()
        {
            if (Credits is null)
                throw new Exception("Current bot using no database.");

            return (TDB)Activator.CreateInstance(typeof(TDB), Credits)!;
        }
        /// <summary>
        /// Gets a Session User Data.
        /// </summary>
        /// <param name="CustomUser"></param>
        /// <returns>Session user if user texted in this session, otherwise null.</returns>
        internal protected SessionUser? GetSessionUser(CustomUser CustomUser) => SessionUsers.Find(User => User.UserID == CustomUser.UserID);
        private List<SessionUser> SessionUsers { get; init; } = new List<SessionUser>();
        private Dictionary<string, CommandHandler> CommandHandlers { get; init; } = new Dictionary<string, CommandHandler>();
        private Dictionary<string, CommandHandler> CallbackHandlers { get; init; } = new Dictionary<string, CommandHandler>();
        protected TelegramBot() => SettingHandlers();
        public async void Run()
        {
            CTS = new CancellationTokenSource();
            ReceiverOptions ReceiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };
            Api = new TelegramBotClient(Token);
            Botself = await Api.GetMeAsync();

            Api.StartReceiving(HandleUpdateAsync, HandlePollingErrorAsync, ReceiverOptions, CTS.Token);
        }
        public void Stop()
        {
            if (CTS?.IsCancellationRequested == true)
            {
                CTS.Cancel();
                Api = null!;
                Botself = null!;
            }
            Logger?.LogInformation("Telegram bot has stopped.");
        }
        private void SettingHandlers()
        {
            Logger?.LogDebug("Analyzing all handlers...");

            MethodInfo[] MethodCommandHandlers = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            Logger?.LogDebug($"Founded {MethodCommandHandlers.Length} methods.");

            OnCommandAttribute? OnCommand;
            OnCommandAttribute? OnCallback;
            foreach (var CommandHandlerMethod in MethodCommandHandlers)
            {
                OnCommand = CommandHandlerMethod.GetCustomAttribute<OnCommandAttribute>(false);
                OnCallback = CommandHandlerMethod.GetCustomAttribute<OnCallbackDataAttribute>(false);

                OnCommand?.Commands.ForEach(Command =>
                {
                    CommandHandlers.Add(Command, CommandHandlerMethod.CreateDelegate<CommandHandler>(this));
                    Logger?.LogDebug($"Binded listener \"{CommandHandlerMethod.Name}\" to command \"{Command}\".");
                });
                OnCallback?.Commands.ForEach(Command =>
                {
                    CallbackHandlers.Add(Command, CommandHandlerMethod.CreateDelegate<CommandHandler>(this));

                    Logger?.LogDebug($"Binded listener \"{CommandHandlerMethod.Name}\" to callback \"{Command}\".");
                });
            }
        }

        #region UpdateHandlers
        private async Task HandleUpdateAsync(ITelegramBotClient BotClient, Update Update, CancellationToken CT)
        {
            User User = null!;
            CustomUser CustomUser = null!;
            SessionUser SessionUser = null!;
            Chat Chat = null!;
            string[]? Command = null;

            void SessionUserCheck()
            {
                if (!SessionUsers.Any(SessionUser => SessionUser.UserID == User.Id))
                {
                    SessionUsers.Add(new SessionUser(User));
                    Logger?.LogDebug($"Added new Session User: {User.Username}");
                }

                SessionUser = SessionUsers.First(SessionUser => SessionUser.UserID == User.Id);
                SessionUser.LastMessage = Update.Message;
            }
            async Task DBUserCheck()
            {
                using TDB DB = GetDataBase();
                if (!DB.Users.Any(DBUser => DBUser.UserID == User.Id))
                {
                    CustomUser = new CustomUser(User);
                    CustomUser.ActionsHistory.Add(new UserAction(Update, CustomUser));
                    DB.Users.Add(CustomUser);
                    Logger?.LogDebug($"Added new Custom User: {CustomUser.UserID}");
                }
                else
                {
                    CustomUser = DB.Users.First(DBUser => DBUser.UserID == User.Id);
                    CustomUser.ActionsHistory.Add(new UserAction(Update, CustomUser));
                    DB.Users.Update(CustomUser);
                }
                await DB.SaveChangesAsync(CT);
            }
            RequestArgs CreateArgs() => new()
            {
                CancellationToken = CT,
                SessionUser = SessionUser,
                Chat = Chat,
                Command = Command,
                CustomUser = CustomUser,
                Update = Update
            };
            void HandleHandlers(Dictionary<string, CommandHandler> Handlers, RequestArgs Args)
            {
                if (Handlers.TryGetValue(Command[0], out CommandHandler Handler))
                    _ = Handler.Invoke(Args);
                else if (Handlers.TryGetValue(DEFAULT, out CommandHandler Default))
                    _ = Default.Invoke(Args);
            }

            #region Command Handle
            if (Update.Type == UpdateType.Message)
            {
                User = Update.Message!.From!;
                Chat = Update.Message.Chat;
                Command = Update.Message?.Text?.Trim().Split(' ') ?? new[] { " " };

                SessionUserCheck();
                if(Credits is not null)
                    await DBUserCheck();

                if (SessionUser.IsGetMessageState)
                {
                    SessionUser.GetMessageStateMessage = Update.Message;
                    return;
                }

                HandleHandlers(CommandHandlers, CreateArgs());
            }
            #endregion

            #region CallbackData Handle
            else if (Update.Type == UpdateType.CallbackQuery)
            {
                User = Update.CallbackQuery!.From;
                Chat = await Api.GetChatAsync(User.Id, CT);
                Command = Update.CallbackQuery?.Data?.Trim().Split(' ') ?? new[] { " " };

                SessionUserCheck();
                if (Credits is not null)
                    await DBUserCheck();

                HandleHandlers(CallbackHandlers, CreateArgs());
            }
            #endregion
        }
        private async Task HandlePollingErrorAsync(ITelegramBotClient BotClient, Exception Exception, CancellationToken CT)
        {
            await Task.CompletedTask;

            Logger?.LogCritical(Exception);
        }
        #endregion
    }
    public abstract class TelegramBot : TelegramBot<PostgreSQLContext, PostgresCreditionals>
    {
        protected override PostgresCreditionals? Credits => null;
    }
}
