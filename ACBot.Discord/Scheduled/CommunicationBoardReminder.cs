using ACBot.Discord.Config;
using ACBot.Discord.Config.Models;
using Discord.WebSocket;
using Quartz;

namespace ACBot.Discord.Scheduled;

public class CommunicationBoardReminder
{
    private readonly IScheduler _scheduler;

    private readonly DiscordSocketClient _client;

    private readonly Configuration _config;

    public CommunicationBoardReminder(IScheduler scheduler, DiscordSocketClient client, Configuration config)
    {
        _scheduler = scheduler;
        _client = client;
        _config = config;
    }

    /// <summary>
    /// Set-up the communication board reminder scheduled task.
    /// </summary>
    public void Execute()
    {
        var reminderModel = _config.ConfigurationMappings
                                .GetValueOrDefault("commboard_reminder") as Reminder
                            ?? new Reminder();

        var jobData = new JobDataMap();
        jobData.Put("discordClient", _client);
        jobData.Put("reminderConfigModel", reminderModel);

        var job = JobBuilder.Create<CommunicationBoardReminderJob>()
            .WithIdentity("CommunicationBoardReminder", "reminders")
            .UsingJobData(jobData)
            .StoreDurably()
            .Build();

        var trigger = GetCommunicationBoardReminderTrigger(
            reminderModel.Day,
            reminderModel.Hour,
            reminderModel.Minute
        );

        _scheduler.ScheduleJob(job, trigger);
    }

    /// <summary>
    /// Generate the communication board reminder trigger for the given day, hour and minute.
    /// </summary>
    /// <param name="day">The day of the week to send the reminder at.</param>
    /// <param name="hour">The hour of the day to send the reminder at.</param>
    /// <param name="minute">The minute of the day to send the reminder at.</param>
    /// <returns></returns>
    public static ITrigger GetCommunicationBoardReminderTrigger(DayOfWeek day, int hour, int minute) =>
        TriggerBuilder.Create()
            .WithIdentity("CommunicationBoardReminderTrigger", "reminders")
            .WithDailyTimeIntervalSchedule(x =>
                x.OnDaysOfTheWeek(day)
                    .InTimeZone(TimeZoneInfo.Local)
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(hour, minute))
                    .EndingDailyAfterCount(1)
            )
            .Build();

    /// <summary>
    /// The job to be executed when the communication board reminder trigger fires.
    /// </summary>
    private sealed class CommunicationBoardReminderJob : IJob
    {
        
        /// <summary>
        /// Job which sends the communication board reminder.
        /// </summary>
        /// <param name="context">The job context.</param>
        public async Task Execute(IJobExecutionContext context)
        {
            var discordClient = context.JobDetail.JobDataMap["discordClient"] as DiscordSocketClient;
            var reminderModel = context.JobDetail.JobDataMap["reminderConfigModel"] as Reminder;

            if (await discordClient.GetChannelAsync(reminderModel.ChannelId) is not SocketTextChannel channel)
                return;

            await channel.SendMessageAsync(reminderModel.ReminderMessage);
        }
    }
}