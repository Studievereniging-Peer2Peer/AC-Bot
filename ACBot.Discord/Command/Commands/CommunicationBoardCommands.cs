using System.Collections.Immutable;
using ACBot.Discord.Command.Utilities;
using ACBot.Discord.Config;
using ACBot.Discord.Config.Models;
using ACBot.Discord.Scheduled;
using Discord;
using Discord.WebSocket;
using Quartz;

namespace ACBot.Discord.Command.Commands;

public class CommunicationBoardCommands
{
    private readonly Configuration _config;
    private readonly IScheduler _scheduler;

    public CommunicationBoardCommands(Configuration config, IScheduler scheduler)
    {
        _config = config;
        _scheduler = scheduler;
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
                break;
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

        var extractedOptions = GenerateKeyValuePairCommandOptions(options);

        // If we do not have the required data for the command to work, abort.
        if (!InputValidationUtility.RequiredOptionsExist(extractedOptions, new[] {"channel", "dag", "uur"}))
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

        // given day must be valid (enum value 0-6)
        if (!InputValidationUtility.IsValidDay(GetOptionDataAsString(extractedOptions, "dag"), out var dayOfWeek))
        {
            await command.RespondAsync(
                "Zorg dat je een bestaande dag aangeeft!"
            );
            return;
        }

        // given hour must be valid (0-23)
        if (!InputValidationUtility.IsValidHourInDay(GetOptionDataAsString(extractedOptions, "uur"), out var hour))
        {
            await command.RespondAsync(
                "Zorg dat je een valide uur aangeeft! (0-23)"
            );
            return;
        }

        // given minute needs to be valid (0-59)
        if (!InputValidationUtility.IsValidMinuteInHour(GetOptionDataAsString(extractedOptions, "minuut"), out var minute))
        {
            await command.RespondAsync(
                "Zorg dat - indien je de minuut parameter aan wil geven - een valide waarde aangeeft! (0-59)"
            );
            return;
        }

        // Update the configuration file
        await UpdateCommunicationBoardReminderConfiguration(channel, (DayOfWeek) dayOfWeek, (int) hour, (int) minute);

        // Restart scheduled job with new trigger.
        await _scheduler.RescheduleJob(
            new TriggerKey("CommunicationBoardReminderTrigger", "reminders"),
            CommunicationBoardReminder.GetCommunicationBoardReminderTrigger((DayOfWeek) dayOfWeek, (int) hour,
                (int) minute)
        );

        await command.RespondAsync(
            $"De reminder voor het communicatiebord is ingesteld op: {dayOfWeek} {hour}h {minute}m."
        );
    }

    /// <summary>
    /// Update the configuration for the communication board, and save it for persistency purposes afterwards.
    /// </summary>
    /// <param name="channel">The text channel to which reminders are to be sent.</param>
    /// <param name="dayOfWeek">The day of the week to send the reminder at.</param>
    /// <param name="hour">The hour of the day to send the reminder at.</param>
    /// <param name="minute">The minute of the day to send the reminder at.</param>
    private async Task UpdateCommunicationBoardReminderConfiguration(SocketTextChannel channel, DayOfWeek dayOfWeek,
        int hour, int minute)
    {
        var commBoardReminder = _config.ConfigurationMappings
                                    .GetValueOrDefault("commboard_reminder", new Reminder()) as Reminder
                                ?? new Reminder();

        commBoardReminder.ChannelId = channel.Id;
        commBoardReminder.Day = dayOfWeek;
        commBoardReminder.Hour = hour;
        commBoardReminder.Minute = minute;

        _config.ConfigurationMappings["commboard_reminder"] = commBoardReminder;

        await _config.Save();
    }

    /// <summary>
    /// Convert the list of options into a dictionary.
    /// </summary>
    /// <param name="options">The collection of options.</param>
    /// <returns>The key-value pair dictionary, Name:Value.</returns>
    private static ImmutableDictionary<string, object> GenerateKeyValuePairCommandOptions(
        IReadOnlyCollection<SocketSlashCommandDataOption> options)
        => options.ToImmutableDictionary(
            x => x.Name,
            x => x.Value
        );

    /// <summary>
    /// Get the given option as a string.
    /// </summary>
    /// <param name="optionsMap">The options map.</param>
    /// <param name="option">The key of the option to parse as a string.</param>
    /// <returns>The option value parsed to a string.</returns>
    private string? GetOptionDataAsString(ImmutableDictionary<string, object> optionsMap, string option)
        => optionsMap.GetValueOrDefault(option, null).ToString();
}