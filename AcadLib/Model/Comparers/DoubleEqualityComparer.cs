﻿namespace AcadLib.Comparers
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    // Сравнение чисел
    [PublicAPI]
    [Obsolete]
    public class DoubleEqualityComparer : IEqualityComparer<double>
    {
        private readonly double threshold;

        public DoubleEqualityComparer(double threshold = 1)
        {
            this.threshold = threshold;
        }

        public bool Equals(double x, double y)
        {
            return Math.Abs(x - y) < threshold;
        }

        public int GetHashCode(double obj)
        {
            return 0;
        }
    }
}