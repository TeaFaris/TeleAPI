using Microsoft.EntityFrameworkCore;
using TeleAPI.Bot.DataBase.Models;

namespace TeleAPI.Bot.DataBase.Existing
{
    public class PostgreSQLContext<TUser> : TelegramDBContext<TUser>, IDBContext<PostgresCreditionals> where TUser : CustomUser, new()
    {
        public PostgresCreditionals Credits { get; init; }
        public PostgreSQLContext(PostgresCreditionals Credits)
        {
            this.Credits = Credits;
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder OptionsBuilder) => OptionsBuilder.UseNpgsql($"""
                                                                                                                  Server={Credits.Host};
                                                                                                                  Port={Credits.Port};
                                                                                                                  Database={Credits.DataBase};
                                                                                                                  User ID={Credits.Username};
                                                                                                                  Password={Credits.Password};
                                                                                                                  Pooling=true;
                                                                                                                  Connection Lifetime=0;
                                                                                                                  SslMode=Disable;
                                                                                                                  SslMode=Disable;
                                                                                                                  """);
    }
    public readonly struct PostgresCreditionals : IDBCredentials
    {
        public PostgresCreditionals() { }
        public required string DataBase { get; init; }
        public string Host { get; init; } = "localhost";
        public uint Port { get; init; } = 5432;
        public required string Password { get; init; }
        public string Username { get; init; } = "postgres";
    }
}
