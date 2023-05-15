using TeleAPI.Bot.DataBase.Models;
using Telegram.Bot.Types;

namespace TeleAPI.Bot.Request
{
    public class RequestArgs<TUser>
        where TUser : CustomUser, new()
    {
        public required TUser CustomUser { get; init; }
        public required SessionUser SessionUser { get; init; }
        public required Chat Chat { get; init; }
        public required Update Update { get; init; }
        public required CancellationToken CancellationToken { get; init; }
        public required string[] Command { get; init; }
        public object[] Args { get; set; }
    }
}
