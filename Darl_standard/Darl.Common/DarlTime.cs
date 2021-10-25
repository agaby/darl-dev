using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace Darl.Common
{
    [ProtoContract(AsReferenceDefault = true)]
    public class DarlTime : IComparable<DarlTime>
    {
        public static readonly DateTime yearZero = new DateTime(1, 1, 1, 0, 0, 0);

        public static readonly double secondsPerYear = 31556952.0;

        public static readonly DarlTime MaxValue = new DarlTime(DateTime.MaxValue);

        public static readonly DarlTime MinValue = new DarlTime(-MaxValue.raw);

        public static DarlTime UtcNow { get { return new DarlTime(DateTime.UtcNow); } }

        public DarlTime()
        {
            raw = 0.0;
        }

        public DarlTime(DateTime dt)
        {
            dateTime = dt;
        }

        public DarlTime(double d)
        {
            raw = d;
        }

        public DarlTime(int year, int? month = null, int? day = null, int? hour = null, int? minute = null, double? second = null)
        {
            if(year < 1)
            {
                raw = (year - 1) * secondsPerYear;
                precision = secondsPerYear;
            }
            else
            {
                if(month == null)
                {
                    precision = secondsPerYear;
                    dateTime = new DateTime(year, 0, 0);

                }
                else if(day == null)
                {
                    precision = secondsPerYear /12.0;
                    dateTime = new DateTime(year, month ?? 0, 0);
                }
                else if (hour == null)
                {
                    precision = 86400;
                    dateTime = new DateTime(year, month ?? 0, day ?? 0);
                }
                else if (minute == null)
                {
                    precision = 3600;
                    dateTime = new DateTime(year, month ?? 0, day ?? 0, hour ?? 0, 0, 0);
                }
                else if (second == null)
                {
                    precision = 60;
                    dateTime = new DateTime(year, month ?? 0, day ?? 0, hour ?? 0, minute ?? 0, 0);
                }
                else
                {
                    precision = 0;
                    dateTime = new DateTime(year, month ?? 0, day ?? 0, hour ?? 0, minute ?? 0, (int)Math.Truncate(second ?? 0.0), (int)((second ?? 0.0) - Math.Truncate(second ?? 0.0)) * 1000);
                }
            }
        }

        public DarlTime(int year, int? season)
        {
            if (year < 1) //BC dates and year zero
            {
                raw = (year - 1 + ((season ?? 0) / 4.0)) * secondsPerYear;
                if (season != null)
                {
                    precision = secondsPerYear / 4.0;
                }
                else
                {
                    precision = secondsPerYear;
                }
            }
            else //can be represented in a DateTime
            {
                if(season == null)
                {
                    precision = secondsPerYear;
                    dateTime = new DateTime(year, 0, 0);
                }
                else
                {
                    precision = secondsPerYear / 4.0;
                    switch(season ?? 0)
                    {
                        case 0:
                            dateTime = new DateTime(year, 12, 21);
                            break;
                        case 1:
                            dateTime = new DateTime(year, 3, 20);
                            break;
                        case 2:
                            dateTime = new DateTime(year, 6, 21);
                            break;
                        case 3:
                            dateTime = new DateTime(year, 9, 21);
                            break;
                    }
                }
            }
        }

        [ProtoMember(1)] 
        public double raw { get; set; }
        [ProtoMember(2)] 
        public double precision { get; set; }

        public DateTime dateTime 
        { 
            get {
                return raw > 0.0 ? yearZero + TimeSpan.FromSeconds((double)this.raw) : DateTime.MinValue;
            }
            set
            {
                raw = (double)(value - yearZero).TotalSeconds;
            }
        }


        public DateTimeOffset dateTimeOffset
        {
            get
            {
                return raw > 0.0 ? yearZero + TimeSpan.FromSeconds((double)this.raw) : DateTimeOffset.MinValue;
            }
            set
            {
                raw = (double)(value - yearZero).TotalSeconds;
            }
        }

        /// <summary>
        /// Get the year -ve = bc
        /// </summary>
        public int year
        {
            get
            {
                return (int)Math.Floor(raw / secondsPerYear);
            }
        }

        /// <summary>
        /// get the season. Winter = 0;
        /// </summary>
        public int season
        {
            get
            {
                return (int)(Math.Floor(raw % secondsPerYear) / (secondsPerYear / 4));
            }
        }

        //
        // Summary:
        //     Converts the specified string representation of a date and time to its System.DateTime
        //     equivalent and returns a value that indicates whether the conversion succeeded.
        //
        // Parameters:
        //   s:
        //     A string containing a date and time to convert.
        //
        //   result:
        //     When this method returns, contains the System.DateTime value equivalent to the
        //     date and time contained in s, if the conversion succeeded, or System.DateTime.MinValue
        //     if the conversion failed. The conversion fails if the s parameter is null, is
        //     an empty string (""), or does not contain a valid string representation of a
        //     date and time. This parameter is passed uninitialized.
        //
        // Returns:
        //     true if the s parameter was converted successfully; otherwise, false.
        public static bool TryParse(string s, out DarlTime? result)
        {
            //add code to parse "summer 46 BC" or "winter 37 AD" or just "56 BC"
            if(!DateTime.TryParse(s, out DateTime dt ))
            {
                result = null;
                return false;
            }
            result = new DarlTime(dt);
            return true;
        }

        public int CompareTo(DarlTime other)
        {
            return raw.CompareTo(other.raw);
        }

        public static DarlTime Parse(string d)
        {
            return new DarlTime(DateTime.Parse(d));
        }

        public override string ToString()
        {
            if(raw >= 0)
            {
                return dateTime.ToString();
            }
            else
            {
                if (raw > -secondsPerYear)
                    return "0 AD";
                return $"{Math.Abs(Math.Truncate(raw / secondsPerYear))} BC";
            }
        }

        public override bool Equals(object obj)
        {
            if( obj is DarlTime)
            {
                var dt = obj as DarlTime;
                return dt.raw == raw;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return raw.GetHashCode();
        }

    }
}
