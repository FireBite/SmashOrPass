using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SmashOrPass.Log;

namespace SmashOrPass
{
    public class Bot
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public string Token { get; private set; }
        public string MessagePrefix { get; set; } = "!";

        public Bot(string token)
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            Token = token;

            _client.Log += Log;
        }

        public async Task StartBot()
        {
            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, Token, false);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += OnMessageReceived;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task OnMessageReceived(SocketMessage socketMessage)
        {
            if (!(socketMessage is SocketUserMessage message) || message.Author.IsBot) return;

            int commandStartPoint = -1;
            if (message.HasStringPrefix(MessagePrefix, ref commandStartPoint) || message.HasMentionPrefix(_client.CurrentUser, ref commandStartPoint))
            {
                Logger.Log($"Msg: {message.Content}", "MsgRecv", LogSeverity.Info);

                var context = new SocketCommandContext(_client, message);
                var result = await _commands.ExecuteAsync(context, commandStartPoint, _services);

                if(!result.IsSuccess)
                    Logger.Log($"Result: {result.Error}", "MsgRecv", LogSeverity.Error);
            }
        }

        private Task Log(LogMessage msg)
        {
            Logger.Log(new Message(){Level = msg.Severity, Origin = msg.Source, Text = msg.Message});
            return Task.CompletedTask;
        }
    }
}
