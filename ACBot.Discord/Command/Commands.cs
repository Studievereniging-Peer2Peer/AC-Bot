using System.Collections.Immutable;
using ACBot.Discord.Command.Utilities;
using ACBot.Discord.Scheduled;
using Discord;
using Discord.WebSocket;
using Quartz;

namespace ACBot.Discord.Command;

public class Commands
{
    private readonly DiscordSocketClient _client;
    private readonly IScheduler _scheduler;

    public Commands(DiscordSocketClient client, IScheduler scheduler)
    {
        _client = client;
        _scheduler = scheduler;
    }

    public async Task HandleACCommand(SocketSlashCommand command)
    {
        switch (command.Data.Options.FirstOrDefault()?.Name)
        {
            case "commboard":
                await HandleCommunicationBoardCommand(command, command.Data.Options.FirstOrDefault().Options);
                break;
            default:
                await command.RespondAsync("Dit is een onbekend commando.");
                break;
        }
    }

    public async Task HandleCommunicationBoardCommand(SocketSlashCommand command,
        IReadOnlyCollection<SocketSlashCommandDataOption> options)
    {
        switch (options.FirstOrDefault()?.Name)
        {
            case "set":
                await HandleCommunicationBoardSetCommand(command, options.FirstOrDefault().Options);
                break;
            default:
                await command.RespondAsync("Dit is een onbekend commando.");
                return;
        }
    }

    public async Task HandleCommunicationBoardSetCommand(SocketSlashCommand command,
        IReadOnlyCollection<SocketSlashCommandDataOption> options)
    {
        var extractedOptions = options.ToImmutableDictionary(
            x => x.Name,
            x => x.Value
        );

        // If we do not have the required data for the command to work, abort.
        if (extractedOptions.IsEmpty ||
            !(extractedOptions.ContainsKey("channel") || extractedOptions.ContainsKey("dag") ||
              extractedOptions.ContainsKey("uur")))
        {
            await command.RespondAsync(
                "Vergeet niet om alle verplichte velden in te vullen! ('channel', 'dag', 'uur')"
            );
            return;
        }

        // If the given channel doesn't actually exist, cancel the operation.
        if (null == extractedOptions["channel"])
        {
            await command.RespondAsync(
                "Zorg dat je een bestaand kanaal aangeeft!"
            );
            return;
        }


        // If the given day is not a number, abort.
        if (!int.TryParse(extractedOptions["dag"].ToString(), out int dayNum))
        {
            await command.RespondAsync(
                "Zorg dat je een bestaande dag aangeeft!"
            );
            return;
        }

        // If we have been given an invalid number for the day, abort.
        DayOfWeek? day = ConversionUtilities.IntegerToDay(dayNum);
        if (day == null)
        {
            await command.RespondAsync(
                "Zorg dat je een bestaande dag aangeeft!"
            );
            return;
        }

        // If we have been given an invalid hour, abort.
        if (!int.TryParse(extractedOptions["uur"].ToString(), out int hourNum) || hourNum is < 0 or > 23)
        {
            await command.RespondAsync(
                "Zorg dat je een bestaand uur aangeeft! (0-23)"
            );
            return;
        }

        // If we have a 'minute' parameter, make sure it is valid as well.
        int minute = 0;
        if (extractedOptions.ContainsKey("minuut")
            && (!int.TryParse(extractedOptions["minuut"].ToString(), out minute)
                || minute is < 0 or > 59))
        {
            await command.RespondAsync(
                "Zorg dat - indien je de minuut parameter aan wil geven - een valide waarde aangeeft! (0-59)"
            );
            return;
        }

        var dayOfWeek = (DayOfWeek) day;

        // TODO: store new date/time in persistent data store, make sure 'channel' also gets used.
        
        // Restart scheduled job with new trigger.
        await _scheduler.RescheduleJob(
            new TriggerKey("CommunicationBoardReminderTrigger", "reminders"),
            CommunicationBoardReminder.GetCommunicationBoardReminderTrigger(dayOfWeek, hourNum, minute)
        );

        // TODO: Format this nicely.
        await command.RespondAsync(
            $"De reminder voor het communicatiebord is ingesteld op: {day} {hourNum}h {minute}m."
        );
    }
}