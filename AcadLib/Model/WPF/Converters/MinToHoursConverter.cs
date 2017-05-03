﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace AcadLib.WPF.Converters
{
    [ValueConversion(typeof(int), typeof(double))]
    [ValueConversion(typeof(double), typeof(double))]
    public class MinToHoursConverter : ConvertorBase
    {
        public override object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            var min = System.Convert.ToInt32(value);
            var hours = Math.Round(min / 60d, 1);
            return hours;
        }
    }
}
