using C3.LINQ;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramTool.Bot.Request;
using TeleInstrument.DataBase;
using TeleInstrument.DataBase.Models;
using TeleInstrument.SessionData;
using TeleTool.Bot.Logger;

namespace TelegramTool.Bot
{
    public abstract class TelegramBot<TDB, TDBCredentials>
        where TDB : TelegramDBContext, IDBContext<TDBCredentials>
        where TDBCredentials : IDBCredentials
    {
        protected const string DEFAULT = "_";
        [NotNull]
        protected abstract string Token { get; }
        [NotNull]
        protected abstract TDBCredentials Credits { get; }
        protected abstract ILogger Logger { get; }
        internal protected virtual bool EditMessagesMode => false;
        internal protected TelegramBotClient Api { get; private set; } = null!;
        protected User Botself { get; private set; } = null!;
        protected CancellationTokenSource CTS { get; private set; } = null!;
        /// <summary>
        /// Use in using block, or dispose it after using.
        /// </summary>
        /// <returns>Database context</returns>
        protected TDB GetDataBase() => (TDB)Activator.CreateInstance(typeof(TDB), Credits);
        /// <summary>
        /// Gets a Session User Data.
        /// </summary>
        /// <param name="CustomUser"></param>
        /// <returns>Session user if user texted in this session, otherwise null.</returns>
        internal protected SessionUser? GetSessionUser(CustomUser CustomUser) => SessionUsers.FirstOrDefault(User => User.UserID == CustomUser.UserID);
        private List<SessionUser> SessionUsers { get; init; } = new List<SessionUser>();
        private Dictionary<string, CommandHandler> CommandHandlers { get; init; } = new Dictionary<string, CommandHandler>();
        private Dictionary<string, CommandHandler> CallbackHandlers { get; init; } = new Dictionary<string, CommandHandler>();
        public TelegramBot() => SettingHandlers();
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
            if (CTS is not null && CTS.IsCancellationRequested)
            {
                CTS.Cancel();
                Api = null!;
                Botself = null!;
            }
            Logger.LogInformation("Telegram bot has stopped.");
        }
        private void SettingHandlers()
        {
            Logger.LogInformation($"Analyzing all handlers...");

            MethodInfo[] MethodCommandHandlers = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            Logger.LogInformation($"Founded {MethodCommandHandlers.Length} methods.");

            foreach (var CommandHandlerMethod in MethodCommandHandlers)
            {
                IEnumerable<OnCommandAttribute> OnCommand = CommandHandlerMethod.GetCustomAttributes<OnCommandAttribute>(false);
                IEnumerable<OnCommandAttribute> OnCallback = CommandHandlerMethod.GetCustomAttributes<OnCallbackDataAttribute>(false);

                OnCommand.IterateThroughAll(CommandHandler => CommandHandler.Commands.IterateThroughAll(Command =>
                {
                    CommandHandlers.Add(Command, CommandHandlerMethod.CreateDelegate<CommandHandler>(this));
                    Logger.LogDebug($"Added listener {CommandHandlerMethod.Name} to command {Command}.");
                }));
                OnCallback.IterateThroughAll(CallbackHandler => CallbackHandler.Commands.IterateThroughAll(Command =>
                {
                    CallbackHandlers.Add(Command, CommandHandlerMethod.CreateDelegate<CommandHandler>(this));
                    Logger.LogDebug($"Added listener {CommandHandlerMethod.Name} to callback {Command}.");
                }));
            }
        }

        #region UpdateHandlers
        private async Task HandleUpdateAsync(ITelegramBotClient BotClient, Update Update, CancellationToken CT)
        {
            Logger.LogInformation($"Recieved new update of type {Update.Type}.");

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
                    Logger.LogInformation($"Added new Session User: {User.Username}");
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
                    Logger.LogInformation($"Added new Custom User: {CustomUser.UserID}");
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
                User = Update.CallbackQuery.From;
                Chat = await Api.GetChatAsync(User.Id, CT);
                Command = Update.CallbackQuery?.Data?.Trim().Split(' ') ?? new[] { " " };

                SessionUserCheck();
                await DBUserCheck();

                HandleHandlers(CallbackHandlers, CreateArgs());
            }
            #endregion
        }
        private async Task HandlePollingErrorAsync(ITelegramBotClient BotClient, Exception Exception, CancellationToken CT)
        {
            await Task.CompletedTask;

            Logger.LogCritical(Exception);
        }
        #endregion
    }
}
