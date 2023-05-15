using TeleAPI.Bot.DataBase.Models;

namespace TeleAPI.Bot.Request
{
    public delegate Task CommandHandler<TUser>(RequestArgs<TUser> Args)
        where TUser : CustomUser, new();
    [AttributeUsage(AttributeTargets.Method)]
    public class OnCommandAttribute : Attribute
    {
        internal string[] Commands { get; set; }
        public OnCommandAttribute(params string[] Commands) => this.Commands = Commands;
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class OnCallbackDataAttribute : Attribute
    {
        internal string[] Commands { get; set; }
        public OnCallbackDataAttribute(params string[] Commands) => this.Commands = Commands;
    }
}
