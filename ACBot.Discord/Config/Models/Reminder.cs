namespace ACBot.Discord.Config.Models;

public class Reminder : IConfigurationModel
{
    /// <summary>
    /// The ID of the text channel to which the reminder is to be sent.
    /// 0 = No Channel Set.
    /// </summary>
    public ulong ChannelId { get; set; } = 0L;

    /// <summary>
    /// The day at which the reminder is to be sent.
    /// </summary>
    public DayOfWeek Day { get; set; } = DayOfWeek.Wednesday;

    /// <summary>
    /// The hour at which the reminder is to be sent.
    /// </summary>
    public int Hour { get; set; } = 15;

    /// <summary>
    /// The minute at which the reminder is to be sent.
    /// </summary>
    public int Minute { get; set; } = 0;

    /// <summary>
    /// The message to be sent at the time of reminder.
    /// </summary>
    public string ReminderMessage { get; set; }
        = "Vergeet niet het communicatieboard bij te werken! @everyone";
}