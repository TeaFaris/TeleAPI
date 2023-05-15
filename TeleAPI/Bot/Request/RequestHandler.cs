using TeleAPI.Bot.DataBase.Models;

namespace TeleAPI.Bot.Request
{
    internal class RequestHandler<TUser>
        where TUser : CustomUser, new()
    {
        internal CommandHandler<TUser> Delegate { get; init; }
        internal RequestHandler(CommandHandler<TUser> Delegate)
        {
            this.Delegate = Delegate;
        }
    }
}
