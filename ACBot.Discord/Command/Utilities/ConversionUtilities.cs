namespace ACBot.Discord.Command.Utilities;

public static class ConversionUtilities
{

    public static DayOfWeek? IntegerToDay(long day)
    {
        if (day is <= 0 or > 7)
            return null;

        return (DayOfWeek) day;
    }
    
}