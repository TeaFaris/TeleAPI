using System.ComponentModel.DataAnnotations;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramTool.Bot;
using TeleInstrument.SessionData;

namespace TeleInstrument.DataBase.Models
{
    public class CustomUser
    {
        [Key]
        public long ID { get; init; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "UserID is required!")]
        public long UserID { get; init; }
        public async Task<User> GetUser(ITelegramBotClient Bot) => (await Bot.GetChatMemberAsync(ID, ID)).User;
        public List<UserAction> ActionsHistory { get; init; } = new List<UserAction>();
        public CustomUser(User User) => this.UserID = User.Id;
        public CustomUser(long UserID) => this.UserID = UserID;
        private CustomUser() { }
    }
    public class UserAction
    {
        [Key]
        public long ID { get; init; }
        public UpdateType ActionType { get; init; }
        public string Data { get; init; }
        public CustomUser Owner { get; init; }
        public int OwnerID { get; init; }
        public string DateString { get; set; } = DateTime.Now.ToString("G");
        public DateTime Date => DateTime.Parse(DateString);
        public UserAction(Update Action, CustomUser Owner)
        {
            this.Owner = Owner;
            this.ActionType = Action.Type;
            this.Data = Action.Message?.Text ?? Action.CallbackQuery?.Data ?? "NoData";
        }
        private UserAction() { }
        public override string ToString() => $"[{Date:G}]: {ActionType} = {Data}";
    }
}
