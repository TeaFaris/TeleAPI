using Telegram.Bot.Types;

namespace TeleAPI.Bot.DataBase.Models
{
    public class SessionUser
    {
        public long UserID { get; init; }
        public Message? LastMessageFromBot { get; internal set; }
        public Message? LastMessage { get; internal set; }
        internal bool IsGetMessageState { get; set; } = false;
        internal Message? GetMessageStateMessage { get; set; }
        public SessionUser(User User) => UserID = User.Id;
        public SessionUser(long UserID) => this.UserID = UserID;
    }
}
