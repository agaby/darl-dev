/// </summary>

﻿using Darl.Common;
using Darl.Lineage.Bot;
using DarlCommon;
using DarlLanguage.Processing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dasl.TemporalDb
{
    /// A simple temporal database.
    /// </summary>
    public class EventList
    {
        protected List<DaslState> _events { get; set; } = new List<DaslState>();

        /// Gets or sets the events.
        /// </summary>
        /// <value>
        /// The events.
        /// </value>
        public List<DaslState> events { get { return _events; } set { _events = value; sorted = false; } }

        /// The sample time to use to create a simulation, analysis or prediction
        /// </summary>
        /// <remarks>Default is 1 second.</remarks>
        public TimeSpan sample { get; set; } = new TimeSpan(10000000); //1 second default

        protected int pos = 0;

        private static readonly string tstamptext = "timestamp";

        /// Gets the start of the stored series.
        /// </summary>
        /// <value>
        /// The start.
        /// </value>
        public DateTime start
        {
            get
            {
                Sort();
                if (_events.Count > 0)
                    return _events[0].timeStamp;
                return DateTime.MinValue;
            }
        }

        /// Gets the end of the stored series.
        /// </summary>
        /// <value>
        /// The end.
        /// </value>
        public DateTime end
        {
            get
            {
                Sort();
                if (_events.Count > 0)
                    return _events.Last().timeStamp;
                return DateTime.MaxValue;
            }
        }

        /// Gets or sets a value indicating whether this <see cref="EventList" /> is sorted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if sorted; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Set sorted to false if you add new data to events.</remarks>
        public bool sorted { get; set; } = false;

        protected void Sort()
        {
            if (!sorted)
            {
                _events.Sort(new DaslStateComparer());
                sorted = true;
            }
        }
        /// Samples the data at the interval chosen and returns the data in a format required by Dasl.
        /// </summary>
        /// <returns>A multivariate time series with equally spaced samples </returns>
        public List<List<DarlResult>> SampleData() //rewrite so that you can create n samples with empty data for sim.
        {
            var list = new List<List<DarlResult>>();
            if (start != DateTime.MinValue)
            {
                bool first = true;
                for (var t = start; t <= end; t += sample)
                {
                    if (first)
                    {
                        list.Add(DarlVarExtensions.Convert(GetValue(t)));
                    }
                    else
                    {
                        list.Add(DarlVarExtensions.Convert(GetNextValue(t)));
                    }
                }
            }
            return list;
        }

        public List<List<DarlResult>> GetEventData()
        {
            Sort();
            var list = new List<List<DarlResult>>();
            foreach (var e in _events)
            {
                e.values.Add(new DarlVar() { dataType = DarlVar.DataType.date, name = tstamptext, times = new List<DarlTime> { new DarlTime(e.timeStamp) } });
                list.Add(DarlVarExtensions.Convert(e.values));
            }
            return list;
        }

        public List<DaslState> ConvertToEvents(List<List<DarlResult>> samples)
        {
            var list = new List<DaslState>();
            int n = 0;
            foreach (var s in samples)
            {
                var sVal = s.FirstOrDefault(a => a.name == tstamptext);
                if (((object)sVal) != null && sVal.values.Any())
                {
                    list.Add(new DaslState { timeStamp = DarlVarExtensions.Convert(sVal).times[0].dateTime, values = DarlVarExtensions.Convert(s) });
                }
                else
                {
                    list.Add(new DaslState
                    {
                        timeStamp = start + new TimeSpan(sample.Ticks * n),
                        values = DarlVarExtensions.Convert(s)
                    });
                }
                n++;
            }
            return list;
        }

        /// finds the Value of the series at the time given,
        /// </summary>
        /// <remarks>random access</remarks>
        /// <param name="Value"></param>
        /// <param name="t"></param>
        /// <returns>false if empty or time is earlier than range held</returns>
        internal List<DarlVar> GetValue(DateTime t)
        {
            var Value = new List<DarlVar>();
            if (_events.Count == 0)
                return Value;
            else if (start > t)
                return Value; // t is earlier than the range of data.
            else if (end < t)// t is later than the range of data
            {
                if (_events.Last() != null)
                    Value = _events.Last().values;
                pos = _events.Count - 1;
            }
            else // Use BinarySearch
            {
                DaslState tempEvent = new DaslState { timeStamp = t };
                int index = _events.BinarySearch(tempEvent, new DaslStateComparer());
                if (index < 0)
                {
                    index = ~index;
                    index--;
                }
                Value = _events[index] != null ? _events[index].values : new List<DarlVar>();
                pos = index;
            }
            return Value;
        }
        /// finds the Value of the series at the time given,
        /// </summary>
        /// <remarks>Should be called after GetValue, starts search at previous location</remarks>
        /// <param name="Value">The required value by reference</param>
        /// <param name="t">Time to look for</param>
        /// <remarks>Must call GetValue First.</remarks>
        internal List<DarlVar> GetNextValue(DateTime t)
        {
            var Value = new List<DarlVar>();

            if (end < t)// t is later than the range of data
            {
                Value = _events[_events.Count - 1] != null ? _events[_events.Count - 1].values : new List<DarlVar>();
                pos = _events.Count - 1;
            }
            else // Use BinarySearch
            {
                var tempEvent = new DaslState { timeStamp = t };
                _events.BinarySearch(pos, _events.Count - pos, tempEvent, new DaslStateComparer());
                int index = _events.BinarySearch(tempEvent);
                if (index < 0)
                {
                    index = ~index;
                    index--;
                }
                Value = _events[index] != null ? _events[index].values : new List<DarlVar>();
                pos = index;
            }
            return Value;
        }
    }

    internal class DaslStateComparer : IComparer<DaslState>
    {
        public int Compare(DaslState x, DaslState y)
        {
            if (x == null)
                return 1;
            if (y == null)
                return -1;
            return x.timeStamp.CompareTo(y.timeStamp);
        }
    }
}
