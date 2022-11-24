﻿using Telegram.Bot.Types;
using Telegram.Bot;
using TeleInstrument.DataBase.Models;

namespace TelegramTool.Bot.Request
{
    public readonly struct RequestArgs
    {
        public required CustomUser CustomUser { get; init; }
        public required Chat Chat { get; init; }
        public required Update Update { get; init; }
        public required CancellationToken CancellationToken { get; init; }
        public required string[] Command { get; init; }
    }
}
