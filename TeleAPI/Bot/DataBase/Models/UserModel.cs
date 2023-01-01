using System.ComponentModel.DataAnnotations;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TeleAPI.Bot.DataBase.Models
{
    public class CustomUser
    {
        [Key]
        public long ID { get; init; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "UserID is required!")]
        public long UserID { get; init; }
        public async Task<User> GetUser(ITelegramBotClient Bot) => (await Bot.GetChatMemberAsync(ID, ID)).User;
        public List<UserAction> ActionsHistory { get; init; } = new List<UserAction>();
        public CustomUser(User User) => UserID = User.Id;
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
            ActionType = Action.Type;
            Data = Action.Message?.Text ?? Action.CallbackQuery?.Data ?? "NoData";
        }
        private UserAction() { }
        public override string ToString() => $"[{Date:G}]: {ActionType} = {Data}";
    }
}
