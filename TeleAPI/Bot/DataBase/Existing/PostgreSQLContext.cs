using Microsoft.EntityFrameworkCore;

namespace TeleAPI.Bot.DataBase.Existing
{
    public class PostgreSQLContext : TelegramDBContext, IDBContext<PostgresCreditionals>
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
