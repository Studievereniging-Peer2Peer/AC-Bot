using ACBot.Discord.Command;
using ACBot.Discord.Scheduled;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace ACBot.Discord;

public class Program
{
    private readonly IServiceProvider _serviceProvider;

    public Program()
        => _serviceProvider = CreateServices();

    public static void Main(string[] args) 
        => new Program().MainAsync().GetAwaiter().GetResult();

    public async Task MainAsync()
    {
        var client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
        client.Log += Log;
        
        await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_API_KEY_AC_BOT"));
        await client.StartAsync();

        client.Ready += async () =>
        {
            await _serviceProvider.GetRequiredService<CommandRegistry>().RegisterAll();
            await _serviceProvider.GetRequiredService<IScheduler>().Start();
            
            var commands = _serviceProvider.GetRequiredService<Commands>();
            client.SlashCommandExecuted += commands.HandleACCommand;
        };
        
        // Enable the communication board reminder service.
        _serviceProvider.GetRequiredService<CommunicationBoardReminder>().Execute();

        await Task.Delay(Timeout.Infinite);
    }
    
    public IServiceProvider CreateServices()
    {
        var discordConfig = new DiscordSocketConfig()
        {
            GatewayIntents = 
                GatewayIntents.MessageContent |
                GatewayIntents.GuildMessages  | 
                GatewayIntents.Guilds
            
        };

        var services = new ServiceCollection();

        services.AddSingleton(discordConfig)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommunicationBoardReminder>()
            .AddSingleton<CommandRegistry>()
            .AddSingleton<Commands>()
            .AddSingleton<IScheduler>(x => SchedulerBuilder.Create().BuildScheduler().Result);

        return services.BuildServiceProvider();
    }

    public Task Log(LogMessage message) => Console.Out.WriteLineAsync(message.Message);
}