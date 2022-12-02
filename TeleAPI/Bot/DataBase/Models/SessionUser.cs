using Telegram.Bot.Types;

namespace TeleInstrument.SessionData
{
    public class SessionUser
    {
        public long UserID { get; init; }
        public Message? LastMessageFromBot { get; internal set; }
        public Message? LastMessage { get; internal set; }
        internal bool IsGetMessageState { get; set; } = false;
        internal Message? GetMessageStateMessage { get; set; }
        public SessionUser(User User) => this.UserID = User.Id;
        public SessionUser(long UserID) => this.UserID = UserID;
    }
}
