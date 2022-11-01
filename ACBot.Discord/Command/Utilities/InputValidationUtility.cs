using System.Collections.Immutable;

namespace ACBot.Discord.Command.Utilities;

public static class InputValidationUtility
{

    /// <summary>
    /// Verify whether the options given in the requiredOptions parameter exist in the options mapping.
    /// </summary>
    /// <param name="options">The options mapping.</param>
    /// <param name="requiredOptions">The options which are required.</param>
    /// <returns>True if all required options are present, false otherwise.</returns>
    public static bool RequiredOptionsExist(ImmutableDictionary<string, object> options, string[] requiredOptions)
        => !options.IsEmpty && requiredOptions.All(options.ContainsKey);
    
    /// <summary>
    /// Verify whether the input is a valid day of the week.
    /// </summary>
    /// <param name="input">The input string</param>
    /// <param name="day">The output, null if the input is not a valid DayOfWeek index. DayOfWeek value otherwise.</param>
    /// <returns>True when the stringified number is a valid DayOfWeek index, false otherwise.</returns>
    public static bool IsValidDay(string? input, out DayOfWeek? day)
    {
        day = null;
        
        if (!int.TryParse(input, out int numifiedDay))
            return false;

        day = ConversionUtilities.IntegerToDay(numifiedDay);

        return day != null;
    }
    
    /// <summary>
    /// Verify whether the input is a valid hour in a day.
    /// </summary>
    /// <param name="input">The input string</param>
    /// <param name="hour">The output, null if it is not a valid hour in the day.</param>
    /// <returns>True when the hour is a valid one in the day, false otherwise.</returns>
    public static bool IsValidHourInDay(string? input, out int? hour)
    {
        hour = null;

        if (!int.TryParse(input, out int numifiedHour))
            return false;

        if (numifiedHour is < 0 or > 23)
            return false;

        hour = numifiedHour;
        
        return true;
    }
    
    /// <summary>
    /// Verify whether the input is a valid minute.
    /// </summary>
    /// <param name="input">The input string</param>
    /// <param name="minute">The output, null if it is not a valid minute in the hour</param>
    /// <returns>True when the minute is a valid one in the hour, false otherwise.</returns>
    public static bool IsValidMinuteInHour(string? input, out int? minute)
    {
        minute = null;

        if (!int.TryParse(input, out int numifiedMinute))
            return false;

        if (numifiedMinute is < 0 or > 59)
            return false;

        minute = numifiedMinute;

        return true;
    }

}