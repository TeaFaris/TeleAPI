namespace TeleAPI.Bot.Request
{
    internal class RequestHandler
    {
        internal CommandHandler Delegate { get; init; }
        internal RequestHandler(CommandHandler Delegate)
        {
            this.Delegate = Delegate;
        }
    }
}
