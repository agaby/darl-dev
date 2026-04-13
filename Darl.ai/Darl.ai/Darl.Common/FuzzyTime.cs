/// <summary>
/// FuzzyTime.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System;
using System.Collections.Generic;

namespace Darl.Common
{
    public class FuzzyTime
    {
        public FuzzyTime(DarlTime time)
        {
            SetEvent(time);
        }

        public FuzzyTime(DarlTime time1, DarlTime time2)
        {
            SetInterval(time1, time2);
        }

        public FuzzyTime(DarlTime time1, DarlTime time2, DarlTime time3)
        {
            SetFuzzyEvent(time1, time2, time3);
        }

        public FuzzyTime(DarlTime time1, DarlTime time2, DarlTime time3, DarlTime time4)
        {
            SetFuzzyInterval(time1, time2, time3, time4);
        }

        public List<DarlTime> darlTimes { get; set; } = new List<DarlTime>();
        public void SetEvent(DarlTime time)
        {
            darlTimes.Clear();
            darlTimes.Add(time);
        }

        public void SetInterval(DarlTime time1, DarlTime time2)
        {
            darlTimes.Clear();
            darlTimes.Add(time1);
            darlTimes.Add(time2);
            darlTimes.Sort();
        }

        public void SetFuzzyEvent(DarlTime time1, DarlTime time2, DarlTime time3)
        {
            darlTimes.Clear();
            darlTimes.Add(time1);
            darlTimes.Add(time2);
            darlTimes.Add(time3);
            darlTimes.Sort();
        }

        public void SetFuzzyInterval(DarlTime time1, DarlTime time2, DarlTime time3, DarlTime time4)
        {
            darlTimes.Clear();
            darlTimes.Add(time1);
            darlTimes.Add(time2);
            darlTimes.Add(time3);
            darlTimes.Add(time4);
            darlTimes.Sort();
        }

        public override string ToString()
        {
            return String.Join(',', darlTimes);
        }

    }
}
