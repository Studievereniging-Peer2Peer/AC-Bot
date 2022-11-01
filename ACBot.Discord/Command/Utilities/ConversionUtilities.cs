namespace ACBot.Discord.Command.Utilities;

public static class ConversionUtilities
{

    /// <summary>
    /// Convert the given input into a DayOfWeek enum value by its index, if valid.
    /// </summary>
    /// <param name="day">The day index for the day of the week.</param>
    /// <returns>The DayOfWeek enum value if the day is a valid index, null otherwise.</returns>
    public static DayOfWeek? IntegerToDay(int day)
    {
        if (day is < 0 or > 6)
            return null;

        return (DayOfWeek) day;
    }
    
}