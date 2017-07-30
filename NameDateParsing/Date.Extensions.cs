
namespace NameDateParsing
{
    public static class DateExtensions
    {
        public static bool HasYear(this Date dt)
        {
            return (dt.Modifier & DateModifier.YearMissing) == 0;
        }

        public static bool HasMonth(this Date dt)
        {
            return (dt.Modifier & DateModifier.MonthMissing) == 0;
        }

        public static bool HasDay(this Date dt)
        {
            return (dt.Modifier & DateModifier.DayMissing) == 0;
        }
    }
}
