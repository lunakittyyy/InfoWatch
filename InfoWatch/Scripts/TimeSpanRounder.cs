using System;
using System.Collections.Generic;
using System.Text;

namespace InfoWatch.Scripts
{
    internal class TimeSpanRounder
    {
        public struct RoundedTimeSpan
        {

            private const int TIMESPAN_SIZE = 7; // it always has seven digits

            private TimeSpan roundedTimeSpan;
            private int precision;

            public RoundedTimeSpan(long ticks, int precision)
            {
                if (precision < 0) { throw new ArgumentException("precision must be non-negative"); }
                this.precision = precision;
                int factor = (int)System.Math.Pow(10, (TIMESPAN_SIZE - precision));

                // This is only valid for rounding milliseconds-will *not* work on secs/mins/hrs!
                roundedTimeSpan = new TimeSpan(((long)System.Math.Round((1.0 * ticks / factor)) * factor));
            }

            public TimeSpan TimeSpan { get { return roundedTimeSpan; } }

            public override string ToString()
            {
                return ToString(precision);
            }

            public string ToString(int length)
            { // this method revised 2010.01.31
                int digitsToStrip = TIMESPAN_SIZE - length;
                string s = roundedTimeSpan.ToString();
                if (!s.Contains(".") && length == 0) { return s; }
                if (!s.Contains(".")) { s += "." + new string('0', TIMESPAN_SIZE); }
                int subLength = s.Length - digitsToStrip;
                return subLength < 0 ? "" : subLength > s.Length ? s : s.Substring(0, subLength);
            }
        }
    }
}
