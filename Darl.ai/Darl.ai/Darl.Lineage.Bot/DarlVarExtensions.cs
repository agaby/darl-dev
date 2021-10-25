using Darl.Common;
using DarlCommon;
using DarlLanguage.Processing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Darl.Lineage.Bot
{
    /// <summary>
    /// Class DarlVarExtensions.
    /// </summary>
    public static class DarlVarExtensions
    {
        /// <summary>
        /// Converts the specified values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>Dictionary&lt;System.String, Result&gt;.</returns>
        public static List<DarlResult> Convert(IEnumerable<DarlVar> values)
        {
            var res = new List<DarlResult>();
            if (values != null)
            {
                foreach (var r in values)
                {
                    if (string.IsNullOrEmpty(r.name))
                        continue; //can't convert without name
                    res.Add(Convert(r));
                }
            }
            return res;
        }

        public static DarlResult Convert(DarlVar val)
        {
            var d = Convert(val.dataType);
            if (val.unknown)
            {
                return new DarlResult(val.name, 0.0, true);
            }
            else if (string.IsNullOrEmpty(val.Value))
            {
                switch (val.dataType)
                {
                    case DarlVar.DataType.categorical:
                        if (val.categories == null || val.categories.Count == 0)
                            return new DarlResult(val.name, 0.0, true);
                        else
                            return new DarlResult(val.name, val.categories, d);
                    case DarlVar.DataType.numeric:
                        if (val.values == null || val.values.Count == 0)
                            return new DarlResult(val.name, 0.0, true);
                        else
                            return new DarlResult(val.name, val.values, d);
                    case DarlVar.DataType.sequence:
                        if (val.sequence == null || val.sequence.Count == 0)
                            return new DarlResult(val.name, 0.0, true);
                        else
                            return new DarlResult(val.name, val.sequence, d);
                    case DarlVar.DataType.textual:
                        return new DarlResult(0.0, true);
                    case DarlVar.DataType.date:
                        if (val.times == null || val.times.Count == 0)
                            return new DarlResult(val.name, 0.0, true);
                        else
                            return new DarlResult(val.name, val.times, DarlResult.DataType.temporal);
                    case DarlVar.DataType.duration:
                        if (val.times == null || val.times.Count == 0)
                            return new DarlResult(0.0, true);
                        else
                            return new DarlResult(val.name, val.times, DarlResult.DataType.duration);
                }
            }
            return new DarlResult(val.name, val.Value, d);
        }

        /// <summary>
        /// Converts the specified values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>List&lt;DarlVar&gt;.</returns>
        public static List<DarlVar> Convert(List<DarlResult> values)
        {
            var res = new List<DarlVar>();
            if (values != null)
            {
                foreach (var r in values)
                {
                    res.Add(Convert(r));
                }
            }
            return res;
        }

        public static DarlVar Convert(DarlResult r)
        {
            //ensure Value holds some value.
            List<double> vals = new List<double>();
            List<DarlTime> times = new List<DarlTime>();
            if (r.values != null)
            { 
                if(r.dataType == DarlResult.DataType.numeric)
                {
                    foreach (var v in r.values)
                        vals.Add((double)v);
                }
                else if(r.dataType == DarlResult.DataType.temporal)
                {
                    foreach (var v in r.values)
                        times.Add(new DarlTime((double)v));
                }
            }
            string dvValue = "";
            if (r.Value == null && !r.IsUnknown())
            {
                switch (r.dataType)
                {
                    case DarlResult.DataType.categorical:
                        if (r.categories.Count > 0)
                        {
                            dvValue = r.categories.Aggregate((l, p) => l.Value > p.Value ? l : p).Key;
                        }
                        break;
                    case DarlResult.DataType.numeric:
                        if (r.values.Count > 0)
                        {
                            if (r.values.Count == 1)
                                dvValue = r.values[0].ToString();
                            else
                            {
                                if (r.cuts != null && r.cuts[0] != null)
                                {
                                    dvValue = r.CofG().ToString();
                                }
                            }
                        }
                        break;
                    case DarlResult.DataType.textual:
                        dvValue = r.stringConstant;
                        break;
                    case DarlResult.DataType.temporal:
                        if(times.Any())
                        {
                            dvValue = times[0].dateTime.ToShortDateString();
                        }
                        break;
                }
            }
            else if(!r.IsUnknown())
            {
                dvValue = r.Value.ToString();
            }
            return new DarlVar { name = r.name, approximate = r.approximate, categories = r.categories, dataType = Convert(r.dataType), sequence = r.sequence, unknown = r.IsUnknown(), Value = dvValue, values = vals, weight = r.GetWeight(), times = times };
        }

        private static DarlResult.DataType Convert(DarlVar.DataType d)
        {
            switch(d)
            {
                case DarlVar.DataType.categorical:
                    return DarlResult.DataType.categorical;
                case DarlVar.DataType.numeric:
                    return DarlResult.DataType.numeric;
                case DarlVar.DataType.textual:
                    return DarlResult.DataType.textual;
                case DarlVar.DataType.sequence:
                    return DarlResult.DataType.sequence;
                case DarlVar.DataType.duration:
                    return DarlResult.DataType.duration;
                case DarlVar.DataType.date:
                case DarlVar.DataType.time:
                    return DarlResult.DataType.temporal;
                default:
                    return DarlResult.DataType.textual;
            }
        }

        private static DarlVar.DataType Convert(DarlResult.DataType d)
        {
            switch(d)
            {
                case DarlResult.DataType.categorical:
                    return DarlVar.DataType.categorical;
                case DarlResult.DataType.numeric:
                    return DarlVar.DataType.numeric;
                case DarlResult.DataType.duration:
                    return DarlVar.DataType.duration;
                case DarlResult.DataType.sequence:
                    return DarlVar.DataType.sequence;
                case DarlResult.DataType.temporal:
                    return DarlVar.DataType.date;
                default:
                    return DarlVar.DataType.textual;
            }
        }

    }
}
