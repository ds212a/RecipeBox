﻿using System;
using Windows.UI.Xaml.Data;

namespace RecipeBox.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TimeSpan span = (TimeSpan)value;
            if (span == TimeSpan.MinValue)
            {
                return "00:00";
            }

            string formatted = string.Format("{0}:{1}",
                span.Duration().Hours > 0 ? string.Format("{0,2:D2}", span.Hours) : "00",
                span.Duration().Minutes > 0 ? string.Format("{0,2:D2}", span.Minutes) : "00");

            if (formatted.EndsWith(", "))
                formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted))
                formatted = "00:00";

            return formatted;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string strValue = value as string;
            TimeSpan resultSpan;

            if (TimeSpan.TryParse(strValue, out resultSpan))
            {
                return resultSpan;
            }
            else if (strValue == "__:__")
            {
                return strValue;
            }

            return new TimeSpan();
        }
    }
}
