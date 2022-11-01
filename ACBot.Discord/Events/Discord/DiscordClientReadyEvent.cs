using ACBot.Discord.Command;
using ACBot.Discord.Config;
using ACBot.Discord.Scheduled;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace ACBot.Discord.Events.Discord;

public class DiscordClientReadyEvent
{
    
    private readonly IServiceProvider _serviceProvider;
    private readonly DiscordSocketClient _client;

    public DiscordClientReadyEvent(IServiceProvider serviceProvider, DiscordSocketClient client)
    {
        _serviceProvider = serviceProvider;
        _client = client;
    }


    /// <summary>
    /// Handle the Discord client 'Ready' event.
    /// </summary>
    public async Task OnReady()
    {
        await _serviceProvider.GetRequiredService<Configuration>().Load();

        await _serviceProvider.GetRequiredService<CommandRegistry>().RegisterAll();
        await _serviceProvider.GetRequiredService<IScheduler>().Start();

        var commands = _serviceProvider.GetRequiredService<CommandHandler>();
        _client.SlashCommandExecuted += commands.Handle;

        // Enable the communication board reminder service.
        _serviceProvider.GetRequiredService<CommunicationBoardReminder>().Execute();
    }
}