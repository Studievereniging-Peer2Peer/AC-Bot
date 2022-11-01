using ACBot.Discord.Command.Commands;
using Discord.WebSocket;

namespace ACBot.Discord.Command;

public class CommandHandler
{

    /// <summary>
    /// Handler for the commands for the communication board.
    /// </summary>
    private readonly CommunicationBoardCommands _communicationBoardCommands;

    public CommandHandler(CommunicationBoardCommands communicationBoardCommands) 
        => _communicationBoardCommands = communicationBoardCommands;

    /// <summary>
    /// Handle the 'highest-level' command arguments of the discord bot.
    /// </summary>
    /// <param name="command">Command information.</param>
    public async Task Handle(SocketSlashCommand command)
    {
        switch (command.Data.Options.FirstOrDefault()?.Name)
        {
            case "commboard":
                await _communicationBoardCommands.HandleCommunicationBoardCommand(
                    command,
                    command.Data.Options.FirstOrDefault().Options
                );
                break;
            default:
                await command.RespondAsync("Dit is een onbekend commando.");
                break;
        }
    }
}