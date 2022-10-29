using Discord.WebSocket;
using Quartz;

namespace ACBot.Discord.Scheduled;

public class CommunicationBoardReminder
{
    private readonly IScheduler _scheduler;

    private readonly DiscordSocketClient _client;

    public CommunicationBoardReminder(IScheduler scheduler, DiscordSocketClient client)
    {
        _scheduler = scheduler;
        _client = client;
    }

    public void Execute()
    {
        var jobData = new JobDataMap();
        jobData.Put("discordClient", _client);

        var job = JobBuilder.Create<CommunicationBoardReminderJob>()
            .WithIdentity("CommunicationBoardReminder", "reminders")
            .UsingJobData(jobData)
            .StoreDurably()
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity("CommunicationBoardReminderTrigger", "reminders")
            .WithDailyTimeIntervalSchedule(x =>
                x.OnDaysOfTheWeek(DayOfWeek.Saturday)
                    .InTimeZone(TimeZoneInfo.Local)
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(14, 30))
            )
            .StartNow()
            .Build();

        _scheduler.ScheduleJob(job, trigger);
    }

    private sealed class CommunicationBoardReminderJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var discordClient = context.JobDetail.JobDataMap["discordClient"] as DiscordSocketClient;
            
            // TODO: replace with commands & small database
            var textChannelsToMessage = discordClient!.Guilds.SelectMany(
                x => x.Channels.OfType<SocketTextChannel>()
            );

            foreach (var textChannel in textChannelsToMessage)
                await textChannel.SendMessageAsync(
                    "Vergeet niet het communicatieboard bij te werken! @everyone"
                );
        }
    }
}