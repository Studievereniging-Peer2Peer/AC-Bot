using ACBot.Discord.Config;
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

    public void Execute()
    {
        var jobData = new JobDataMap();
        jobData.Put("discordClient", _client);
        jobData.Put("config", _config);

        var job = JobBuilder.Create<CommunicationBoardReminderJob>()
            .WithIdentity("CommunicationBoardReminder", "reminders")
            .UsingJobData(jobData)
            .StoreDurably()
            .Build();

        var trigger =
            GetCommunicationBoardReminderTrigger(
                (DayOfWeek) (_config.ConfigurationMappings.GetValueOrDefault("commboard_reminder_dag",
                    DayOfWeek.Wednesday) ?? DayOfWeek.Wednesday),
                (int) (_config.ConfigurationMappings.GetValueOrDefault("commboard_reminder_uur", 15) ?? 15),
                (int) (_config.ConfigurationMappings.GetValueOrDefault("commboard_reminder_minuut") ?? 30)
            );

        _scheduler.ScheduleJob(job, trigger);
    }

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

    private sealed class CommunicationBoardReminderJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var discordClient = context.JobDetail.JobDataMap["discordClient"] as DiscordSocketClient;
            var config = context.JobDetail.JobDataMap["config"] as Configuration;

            var textChannelIdentification = (ulong) config.ConfigurationMappings
                .GetValueOrDefault("commboard_reminder_channel", -1);

            if (await discordClient.GetChannelAsync(textChannelIdentification) is not SocketTextChannel channel)
                return;

            await channel.SendMessageAsync(
                "Vergeet niet het communicatieboard bij te werken! @everyone"
            );
        }
    }
}