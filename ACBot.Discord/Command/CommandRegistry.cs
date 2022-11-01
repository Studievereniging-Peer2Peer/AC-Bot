using Discord;
using Discord.WebSocket;

namespace ACBot.Discord.Command;

public class CommandRegistry
{
    private readonly DiscordSocketClient _client;

    public CommandRegistry(DiscordSocketClient client)
        => _client = client;

    /// <summary>
    /// Register all SlashCommand information for the client-side.
    /// This registers all the SlashCommand helper things on the client-side.
    /// </summary>
    public async Task RegisterAll()
    {
        var ACBotCommands = new SlashCommandBuilder()
            .WithName("ac")
            .WithDescription("activiteitencommissie bot hoofdcommand")
            .AddOption(RegisterCommunicationBoardSubCommand())
            .Build();

        // Send command creation to target guild(s).
        foreach (var clientGuild in _client.Guilds)
            await clientGuild.CreateApplicationCommandAsync(ACBotCommands);
    }


    /// <summary>
    /// Register SlashCommand information for the client-side.
    /// </summary>
    /// <returns>The SlashCommandOptionBuilder containing all command info for the commboard sub command.</returns>
    private SlashCommandOptionBuilder RegisterCommunicationBoardSubCommand()
        => new SlashCommandOptionBuilder()
            .WithName("commboard")
            .WithType(ApplicationCommandOptionType.SubCommandGroup)
            .WithDescription("Communicatieboard reminder configuratie")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("set")
                .WithDescription("Configureer de bot om reminders te sturen")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(
                    new SlashCommandOptionBuilder()
                        .WithName("channel")
                        .WithType(ApplicationCommandOptionType.Channel)
                        .WithDescription("De channel waarin de reminders gestuurd worden")
                        .WithRequired(true)
                )
                .AddOption(
                    new SlashCommandOptionBuilder()
                        .WithName("dag")
                        .WithDescription("De dag waarop de reminder gestuurd wordt")
                        .WithRequired(true)
                        .AddChoice("Maandag", 1)
                        .AddChoice("Dinsdag", 2)
                        .AddChoice("Woensdag", 3)
                        .AddChoice("Donderdag", 4)
                        .AddChoice("Vrijdag", 5)
                        .AddChoice("Zaterdag", 6)
                        .AddChoice("Zondag", 0)
                        .WithType(ApplicationCommandOptionType.Integer)
                )
                .AddOption(
                    new SlashCommandOptionBuilder()
                        .WithName("uur")
                        .WithType(ApplicationCommandOptionType.Integer)
                        .WithDescription("Het uur waarop de reminder gestuurd wordt")
                        .WithRequired(true)
                )
                .AddOption(
                    new SlashCommandOptionBuilder()
                        .WithName("minuut")
                        .WithType(ApplicationCommandOptionType.Integer)
                        .WithDescription("De minuut waarop de reminder gestuurd wordt")
                        .WithRequired(false)
                )
            );
}