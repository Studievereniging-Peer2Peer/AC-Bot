using System.Collections.Immutable;
using ACBot.Discord.Command.Utilities;
using ACBot.Discord.Config;
using ACBot.Discord.Scheduled;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Quartz;

namespace ACBot.Discord.Command;

public class Commands
{
    private readonly IScheduler _scheduler;
    private readonly Configuration _config;

    public Commands(IScheduler scheduler, Configuration config)
    {
        _scheduler = scheduler;
        _config = config;
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

        if (!(command.User as SocketGuildUser)!.GuildPermissions.Has(GuildPermission.Administrator))
        {
            await command.RespondAsync(
                "Je hebt administratieve rechten nodig om deze command uit te mogen voeren."
            );
            return;
        }
        
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
        if (extractedOptions["channel"] is not SocketTextChannel channel)
        {
            await command.RespondAsync(
                "Zorg dat je een valide kanaal aangeeft!"
            );
            return;
        }

        // If the given day is not a number, abort.
        if (!int.TryParse(extractedOptions["dag"].ToString(), out int dayNum) || dayNum is < 0 or > 6)
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

        var dayOfWeek = (DayOfWeek) day;
        
        // If we have been given an invalid hour, abort.
        if (!int.TryParse(extractedOptions["uur"].ToString(), out int hour) || hour is < 0 or > 23)
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

        await UpdateCommunicationBoardReminderConfiguration(channel, dayOfWeek, hour, minute);

        // Restart scheduled job with new trigger.
        await _scheduler.RescheduleJob(
            new TriggerKey("CommunicationBoardReminderTrigger", "reminders"),
            CommunicationBoardReminder.GetCommunicationBoardReminderTrigger(dayOfWeek, hour, minute)
        );

        // TODO: Format this nicely.
        await command.RespondAsync(
            $"De reminder voor het communicatiebord is ingesteld op: {day} {hour}h {minute}m."
        );
    }

    private async Task UpdateCommunicationBoardReminderConfiguration(SocketTextChannel channel, DayOfWeek dayOfWeek,
        int hour, int minute)
    {
        // Update configuration to set new reality.
        // commboard_reminder_channel => channel to message in
        // commboard_reminder_dag => day to send the message at
        // commboard_reminder_uur => hour to send the message at (24 hour clock)
        // commboard_reminder_minuut => minute to send the message at
        _config.ConfigurationMappings["commboard_reminder_channel"] = channel.Id;
        _config.ConfigurationMappings["commboard_reminder_dag"] = dayOfWeek;
        _config.ConfigurationMappings["commboard_reminder_uur"] = hour;
        _config.ConfigurationMappings["commboard_reminder_minuut"] = minute;

        // Save the changes. This is an I/O operation, thus it may be interesting to handle it
        // in another way in the future. E.g. an event for when the console application exits.
        // However do note that the consistency is of utmost importance.
        // For now it works, seeing as I don't think this command will be executed often.
        await _config.Save();
    }
}