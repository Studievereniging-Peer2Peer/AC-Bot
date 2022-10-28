using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ACBot.Discord;

public class Program
{
    private readonly IServiceProvider _serviceProvider;

    public Program()
        => _serviceProvider = CreateServices();

    public static void Main(string[] args) 
        => new Program().MainAsync().GetAwaiter().GetResult();

    public IServiceProvider CreateServices()
    {
        var discordConfig = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.GuildMessages
        };

        var services = new ServiceCollection();
        
        services
            .AddSingleton(discordConfig)
            .AddSingleton<DiscordSocketClient>();

        return services.BuildServiceProvider();
    }
    
    public async Task MainAsync()
    {
        // var client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
        //
        // await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_API_KEY_AC_BOT"));
        // await client.StartAsync();
        
        await Task.Delay(Timeout.Infinite);
    }
    
}