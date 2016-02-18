using System;
using System.Collections.Generic;
using System.Linq;

namespace ABSoft.Photobookmart.FTPSync.Helper
{
    public static class StringExtensions
    {
        public static DateTime Spiral_ToTime(this string s)
        {
            double minutes = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes;
            return DateTime.Parse(s).Subtract(TimeSpan.FromMinutes(minutes));
        }

        public static DateTime Spiral_ToDate(this string s)
        {
            DateTime dt = s.Spiral_ToTime();
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0, 0);
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime Spiral_ToDate(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0, 0);
        }
    }

    public static class ObjectExtensions
    {
        public static object Spiral_GetProperty(this object obj, string name)
        {
            return obj != null ? obj.GetType().GetProperty(name).GetValue(obj, null) : null;
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }
    }
}
