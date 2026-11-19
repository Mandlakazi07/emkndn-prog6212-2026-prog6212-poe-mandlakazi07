// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        17 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Model validation in ASP.NET Core MVC and Razor Pages. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/mvc/models/validation [Accessed 17 November 2026].

using RaceDay.Web.Models.Api;

namespace RaceDay.Web.Models;

// Small helpers that turn a result into the text a runner expects to read
public static class ResultDisplay
{
    // 1 -> "1st", 2 -> "2nd", 3 -> "3rd", 11 -> "11th", 47 -> "47th"
    public static string Ordinal(int number)
    {
        var lastTwo = number % 100;
        if (lastTwo is >= 11 and <= 13)
        {
            return number + "th";
        }

        return (number % 10) switch
        {
            1 => number + "st",
            2 => number + "nd",
            3 => number + "rd",
            _ => number + "th"
        };
    }

    // "47th out of 1250 finishers"
    public static string PositionText(this ResultDto result) =>
        result.FinishPosition.HasValue
            ? $"{Ordinal(result.FinishPosition.Value)} out of {result.TotalFinishers} finishers"
            : "Not recorded";

    // "01:45:32"
    public static string TimeText(this ResultDto result) =>
        result.FinishTime.HasValue ? result.FinishTime.Value.ToString("hh\\:mm\\:ss") : "-";
}
