using ACBot.Discord.Command;
using ACBot.Discord.Command.Commands;
using ACBot.Discord.Config;
using ACBot.Discord.Scheduled;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace ACBot.Discord.Extensions;

public static class DependencyInjectionExtension
{

    /// <summary>
    /// Register the reminder dependencies.
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static IServiceCollection AddReminders(this IServiceCollection collection)
        => collection.AddSingleton<CommunicationBoardReminder>();
    
    /// <summary>
    /// Register the configuration dependencies.
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static IServiceCollection AddConfiguration(this IServiceCollection collection)
        => collection.AddSingleton<Configuration>();

    /// <summary>
    /// Register the Discord dependencies.
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="discordConfig">The configuration of the discord bot.</param>
    /// <returns></returns>
    public static IServiceCollection AddDiscord(this IServiceCollection collection, DiscordSocketConfig discordConfig)
        => collection.AddSingleton(discordConfig)
            .AddSingleton<DiscordSocketClient>();

    /// <summary>
    /// Register the scheduler dependencies.
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static IServiceCollection AddScheduler(this IServiceCollection collection)
        => collection.AddSingleton(SchedulerBuilder.Create().BuildScheduler().Result);

    /// <summary>
    /// Register the command dependencies.
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static IServiceCollection AddCommands(this IServiceCollection collection)
        => collection.AddSingleton<CommandRegistry>()
            .AddSingleton<CommunicationBoardCommands>()
            .AddSingleton<CommandHandler>();
}