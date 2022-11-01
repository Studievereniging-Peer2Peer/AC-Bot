using ACBot.Discord.Events.Discord;
using ACBot.Discord.Extensions;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ACBot.Discord;

public class Program
{
    private readonly IServiceProvider _serviceProvider;

    public Program()
        => _serviceProvider = CreateDependencyInjectionServices();

    public static void Main(string[] args) 
        => new Program().MainAsync().GetAwaiter().GetResult();

    public async Task MainAsync()
    {
        var client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
        client.Log += new DiscordClientLogEvent().Log;
        
        await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_API_KEY_AC_BOT"));
        await client.StartAsync();

        client.Ready += new DiscordClientReadyEvent(_serviceProvider, client).OnReady;
        
        await Task.Delay(Timeout.Infinite);
    }
    
    public IServiceProvider CreateDependencyInjectionServices()
    {
        var discordConfig = new DiscordSocketConfig()
        {
            GatewayIntents = 
                GatewayIntents.MessageContent |
                GatewayIntents.GuildMessages  | 
                GatewayIntents.Guilds,
            AlwaysDownloadUsers = true
        };

        var services = new ServiceCollection();

        services
            .AddDiscord(discordConfig)
            .AddConfiguration()
            .AddReminders()
            .AddCommands()
            .AddScheduler();

        return services.BuildServiceProvider();
    }
    
}