using System.Text;

namespace AudioConverter;

internal static class TimeSpanExtensions
{
    public static string ToDurationString(this TimeSpan duration)
    {
        var sb = new StringBuilder();

        if (duration.Hours > 0)
        {
            sb.Append(duration.Hours).Append(" hour");

            if (duration.Hours > 1)
            {
                sb.Append('s');
            }
        }

        if (duration.Minutes > 0)
        {
            if (sb.Length > 0)
            {
                sb.Append(", ");
            }

            sb.Append(duration.Minutes).Append(" minute");

            if (duration.Minutes > 1)
            {
                sb.Append('s');
            }
        }

        if (duration.Seconds > 0)
        {
            if (sb.Length > 0)
            {
                sb.Append(" and ");
            }

            sb.Append(duration.Seconds).Append(" second");

            if (duration.Seconds > 1)
            {
                sb.Append('s');
            }
        }

        return sb.ToString();
    }
}