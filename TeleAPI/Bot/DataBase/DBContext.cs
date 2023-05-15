using Microsoft.EntityFrameworkCore;
using TeleAPI.Bot.DataBase.Models;

namespace TeleAPI.Bot.DataBase
{
    public abstract class TelegramDBContext<TUser> : DbContext where TUser : CustomUser, new()
    {
        public DbSet<TUser> Users { get; set; } = null!;
        internal DbSet<UserAction> UsersActions { get; set; } = null!;

        protected TelegramDBContext(DbContextOptions options) : base(options) { }
        protected TelegramDBContext() { }
    }
    public interface IDBContext<TCredentials> where TCredentials : IDBCredentials
    {
        protected TCredentials Credits { get; init; }
    }
    public interface IDBCredentials
    {
        public string Host { get; init; }
        public uint Port { get; init; }
        public string Password { get; init; }
        public string Username { get; init; }
    }
}
