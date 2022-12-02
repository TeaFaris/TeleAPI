using Microsoft.EntityFrameworkCore;
using TeleInstrument.DataBase.Models;

namespace TeleInstrument.DataBase
{
    public abstract class TelegramDBContext : DbContext
    {
        public DbSet<CustomUser> Users { get; set; } = null!;
        internal DbSet<UserAction> UsersActions { get; set; } = null!;
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
