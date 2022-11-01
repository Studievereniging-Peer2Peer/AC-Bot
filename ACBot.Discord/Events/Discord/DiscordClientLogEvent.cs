using Discord;

namespace ACBot.Discord.Events.Discord;

public class DiscordClientLogEvent
{

    /// <summary>
    /// Log Discord client messages.
    /// </summary>
    /// <param name="message"></param>
    public async Task Log(LogMessage message) 
        => await Console.Out.WriteLineAsync(message.Message);
}