// ***********************************************************************
// Assembly         : DarlInfAPI
// Author           : Andrew
// Created          : 08-17-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-26-2015
// ***********************************************************************
// <copyright file="DarlVarExtensions.cs" company="Dr Andy's IP Ltd (BVI)">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using DarlCommon;
using DarlLanguage.Processing;

namespace Dasl.TemporalDb
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
        public static Dictionary<string, Result> Convert(IEnumerable<DarlVar> values)
        {
            var res = new Dictionary<string, Result>();
            if (values != null)
            {
                foreach (var r in values)
                {
                    if (string.IsNullOrEmpty(r.name))
                        continue; //can't convert without name
                    var d = (Result.DataType)Enum.Parse(typeof(Result.DataType), r.dataType.ToString());
                    if (r.unknown)
                    {
                        res.Add(r.name, new Result(0.0, true));
                    }
                    else if (string.IsNullOrEmpty(r.Value))
                    {
                        switch (r.dataType)
                        {
                            case DarlVar.DataType.categorical:
                                if (r.categories == null || r.categories.Count == 0)
                                    res.Add(r.name, new Result(0.0, true));
                                else
                                    res.Add(r.name, new Result(r.categories, d));
                                break;
                            case DarlVar.DataType.numeric:
                                if (r.values == null || r.values.Count == 0)
                                    res.Add(r.name, new Result(0.0, true));
                                else
                                    res.Add(r.name, new Result(r.values, d));
                                break;
                            case DarlVar.DataType.sequence:
                                if (r.sequence == null || r.sequence.Count == 0)
                                    res.Add(r.name, new Result(0.0, true));
                                else
                                    res.Add(r.name, new Result(r.sequence, d));
                                break;
                            case DarlVar.DataType.textual:
                                res.Add(r.name, new Result(0.0, true));
                                break;
                        }
                    }
                    else
                    {
                        res.Add(r.name, new Result(r.Value, d));
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Converts the specified values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>List&lt;DarlVar&gt;.</returns>
        public static List<DarlVar> Convert(Dictionary<string, Result> values)
        {
            var res = new List<DarlVar>();
            if (values != null)
            {
                foreach (string key in values.Keys)
                {
                    var r = values[key];
                    List<double> vals = new List<double>();
                    if (r.values != null && r.dataType == Result.DataType.numeric)
                    {
                        foreach (var v in r.values)
                            vals.Add((double)v);
                    }
                    res.Add(new DarlVar { name = key, approximate = r.approximate, categories = r.categories, dataType = (DarlVar.DataType)Enum.Parse(typeof(DarlVar.DataType), r.dataType.ToString()), sequence = r.sequence, unknown = r.IsUnknown(), Value = r.Value == null ? "" : r.Value.ToString(), values = vals, weight = r.GetWeight() });
                }
            }
            return res;
        }

    }
}