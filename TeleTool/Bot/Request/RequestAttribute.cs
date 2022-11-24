namespace TelegramTool.Bot.Request
{
    public delegate Task CommandHandler(RequestArgs Args);
    [AttributeUsage(AttributeTargets.Method)]
    public class OnCommandAttribute : Attribute
    {
        internal string[] Commands { get; set; }
        public OnCommandAttribute(params string[] Commands) => this.Commands = Commands;
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class OnCallbackDataAttribute : OnCommandAttribute
    {
        public OnCallbackDataAttribute(params string[] Commands) : base(Commands) { }
    }
}
