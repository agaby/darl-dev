using Darl.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Darl.Thinkbase.Meta
{
    /// <summary>
    /// Darl Form's general purpose representation for numbers and constants containing uncertainty.
    /// </summary>
    /// <remarks>
    /// Results encompass both an unknown state, a degree of truth, singletons and a variety of representations for fuzzy numbers.
    /// Implementation of fuzzy arithmetic follows: 
    /// <para>
    /// Introduction to Fuzzy Arithmetic,  
    /// Arnold Kaufmann, Madan M Gupta, 1991 Van Nost. Reinhold, ISBN 0442008996
    /// </para>
    /// <para>and</para>
    /// <para>Implementing Fuzzy Arithmetic, A.M. Anile, S, Deodata, G. Privitera,
    /// Fuzzy Sets and Systems 72 (1995) 239 - 250</para>
    /// </remarks>
    [Serializable]
    public class DarlResult : IComparable
    {

        /// <summary>
        /// Indicates the type of fuzzy number contained in this result
        /// </summary>
        public enum Fuzzyness
        {
            /// <summary>
            /// This result is unknown
            /// </summary>
            unknown,
            /// <summary>
            /// This result is a single crisp value
            /// </summary>
            singleton,
            /// <summary>
            /// This result is an interval, two values.
            /// </summary>
            interval,
            /// <summary>
            /// This result is a triangular fuzzy set
            /// </summary>
            triangle,
            /// <summary>
            /// This result is a trapezoidal fuzzy set defined by 4 values.
            /// </summary>
            trapezoid
        };

        /// <summary>
        /// The type of data stored in the Result
        /// </summary>
        public enum DataType
        {
            /// <summary>
            /// Numeric including fuzzy
            /// </summary>
            numeric,
            /// <summary>
            /// One or more categories with confidences
            /// </summary>
            categorical,
            /// <summary>
            /// Textual
            /// </summary>
            textual,
            /// <summary>
            /// a text sequence
            /// </summary>
            sequence,
            /// <summary>
            /// A time value
            /// </summary>
            temporal,
            /// <summary>
            /// A time period
            /// </summary>
            duration,
            /// <summary>
            /// a goal seek specification
            /// </summary>
            seek,
            /// <summary>
            /// a possibility discovery specification
            /// </summary>
            discover,
            /// <summary>
            /// a network action such as seek or discover
            /// </summary>
            network
        }

        /// <summary>
        /// Initializes a Result
        /// </summary>
        public DarlResult()
        {
            numeric = true;
            unknown = false;
            weight = 1.0;
            values = new List<object>();
            categories = new Dictionary<string, double>();
            approximate = false;
            leftUnbounded = false;
            rightUnbounded = false;
            dataType = DataType.numeric;
        }
        /// <summary>
        /// Is numeric if true.
        /// </summary>
        private bool numeric
        {
            get
            {
                return dataType == DataType.numeric || dataType == DataType.temporal || dataType == DataType.duration;
            }
            set
            {
                if (value)
                    dataType = DataType.numeric;
                else
                    dataType = DataType.categorical;
            }
        }

        /// <summary>
        /// if temporal is true
        /// </summary>
        private bool temporal
        {
            get { return dataType == DataType.temporal || dataType == DataType.duration; }
        }

        /// <summary>
        ///  This result is unknown if true.
        /// </summary>
        private bool unknown;
        /// <summary>
        /// The confidence placed in this result
        /// </summary>
        private double weight;

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        public DataType dataType { get; set; }

        /// <summary>
        /// The array containing the up to 4 values representing the fuzzy number.
        /// </summary>
        /// <remarks>Since all fuzzy numbers used by DARL are convex, i,e. their envelope doesn't have any in-folding 
        /// sections, the user can specify numbers with a simple sequence of doubles.
        /// So 1 double represents a crisp or singleton value.
        ///    2 doubles represent an interval,
        ///    3 a triangular fuzzy set,
        ///    4 a trapezoidal fuzzy set.
        /// The values must be ordered in ascending order, but it is permissible for two or more to hold the same value.</remarks>
        public List<object> values = new List<object>();

        /// <summary>
        /// Gets or sets the sequence.
        /// </summary>
        /// <value>
        /// The sequence.
        /// </value>
        public List<List<string>> sequence { get; set; }

        /// <summary>
        /// Single central or most confident value, expressed as a string or double.
        /// </summary>
        public object Value { get; set; }

        public string name { get; set; }

        /// <summary>
        /// Hash-table with list of categories, each indexed against a truth value.
        /// </summary>
        public Dictionary<string, double> categories = new Dictionary<string, double>();

        /// <summary>
        /// Array of 11 intervals representing horizontal cuts across 
        /// the truth diagram at 0,0.1,0,2...1.0 truth levels.
        /// </summary>
        /// <remarks>This array simplifies fuzzy arithmetic. Whereas addition and subtraction
        /// can be done on the coordinates of the fuzzy numbers, multiplication, division and 
        /// power between two fuzzy numbers can create results that are no longer strictly triangles
        /// or trapezoids. This approach gives us an approximation to the true shape. It is very
        /// unlikely that the 1-3% error inherent in interpolating the results will matter in a practical application.</remarks>
        public Interval[] cuts;
        /// <summary>
        /// String constant used in string processing
        /// </summary>
        public string stringConstant = string.Empty;
        /// <summary>
        /// Processes the results of an aggregation
        /// </summary>
        /// <param name="output">The output to aggregate rule truth values for.</param>
        internal void Simplify(IOSequenceDefinitionNode output, DarlResult? other = null)// new signature to simplify things
        {
            if (!numeric)
            {
                output.confidence = 0.0;
                output.Value = null;
                if (dataType == DataType.categorical)
                {
                    foreach (DarlResult result in values)
                    {
                        foreach (string catname in result.categories.Keys)
                        {//normally only one
                            if (!categories.ContainsKey(catname))
                                categories.Add(catname, result.weight);
                            categories[catname] = Math.Max(result.weight, (double)categories[catname]);
                            if (result.weight > output.confidence)
                            {
                                output.confidence = result.weight;
                                output.Value = catname;
                                this.Value = catname;
                                this.weight = result.weight;
                            }
                        }
                    }
                }
                else if (dataType == DataType.textual)
                {
                    if (values.Count > 0)
                    {
                        var list = new List<string>();
                        var maxConfidence = 0.0;
                        output.confidence = 1.0;
                        foreach (DarlResult r in values)
                        {
                            //convert type of r if necessary
                            var res = r.Convert(DataType.textual);
                            if (res.weight > maxConfidence)
                            {
                                maxConfidence = res.weight;
                                list.Clear();
                                list.Add(res.stringConstant);
                            }
                            else if (weight == maxConfidence)
                            {
                                list.Add(res.stringConstant);
                            }
                            output.confidence = Math.Min(output.confidence, res.weight);
                        }
                        stringConstant = string.Empty;
                        for (int n = 0; n < list.Count; n++)
                        {
                            if (list[n].Any())
                                stringConstant += list[n] + (n < list.Count - 1 && !Char.IsWhiteSpace(list[n].Last<char>()) ? "\n" : "");
                        }
                        Value = stringConstant;
                        output.Value = stringConstant;

                    }
                }
                else if (dataType == DataType.network) //we've got a network output, but we don't know if it's seeking or discovering
                {
                    if (values.Any())
                    {
                        var embeddedRes = values[0] as DarlResult;
                        var outp = output as OutputDefinitionNode;
                        var seq = new List<List<string>>();
                        seq.Add(new List<string> { outp.nodeId });
                        seq.Add(embeddedRes.sequence[0]);
                        seq.Add(new List<string> { outp.lineage });
                        sequence = seq;
                        this.dataType = embeddedRes.dataType; //set seek or discover
                    }
                }
            }
            else
            {
                if (weight > 0.0)
                {
                    for (int level = 0; level < cutCount; level++)
                    {//for each cut
                        int count = 0;
                        double cutLevel = level * 0.1;
                        foreach (DarlResult result in values)
                        {
                            double scale = result.weight / weight; // 1.0 for largest
                            if (count == 0)
                                cuts[level] = result.IntervalAtConfidence(cutLevel / scale);
                            else
                                cuts[level] = Interval.Conjunction(cuts[level], result.IntervalAtConfidence(cutLevel / scale));
                            count++;
                        }
                    }
                    Normalise(true);
                    //			values[1] = CofG();
                    output.Value = CofG();
                    this.Value = output.Value;
                }
                output.confidence = weight;
            }
            if (weight == 0.0)
            {
                if (((object)other) != null && other.weight > 0.0)//otherwise rule exists and has a valid result
                {
                    other.Simplify(output);
                    this.Value = other.Value;
                    this.weight = Value != null ? other.weight : 0.0;
                    this.values = other.values;
                    this.categories = other.categories;
                    this.stringConstant = other.stringConstant;
                    this.Normalise(false);
                    return;
                }
                else
                    unknown = true;
            }
            output.fuzzyResults = this;
        }

        /// <summary>
        /// Processes the results of an aggregation with type derived from the aggregation
        /// </summary>
        /// <param name="output">The output to aggregate rule truth values for.</param>
        /// <param name="other">The alternate output to aggregate rule truth values for if output is unknown.</param>
        internal void PolymorphicSimplify(IOSequenceDefinitionNode output, DarlResult? other = null)
        {
            foreach (DarlResult r in values)
            {
                dataType = r.dataType;
            }
            bool outputSet = false;
            switch (dataType)
            {
                case DataType.categorical:
                    foreach (DarlResult result in values)
                    {
                        foreach (string catname in result.categories.Keys)
                        {//normally only one
                            if (!categories.ContainsKey(catname))
                                categories.Add(catname, result.weight);
                            categories[catname] = Math.Max(result.weight, (double)categories[catname]);
                            if (result.weight > output.confidence)
                            {
                                var outRes = new DarlResult(output.name, catname)
                                {
                                    weight = weight,
                                    stringConstant = catname,
                                    Value = catname
                                };
                                output.confidence = result.weight;
                                this.Value = catname;
                                this.weight = result.weight;
                                outputSet = true;
                                output.Value = outRes;
                            }
                        }
                    }
                    break;
                case DataType.numeric:
                case DataType.temporal:
                    if (weight > 0.0)
                    {
                        for (int level = 0; level < cutCount; level++)
                        {//for each cut
                            int count = 0;
                            double cutLevel = level * 0.1;
                            foreach (DarlResult result in values)
                            {
                                double scale = result.weight / weight; // 1.0 for largest
                                if (count == 0)
                                    cuts[level] = result.IntervalAtConfidence(cutLevel / scale);
                                else
                                    cuts[level] = Interval.Conjunction(cuts[level], result.IntervalAtConfidence(cutLevel / scale));
                                count++;
                            }
                        }
                        Normalise(true);
                        //			values[1] = CofG();
                        this.Value = CofG();
                        var outRes = new DarlResult((double)this.Value)
                        {
                            weight = weight
                        };
                        output.confidence = weight;
                        output.Value = outRes;
                        outputSet = true;
                    }
                    output.confidence = weight;
                    break;
                case DataType.textual:
                    if (values.Count > 0)
                    {
                        var list = new List<string>();
                        var maxConfidence = 0.0;
                        foreach (DarlResult r in values)
                        {
                            var res = r.Convert(DataType.textual);
                            if (res.weight > maxConfidence)
                            {
                                maxConfidence = res.weight;
                                list.Clear();
                                list.Add(res.stringConstant);
                            }
                            else if (weight == maxConfidence)
                            {
                                list.Add(res.stringConstant);
                            }
                        }
                        stringConstant = string.Empty;
                        for (int n = 0; n < list.Count; n++)
                            stringConstant += list[n] + (n < list.Count - 1 && !Char.IsWhiteSpace(list[n].Last<char>()) ? "\n" : "");
                        Value = stringConstant;
                        var outRes = new DarlResult(output.name, stringConstant, DataType.textual);
                        output.confidence = maxConfidence;
                        outRes.weight = maxConfidence;
                        output.Value = outRes;
                        outputSet = true;
                    }
                    break;
            }
            if (weight == 0.0)
            {
                if (((object)other) != null && other.weight > 0.0)//otherwise rule exists and has a valid result
                {
                    other.Simplify(output);
                    this.Value = other.Value;
                    this.weight = Value != null ? other.weight : 0.0;
                    this.values = other.values;
                    this.categories = other.categories;
                    outputSet = true;
                    return;
                }
                else
                    unknown = true;
            }
            if (!outputSet)
            {
                output.confidence = 0.0;
                output.Value = new DarlResult(0.0, true); //was null 07/06/17
            }
            output.fuzzyResults = this;
        }


        internal void FuzzyAggregate(DarlResult result)
        {
            if (result.weight > 0.0 && !result.IsUnknown())
            {
                if (values.Count == 0)
                    weight = result.weight;
                else
                    weight = Math.Max(result.weight, weight);
                values.Add(result);
            }
        }
        /// <summary>
        /// Sets the weight of this Result
        /// </summary>
        /// <param name="dWeight">The weight 0-1</param>
        public void SetWeight(double dWeight)
        {
            weight = dWeight;
        }
        /// <summary>
        /// Returns the weight of this Result
        /// </summary>
        /// <returns>Weight 0-1</returns>
        public double GetWeight()
        {
            return weight;
        }
        /// <summary>
        /// Compares this result with another and returns a result corresponding to their
        /// mutual features.
        /// </summary>
        /// <param name="result">The Result object to compare to</param>
        /// <returns>The result of the comparison</returns>
        public DarlResult Equal(DarlResult result)
        {
            if (Equals(result))
                return new DarlResult(Math.Min(this.weight, result.weight), false);//XMIN-257
            if (unknown || result.unknown)
                return new DarlResult(-1.0, true);
            if (!numeric || !result.numeric)
            {
                if (numeric == result.numeric)
                {
                    //categorical comparison
                    // The returned value is the intersection of the two sets of alternatives,
                    // each item in the new list being given the lower of the two weights.
                    // order of comparison is arbitrary, i.e, this versus result
                    DarlResult cat = new DarlResult(false, 1.0);
                    foreach (string name in categories.Keys)
                    {
                        if (result.categories.ContainsKey(name))
                            cat.categories.Add(name, Math.Min((double)categories[name], (double)result.categories[name]));
                    }
                    if (cat.categories.Count == 0)
                        cat.weight = 0.0;
                    return cat;
                }
                else
                    return new DarlResult(-1.0, true);
            }
            // now numeric
            double gap = 0.0;
            int n;
            for (n = 0; n < cutCount && Interval.Overlap(cuts[n], result.cuts[n], ref gap); n++) ;
            if (n == 0)
                return new DarlResult(0.0, false);
            if (n == DarlResult.cutCount)
                return new DarlResult(1.0, false);
            //otherwise interpolate to find the truth value where they crossed
            //note that for "linear" sets the result will be exact, for quantized non-linear sets a small error will be created.
            //(Although we only use linear sets, non-linear ones can be generated by fuzzy arithmetic action on linear sets.)
            //The sets overlapped at n-1, and ceased to overlap at n.
            //The truth value therefore lies between (n-1)/(cutCount -1) and n/(cutCount - 1);
            double overlap = 0.0;
            Interval.Overlap(cuts[n - 1], result.cuts[n - 1], ref overlap);
            double ratio = overlap / gap;
            DarlResult res = new DarlResult((n - 1) * 0.1 + ratio * 0.1 / (1.0 + ratio), false)
            {
                weight = Math.Min(this.weight, result.weight) //XMIN-257
            };
            return res;
        }
        /// <summary>
        /// Changes the sign of a fuzzy number.
        /// </summary>
        /// <returns>Result of sign change.</returns>
        internal DarlResult Minus()
        {
            if (unknown)
                return new DarlResult(-1.0, true);
            if (!numeric)
                throw new MetaRuleException("passing non numeric parameter(s) to minus operator");
            return this * new DarlResult(-1.0);
        }

        /// <summary>
        /// Returns true if Result is unknown
        /// </summary>
        /// <returns>true if unknown</returns>
        public bool IsUnknown()
        {
            return unknown;
        }
        /// <summary>
        /// Returns true if Result is Numeric
        /// </summary>
        /// <returns>true if numeric</returns>
        public bool IsNumeric()
        {
            return numeric;
        }

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DarlResult"/> class.
        /// </summary>
        /// <param name="d">A double value</param>
        public DarlResult(double d)
        {
            numeric = true;
            dataType = DataType.numeric;
            values = new List<object>();
            categories = new Dictionary<string, double>();
            unknown = false;
            weight = 1.0;
            approximate = false;
            leftUnbounded = false;
            rightUnbounded = false;
            values.Add(d);
            this.Normalise(false);
            Value = d;
        }

        public DarlResult(string name, double d) : this(d)
        {
            this.name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DarlResult"/> class.
        /// </summary>
        /// <param name="i">An integer value</param>
        public DarlResult(int i) : this((double)i)
        {
        }

        public DarlResult(string name, int i) : this(i)
        {
            this.name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DarlResult"/> class.
        /// </summary>
        /// <param name="vals">A list of double values: a fuzzytuple.</param>
        public DarlResult(List<double> vals)
        {
            numeric = true;
            values = new List<object>();
            categories = new Dictionary<string, double>();
            unknown = false;
            weight = 1.0;
            approximate = false;
            leftUnbounded = false;
            rightUnbounded = false;
            vals.Sort();
            for (int n = 0; n < vals.Count; n++)
                values.Add(vals[n]);
            this.Normalise(false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DarlResult"/> class.
        /// </summary>
        /// <param name="s">A string value</param>
        public DarlResult(string name, string s)
        {
            this.name = name;
            values = new List<object>();
            categories = new Dictionary<string, double>();
            approximate = false;
            leftUnbounded = false;
            rightUnbounded = false;
            unknown = false;
            weight = 1.0;
            numeric = false;
            categories.Add(s, 1.0);
            dataType = DataType.categorical;
            Value = s;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DarlResult"/> class.
        /// </summary>
        /// <param name="res">A result value</param>
        public DarlResult(DarlResult res)
        {
            name = res.name;
            numeric = res.numeric;
            dataType = res.dataType;
            unknown = res.unknown;
            weight = res.weight;
            switch (dataType)
            {
                case DataType.numeric:
                case DataType.temporal:
                case DataType.duration:
                    values.Clear();
                    foreach (var val in res.values)
                    {
                        values.Add(val);
                    }
                    this.Normalise(false);
                    break;
                case DataType.categorical:
                    categories.Clear();
                    foreach (var cat in res.categories.Keys)
                    {
                        categories.Add(cat, res.categories[cat]);
                    }
                    Value = res.Value;
                    break;
                case DataType.textual:
                    stringConstant = res.stringConstant;
                    Value = res.Value;
                    break;
                case DataType.sequence:
                    sequence.Clear();
                    foreach (var seq in res.sequence)
                    {
                        sequence.Add(seq);
                    }
                    Value = res.Value;
                    break;
            }

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DarlResult"/> class.
        /// </summary>
        /// <param name="res">A result value</param>
        public DarlResult(string name, DarlResult res) : this(res)
        {
            this.name = name;
        }

        /*            /// <summary>
                    /// Initializes a result from a string or other object, performing appropriate type changes.
                    /// </summary>
                    /// <param name="obj">object to use to initialize. </param>
                    public Result(object obj)
                    {
                        values = new List<object>();
                        categories = new Dictionary<string, double>();
                        approximate = false;
                        leftUnbounded = false;
                        rightUnbounded = false;
                        if (obj == null)
                        {
                            unknown = true;
                            numeric = true;
                            weight = 0.0;
                        }
                        else
                        {
                            unknown = false;
                            weight = 1.0;
                            if (obj is string)
                            {
                                numeric = false;
                                categories.Add((string)obj, 1.0);
                                dataType = DataType.categorical;
                            }
                            else if (obj is Result)
                            {
                                Result res = (Result)obj;
                                numeric = res.numeric;
                                dataType = res.dataType;
                                unknown = res.unknown;
                                weight = res.weight;
                                if (numeric)
                                {
                                    values.Clear();
                                    foreach (object val in res.values)
                                    {
                                        values.Add(val);
                                    }
                                    this.Normalise(false);
                                }
                                else
                                {
                                    categories.Clear();
                                    foreach (string cat in res.categories.Keys)
                                    {
                                        categories.Add(cat, categories[cat]);
                                    }
                                }
                            }
                            else if(obj is List<List<string>>)
                            {
                                dataType = DataType.sequence;
                                sequence = (List<List<string>>)obj;
                            }
                            else
                            {
                                numeric = true;
                                if (obj is Int32)
                                {
                                    int val = (int)obj;
                                    double dval = (double)val;
                                    values.Add(dval);
                                    this.Normalise(false);
                                }
                                if (obj is Double)
                                {
                                    values.Add(obj);
                                    this.Normalise(false);
                                }
                                if (obj is List<double>)
                                {
                                    List<double> vals = obj as List<double>;
                                    vals.Sort();
                                    for(int n = 0; n < vals.Count; n++)
                                        values.Add(vals[n]);
                                    this.Normalise(false);
                                }
                            }
                        }
                    }*/



        /// <summary>
        /// Initializes a Result
        /// </summary>
        /// <param name="numeric">true if numeric</param>
        /// <param name="weight">confidence</param>
        public DarlResult(bool numeric, double weight)
        {
            this.numeric = numeric;
            unknown = false;
            this.weight = weight;
            values = new List<object>();
            categories = new Dictionary<string, double>();
            approximate = false;
            leftUnbounded = false;
            rightUnbounded = false;
            if (numeric)
            {
                cuts = new Interval[cutCount];
                dataType = DataType.numeric;
            }
            else
            {
                dataType = DataType.categorical;
            }
        }

        public DarlResult(string name, bool numeric, double weight) : this(numeric, weight)
        {
            this.name = name;
        }


        public DarlResult(DataType dtype, double weight)
        {
            unknown = false;
            this.weight = weight;
            values = new List<object>();
            categories = new Dictionary<string, double>();
            approximate = false;
            leftUnbounded = false;
            rightUnbounded = false;
            dataType = dtype;
            if (dtype == DataType.numeric || dtype == DataType.temporal || dtype == DataType.duration)
            {
                cuts = new Interval[cutCount];
            }
        }

        public DarlResult(string name, DataType dtype, double weight = 0.0) : this(dtype, weight)
        {
            this.name = name;
        }



        /// <summary>
        /// Creates and initializes a Result.
        /// </summary>
        /// <param name="numeric">true if numeric</param>
        /// <param name="weight">confidence</param>
        /// <param name="bUnknown">true if unknown</param>
        public DarlResult(bool numeric, double weight, bool bUnknown)
        {
            this.numeric = numeric;
            unknown = bUnknown;
            this.weight = weight;
            values = new List<object>();
            categories = new Dictionary<string, double>();
            approximate = false;
            leftUnbounded = false;
            rightUnbounded = false;
            if (numeric)
            {
                cuts = new Interval[cutCount];
                dataType = DataType.numeric;
            }
            else
            {
                dataType = DataType.categorical;
            }
        }

        public DarlResult(string name, double truth, bool unknown)
        {
            numeric = true;
            this.unknown = unknown;
            values = new List<object>();
            categories = new Dictionary<string, double>();
            values.Add(truth);
            weight = 1.0;
            approximate = false;
            leftUnbounded = false;
            rightUnbounded = false;
            dataType = DataType.numeric;
            this.name = name;
        }

        /// <summary>
        /// Initializes a Result containing a degree of truth.
        /// </summary>
        /// <param name="truth">Degree of truth, 0-1</param>
        /// <param name="unknown">true if unknown</param>
        public DarlResult(double truth, bool unknown)
        {
            numeric = true;
            this.unknown = unknown;
            values = new List<object>();
            categories = new Dictionary<string, double>();
            values.Add(truth);
            weight = 1.0;
            approximate = false;
            leftUnbounded = false;
            rightUnbounded = false;
            dataType = DataType.numeric;
        }
        /// <summary>
        /// Initializes an interval Result
        /// </summary>
        /// <param name="lower">first value</param>
        /// <param name="upper">second value</param>
        public DarlResult(double lower, double upper)
        {
            numeric = true;
            unknown = false;
            approximate = false;
            leftUnbounded = false;
            rightUnbounded = false;
            values = new List<object>
            {
                lower,
                upper
            };
            weight = 1.0;
            cuts = new Interval[cutCount];
            dataType = DataType.numeric;
            for (int n = 0; n < cutCount; n++)
                cuts[n] = new Interval(lower, upper);
        }
        /// <summary>
        /// Initializes a triangular set Result
        /// </summary>
        /// <param name="lower">first value</param>
        /// <param name="middle">second value</param>
        /// <param name="upper">third value</param>
        public DarlResult(double lower, double middle, double upper)
        {
            dataType = DataType.numeric;
            numeric = true;
            unknown = false;
            approximate = false;
            leftUnbounded = false;
            rightUnbounded = false;
            values = new List<object>
            {
                lower,
                middle,
                upper
            };
            weight = 1.0;
            cuts = new Interval[cutCount];
            cuts[0] = new Interval(lower, upper);
            /*               //cope with unbounded sets
                           if (double.IsInfinity(lower))
                               cuts[cutCount - 1] = new Interval(double.NegativeInfinity, middle);
                           else if (double.IsInfinity(upper))
                               cuts[cutCount - 1] = new Interval(middle, double.PositiveInfinity);
                           else*/
            cuts[cutCount - 1] = new Interval(middle);
            double tmp1 = middle - lower;
            double tmp2 = upper - middle;
            for (int n = 1; n < cutCount - 1; n++)//changed 08/08/2013 because first and last point was set twice
            {
                double tmp3 = n / (double)(cutCount - 1);
                cuts[n] = new Interval(double.IsInfinity(tmp1) ? double.NegativeInfinity : lower + tmp1 * tmp3,
                                        double.IsInfinity(tmp2) ? double.PositiveInfinity : upper - tmp2 * tmp3);
            }
        }
        /// <summary>
        /// Initializes a trapezoidal set Result
        /// </summary>
        /// <param name="lower">first value</param>
        /// <param name="lowMid">second value</param>
        /// <param name="highMid">third value</param>
        /// <param name="upper">fourth value</param>
        public DarlResult(double lower, double lowMid, double highMid, double upper)
        {
            dataType = DataType.numeric;
            numeric = true;
            unknown = false;
            approximate = false;
            leftUnbounded = false;
            rightUnbounded = false;
            values = new List<object>
            {
                lower,
                lowMid,
                highMid,
                upper
            };
            weight = 1.0;
            cuts = new Interval[cutCount];
            cuts[0] = new Interval(lower, upper);
            if (double.IsInfinity(lower))
                cuts[cutCount - 1] = new Interval(double.NegativeInfinity, highMid);
            else if (double.IsInfinity(upper))
                cuts[cutCount - 1] = new Interval(lowMid, double.PositiveInfinity);
            else
                cuts[cutCount - 1] = new Interval(lowMid, highMid);
            double tmp1 = lowMid - lower;
            double tmp2 = upper - highMid;
            for (int n = 0; n < cutCount - 1; n++)
            {
                double tmp3 = n / (double)(cutCount - 1);
                cuts[n] = new Interval(double.IsInfinity(tmp1) ? double.NegativeInfinity : lower + tmp1 * tmp3,
                    double.IsInfinity(tmp2) ? double.PositiveInfinity : upper - tmp2 * tmp3);
            }
        }
        /// <summary>
        /// Initializes a result based on the three parts of a rule evaluation.
        /// </summary>
        /// <param name="condition">The Result of the calculation of the left side of the rule</param>
        /// <param name="result">The value of the right side</param>
        /// <param name="confidence">The degree of confidence to put in the rule</param>
        internal DarlResult(DarlResult condition, DarlResult result, DarlResult confidence)
        {
            weight = confidence.weight * (double)condition.values[0];
            weight = Math.Min(weight, result.weight);
            dataType = result.dataType;
            approximate = result.approximate;
            leftUnbounded = false;
            rightUnbounded = false;
            values = new List<object>();
            categories = new Dictionary<string, double>();
            unknown = false;
            if (numeric)
            {
                foreach (object obj in result.values)
                    values.Add(obj);
                cuts = new Interval[cutCount];
                for (int n = 0; n < cutCount; n++)
                    cuts[n] = result.cuts[n];
            }
            else if (dataType == DataType.categorical)
            {
                foreach (string obj in result.categories.Keys)
                {
                    categories.Add(obj, result.categories[obj]);
                }
            }
            else if (dataType == DataType.textual)
            {
                stringConstant = result.stringConstant;
            }
            else if (dataType == DataType.seek || dataType == DataType.discover)
            {
                sequence = result.sequence;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DarlResult"/> class.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <remarks>Permits explicit textual</remarks>
        public DarlResult(string name, Object obj, DataType dataType, double maxWeight = 1.0)
        {
            this.name = name;
            values = new List<object>();
            categories = new Dictionary<string, double>();
            approximate = false;
            leftUnbounded = false;
            rightUnbounded = false;
            this.dataType = dataType;
            if (obj == null)
            {
                unknown = true;
                numeric = true;
                weight = 0.0;
                dataType = numeric ? DataType.numeric : DataType.categorical;
            }
            else
            {
                unknown = false;
                weight = maxWeight;
                switch (dataType)
                {
                    case DataType.textual:
                        this.stringConstant = (string)obj;
                        this.Value = this.stringConstant;
                        break;
                    case DataType.sequence:
                        if (obj is string)
                        {
                            this.stringConstant = (string)obj;
                        }
                        else
                        {
                            this.sequence = (List<List<string>>)obj;
                        }
                        break;
                    case DataType.duration:
                    case DataType.temporal:
                        if (obj is string)
                        {
                            if (!DarlTime.TryParse((string)obj, out DarlTime dt))
                            {
                                unknown = true;
                                weight = 0.0;
                            }
                            values.Add(dt.raw);
                            this.Normalise(false);
                        }
                        else if (obj is DarlTime dt)
                        {
                            values.Add(dt.raw);
                            this.Normalise(false);
                        }
                        else if (obj is DateTime dte)
                        {
                            values.Add(new DarlTime(dte).raw);
                            this.Normalise(false);
                        }
                        else if (obj is TimeSpan ts)
                        {
                            values.Add((double)ts.TotalSeconds);
                            this.Normalise(false);
                        }
                        else if (obj is List<DarlTime>)
                        {
                            List<DarlTime> vals = obj as List<DarlTime>;
                            vals.Sort();
                            for (int n = 0; n < vals.Count; n++)
                                values.Add(vals[n].raw);
                            this.Normalise(false);
                        }
                        else if (obj is List<DarlTime?>)
                        {
                            List<DarlTime?> vals = obj as List<DarlTime?>;
                            vals.Sort();
                            foreach (var i in vals)
                            {
                                if (i != null)
                                {
                                    values.Add(i.raw);
                                }
                            }
                            this.Normalise(false);
                        }
                        else if (obj is List<TimeSpan>)
                        {
                            List<TimeSpan> vals = obj as List<TimeSpan>;
                            vals.Sort();
                            for (int n = 0; n < vals.Count; n++)
                                values.Add((double)vals[n].TotalSeconds);
                            this.Normalise(false);
                        }
                        else if (obj is Double)
                        {
                            values.Add(obj);
                            this.Normalise(false);
                        }
                        else if (obj is List<double>)
                        {
                            List<double> vals = obj as List<double>;
                            vals.Sort();
                            for (int n = 0; n < vals.Count; n++)
                                values.Add(vals[n]);
                            this.Normalise(false);
                        }
                        break;
                    case DataType.numeric:
                        if (obj is Int32)
                        {
                            int val = (int)obj;
                            double dval = val;
                            values.Add(dval);
                            this.Normalise(false);
                        }
                        else if (obj is Double)
                        {
                            values.Add(obj);
                            this.Normalise(false);
                        }
                        else if (obj is List<double>)
                        {
                            List<double> vals = obj as List<double>;
                            vals.Sort();
                            for (int n = 0; n < vals.Count; n++)
                                values.Add(vals[n]);
                            this.Normalise(false);
                        }
                        else if (obj is string)
                        {
                            if (!double.TryParse((string)obj, out double dVal))
                            {
                                unknown = true;
                                weight = 0.0;
                            }
                            else
                            {
                                values.Add(dVal);
                                Value = obj as string;
                                this.Normalise(false);
                            }
                        }
                        break;
                    case DataType.categorical:
                        if (obj is string)
                        {
                            categories.Add((string)obj, 1.0);
                            Value = (string)obj;
                        }
                        else if (obj is Dictionary<string, double>)
                        {
                            categories = obj as Dictionary<string, double>;
                        }
                        break;

                }
            }
        }

        /// <summary>
        /// aggregating constructor
        /// </summary>
        /// <param name="name">the name of the data contained</param>
        /// <param name="vals"></param>
        /// <param name="dataType"></param>
        public DarlResult(string name, List<DarlResult> vals, DataType dataType) : this(name, 0.0, true)
        {
            //first separate out all vals that are of the right type
            var ourVals = vals.Where(a => a.name == name && a.dataType == dataType);
            if (ourVals.Any())
            {
                this.dataType = dataType;
                switch (dataType)
                {
                    case DataType.numeric:

                        if (ourVals.Count() == 1)
                        {

                        }
                        break;
                }
            }

        }

        #endregion


        /// <summary>
        /// Returns an enum representing the fuzziness of the rule
        /// </summary>
        /// <returns>Fuzziness</returns>
        public Fuzzyness HowFuzzy()
        {
            if (numeric && values.Count <= 4)
                return (Fuzzyness)values.Count;
            return Fuzzyness.unknown;
        }
        /// <summary>
        /// Calculates a neural network sigmoid function on the existing numeric values.
        /// </summary>
        /// <returns>The result</returns>
        internal DarlResult Sigmoid()
        {
            if (unknown || !numeric)
                return new DarlResult(-1.0, true);
            double dval = CofG();
            double dRes = 1.0 / (1.0 + Math.Exp(dval * -1.0));
            return new DarlResult(dRes, false);
        }

        /// <summary>
        /// Calculates the Cumulative Normal Distribution of a value for a normal distribution of average zero and SD 1.
        /// From Abromowitz and Stegun, Handbook of Mathematical Functions
        /// </summary>
        /// <returns>Percentile rank scaled 0-1</returns>
        internal DarlResult NormProb()
        {
            const double b1 = 0.319381530;
            const double b2 = -0.356563782;
            const double b3 = 1.781477937;
            const double b4 = -1.821255978;
            const double b5 = 1.330274429;
            const double p = 0.2316419;
            const double c = 0.39894228;

            Normalise(true);
            if (unknown || !numeric)
                return new DarlResult(0.0, true);
            double x = CofG();

            if (x >= 0.0)
            {
                double t = 1.0 / (1.0 + p * x);
                return new DarlResult((1.0 - c * Math.Exp(-x * x / 2.0) * t *
                (t * (t * (t * (t * b5 + b4) + b3) + b2) + b1)));
            }
            else
            {
                double t = 1.0 / (1.0 - p * x);
                return new DarlResult((c * Math.Exp(-x * x / 2.0) * t *
                (t * (t * (t * (t * b5 + b4) + b3) + b2) + b1)));
            }
        }

        #region operators
        /// <summary>
        /// Compares two Results for equality.
        /// </summary>
        /// <param name="res1">Result 1</param>
        /// <param name="res2">Result 2</param>
        /// <returns>true if equal</returns>
        public static bool operator ==(DarlResult res1, DarlResult res2)
        {
            return res1.Equals(res2);
        }
        /// <summary>
        /// Compares two Results for inequality.
        /// </summary>
        /// <param name="res1">Result 1</param>
        /// <param name="res2">Result 2</param>
        /// <returns>true if unequal</returns>
        public static bool operator !=(DarlResult res1, DarlResult res2)
        {
            return !(res1 == res2);
        }
        /// <summary>
        /// Returns the degree of truth to the statement "Result1 is greater than Result 2".
        /// </summary>
        /// <remarks>Based on B. Dorohonceanu. Comparing Fuzzy Numbers, Algorithm Alley, Dr. Dobb's Journal, vol. 343, pp. 38-45, CMP Media LLC., San Francisco, CA, 12/2002,  ISSN 1044-789X. Also in Dr. Dobb's CD Release 14, CMP Media LLC., San Francisco, CA, 8/2003. 
        /// Used as the basis for the other comparisons.</remarks>
        /// <param name="res1">Result 1</param>
        /// <param name="res2">Result 2</param>
        /// <returns>Result</returns>
        public static DarlResult operator >(DarlResult res1, DarlResult res2)
        {
            if (res1 == res2)
                return new DarlResult(0.0, false);
            if (!res1.numeric || !res2.numeric)
                throw new MetaRuleException("passing non numeric parameter(s) to > operator");
            if (res1.unknown || res2.unknown || !res1.numeric || !res2.numeric)
                return new DarlResult(-1.0, true);
            //if both singletons 
            if (res1.cuts[0].IsSingleton() && res2.cuts[0].IsSingleton())
                return new DarlResult(res1.cuts[0].lower > res2.cuts[0].lower ? 1.0 : 0.0, false);
            if (res1.cuts[0].IsSingleton())
            {
                if (res1.cuts[0].lower > res2.cuts[0].upper)
                    return new DarlResult(1.0, false);
                if (res1.cuts[0].lower < res2.cuts[0].lower)
                    return new DarlResult(0.0, false);
                int n;
                for (n = 0; n < cutCount && res1.cuts[0].lower <= res2.cuts[n].upper; n++) ;
                if (n == 0)
                    return new DarlResult(1.0, false);
                if (n == DarlResult.cutCount)
                    return new DarlResult(0.0, false);
                //else interpolate
                double divisor = (res2.cuts[n].upper - res2.cuts[n - 1].upper);
                double remainder = res1.cuts[0].lower - res2.cuts[n - 1].upper;
                return new DarlResult((n - 1) * 0.1 + remainder * 0.1 / divisor, false);
            }
            if (res2.cuts[0].IsSingleton())
            {
                if (res1.cuts[0].lower > res2.cuts[0].upper)
                    return new DarlResult(1.0, false);
                if (res1.cuts[0].upper < res2.cuts[0].lower)
                    return new DarlResult(0.0, false);
                int n;
                for (n = 0; n < cutCount && res2.cuts[0].upper <= res1.cuts[n].upper; n++) ;
                if (n == 0)
                    return new DarlResult(0.0, false);
                if (n == DarlResult.cutCount)
                    return new DarlResult(1.0, false);
                //else interpolate
                double divisor = (res1.cuts[n].upper - res1.cuts[n - 1].upper);
                double remainder = res2.cuts[0].upper - res1.cuts[n - 1].upper;
                return new DarlResult((n - 1) * 0.1 + remainder * 0.1 / divisor, false);
            }
            double alpha = 0.0, m = 0.0;
            for (int i = 0; i < cutCount; i++)
            {
                double p = res1.cuts[i].length * res2.cuts[i].length;
                m += p;
                alpha += p * Interval.GreaterThan(res1.cuts[i], res2.cuts[i]);
            }
            return new DarlResult((m == 0.0) ? alpha : (alpha / m), false);
        }
        /// <summary>
        /// Compares if Result1 is lesser than Result 2.
        /// </summary>
        /// <param name="res1">Result 1</param>
        /// <param name="res2">Result 2</param>
        /// <returns>Result</returns>
        public static DarlResult operator <(DarlResult res1, DarlResult res2)
        {
            if (res1 == res2)
                return new DarlResult(0.0, false);
            if (!res1.numeric || !res2.numeric)
                throw new MetaRuleException("passing non numeric parameter(s) to > operator");
            if (res1.unknown || res2.unknown)
                return new DarlResult(-1.0, true);
            return !(res1 > res2);
        }
        /// <summary>
        /// Compares if Result1 is greater than or equal to Result 2.
        /// </summary>
        /// <param name="res1">Result 1</param>
        /// <param name="res2">Result 2</param>
        /// <returns>Result</returns>
        public static DarlResult operator >=(DarlResult res1, DarlResult res2)
        {
            if (res1 == res2)
                return new DarlResult(1.0, false);
            if (!res1.numeric || !res2.numeric)
                throw new MetaRuleException("passing non numeric parameter(s) to > operator");
            if (res1.unknown || res2.unknown)
                return new DarlResult(-1.0, true);
            return (res1 > res2) | res1.Equal(res2);
        }
        /// <summary>
        /// Compares if Result1 is less than or equal to Result 2.
        /// </summary>
        /// <param name="res1">Result 1</param>
        /// <param name="res2">Result 2</param>
        /// <returns>Result</returns>
        public static DarlResult operator <=(DarlResult res1, DarlResult res2)
        {
            if (res1 == res2)
                return new DarlResult(1.0, false);
            if (!res1.numeric || !res2.numeric)
                throw new MetaRuleException("passing non numeric parameter(s) to > operator");
            if (res1.unknown || res2.unknown)
                return new DarlResult(-1.0, true);
            return (res1 < res2) | res1.Equal(res2);
        }
        /// <summary>
        /// Adds Result 1 to Result 2.
        /// </summary>
        /// <param name="res1">Result 1</param>
        /// <param name="res2">Result 2</param>
        /// <returns>Result</returns>
        public static DarlResult operator +(DarlResult res1, DarlResult res2)
        {
            if (res1.unknown || res2.unknown)
                return new DarlResult(-1.0, true);
            if (!res1.numeric || !res2.numeric)
                throw new MetaRuleException("passing non numeric parameter(s) to a + operator");
            DarlResult res = (res1.temporal || res2.temporal) ? new DarlResult("", 0, DarlResult.DataType.temporal) : new DarlResult();
            res.values.Clear();
            res.cuts = new Interval[cutCount];
            for (int n = 0; n < cutCount; n++)
                res.cuts[n] = res1.cuts[n] + res2.cuts[n];
            res.weight = Math.Min(res1.weight, res2.weight);
            return res;
        }
        /// <summary>
        /// subtracts Result 2 from Result 1.
        /// </summary>
        /// <param name="res1">Result 1</param>
        /// <param name="res2">Result 2</param>
        /// <returns>Result</returns>
        public static DarlResult operator -(DarlResult res1, DarlResult res2)
        {
            if (res1.unknown || res2.unknown)
                return new DarlResult(-1.0, true);
            if (!res1.numeric || !res2.numeric)
                throw new MetaRuleException("passing non numeric parameter(s) to a - operator");
            DarlResult res = (res1.temporal || res2.temporal) ? new DarlResult("", 0, DarlResult.DataType.temporal) : new DarlResult();
            res.values.Clear();
            res.cuts = new Interval[cutCount];
            for (int n = 0; n < cutCount; n++)
                res.cuts[n] = res1.cuts[n] - res2.cuts[n];
            res.weight = Math.Min(res1.weight, res2.weight);
            return res;
        }
        /// <summary>
        /// multiplies Result 1 by Result 2.
        /// </summary>
        /// <param name="res1">Result 1</param>
        /// <param name="res2">Result 2</param>
        /// <returns>Result</returns>
        public static DarlResult operator *(DarlResult res1, DarlResult res2)
        {
            if (res1.unknown || res2.unknown)
                return new DarlResult(-1.0, true);
            if (!res1.numeric || !res2.numeric)
                throw new MetaRuleException("passing non numeric parameter(s) to a * operator");
            DarlResult res = new DarlResult
            {
                cuts = new Interval[cutCount]
            };
            for (int n = 0; n < cutCount; n++)
                res.cuts[n] = res1.cuts[n] * res2.cuts[n];
            res.weight = Math.Min(res1.weight, res2.weight);
            return res;
        }
        /// <summary>
        /// Divides Result 1 by Result 2.
        /// </summary>
        /// <param name="res1">Result 1</param>
        /// <param name="res2">Result 2</param>
        /// <returns>Result</returns>
        public static DarlResult operator /(DarlResult res1, DarlResult res2)
        {
            if (res1.unknown || res2.unknown)
                return new DarlResult(-1.0, true);
            if (!res1.numeric || !res2.numeric)
                throw new MetaRuleException("passing non numeric parameter(s) to a / operator");
            DarlResult res = new DarlResult
            {
                cuts = new Interval[cutCount]
            };
            for (int n = 0; n < cutCount; n++)
                res.cuts[n] = res1.cuts[n] / res2.cuts[n];
            res.weight = Math.Min(res1.weight, res2.weight);
            return res;
        }
        /// <summary>
        /// Divides Result 1 by an int.
        /// </summary>
        /// <param name="res1">Result 1</param>
        /// <param name="res2">integer divisor</param>
        /// <returns>Result</returns>
        public static DarlResult operator /(DarlResult res1, int res2)
        {
            if (res1.unknown)
                return new DarlResult(-1.0, true);
            if (!res1.numeric)
                throw new MetaRuleException("Passing non numeric parameter(s) to a / operator");
            if (res2 == 0)
                throw new MetaRuleException("Divide by zero in an integer / operator");
            DarlResult res = new DarlResult
            {
                cuts = new Interval[cutCount]
            };
            for (int n = 0; n < cutCount; n++)
                res.cuts[n] = res1.cuts[n] / res2;
            res.weight = res1.weight;
            return res;
        }
        /// <summary>
        /// raises Result 1 to the power Result 2.
        /// </summary>
        /// <param name="res1">Result 1</param>
        /// <param name="res2">Result 2</param>
        /// <returns>Result</returns>
        public static DarlResult operator ^(DarlResult res1, DarlResult res2)
        {
            if (res1.unknown || res2.unknown)
                return new DarlResult(-1.0, true);
            if (!res1.numeric || !res2.numeric)
                throw new MetaRuleException("passing non numeric parameter(s) to a ^ operator");
            DarlResult res = new DarlResult
            {
                cuts = new Interval[cutCount]
            };
            for (int n = 0; n < cutCount; n++)
                res.cuts[n] = Interval.power(res1.cuts[n], res2.cuts[n]);
            res.weight = Math.Min(res1.weight, res2.weight);
            return res;
        }
        /// <summary>
        /// Performs logical or operator between two logical Results
        /// Throws exception if not singletons.
        /// </summary>
        /// <param name="res1">Result 1</param>
        /// <param name="res2">Result 2</param>
        /// <returns>Result</returns>
        public static DarlResult operator |(DarlResult res1, DarlResult res2)
        {
            if (res1.unknown && res2.unknown)
                return new DarlResult(-1.0, true);
            if (!res1.numeric || !res2.numeric)
                throw new MetaRuleException("passing non logical parameter(s) to a | operator");
            if ((double)res1.values[0] > 1.0 || (double)res1.values[0] < -1.0 || (double)res2.values[0] > 1.0 || (double)res2.values[0] < -1.0)
                throw new MetaRuleException("passing non logical (out of range) parameter(s) to a | operator");
            if (res1.values.Count != 1 || res2.values.Count != 1)
                throw new MetaRuleException("Operations other than comparison not available for fuzzy numbers in this version");
            if (res1.unknown)
                return res2;
            if (res2.unknown)
                return res1;
            return new DarlResult(Math.Max((double)res1.values[0], (double)res2.values[0]));
        }
        /// <summary>
        /// Performs logical "and" operator between two logical Results
        /// Throws exception if not singletons.
        /// </summary>
        /// <param name="res1">Result 1</param>
        /// <param name="res2">Result 2</param>
        /// <returns>Result</returns>
        public static DarlResult operator &(DarlResult res1, DarlResult res2)
        {
            if (res1.unknown || res2.unknown)
                return new DarlResult(-1.0, true);
            if (!res1.numeric || !res2.numeric)
                throw new MetaRuleException("passing non logical parameter(s) to a & operator");
            if ((double)res1.values[0] > 1.0 || (double)res1.values[0] < -1.0 || (double)res2.values[0] > 1.0 || (double)res2.values[0] < -1.0)
                throw new MetaRuleException("passing non logical (out of range) parameter(s) to a & operator");
            if (res1.values.Count != 1 || res2.values.Count != 1)
                throw new MetaRuleException("Operations other than comparison not available for fuzzy numbers in this version");
            return new DarlResult(Math.Min((double)res1.values[0], (double)res2.values[0]));
        }
        /// <summary>
        /// Performs logical not operator on a single Result
        /// Throws exception if not singleton.
        /// </summary>
        /// <param name="res1">Result 1</param>
        /// <returns>Result</returns>
        public static DarlResult operator !(DarlResult res1)
        {
            if (!res1.numeric ||
                res1.values.Count != 1 ||
                (double)res1.values[0] > 1.0 ||
                (double)res1.values[0] < -1.0)
                return new DarlResult(-1.0, true);
            if ((double)res1.values[0] >= 0.0 && !res1.IsUnknown())
                return new DarlResult(1.0 - (double)res1.values[0], false);
            return new DarlResult(-1.0, true);
        }

        #endregion
        /// <summary>
        /// Looks for strict equality between two results.
        /// </summary>
        /// <param name="o">Other Result</param>
        /// <returns>if they are identical</returns>
        public override bool Equals(object o)
        {
            // look for strict equality
            if (!(o is DarlResult))
                return false;
            DarlResult v = (DarlResult)o;
            if (unknown != v.unknown)
                return false;
            if (numeric != v.numeric)
                return false;
            if (numeric)
            {
                if (values.Count != v.values.Count)
                    return false;
                for (int n = 0; n < values.Count; n++)
                {
                    if ((double)values[n] != (double)v.values[n])
                        return false;
                }
            }
            else if (dataType == DataType.textual)
            {
                return Regex.IsMatch(v.stringConstant, stringConstant);
            }
            else
            {
                if (categories.Count != v.categories.Count)
                    return false;
                foreach (string key in categories.Keys)
                {
                    if (!v.categories.ContainsKey(key))
                        return false;
                }
            }
            return true;

        }

        private List<List<string>> ValuesToSequence(List<object> values)
        {
            var list = new List<List<string>>();
            foreach (var o in values)
            {
                var sublist = new List<string>();
                var sl = o as List<string>;
                foreach (var s in sl)
                    sublist.Add(s);
                list.Add(sublist);
            }
            return list;
        }

        /// <summary>
        /// Returns the minimum of two fuzzy numbers
        /// </summary>
        /// <param name="res1">Fuzzy number 1</param>
        /// <param name="res2">Fuzzy number 2</param>
        /// <returns>Fuzzy number representing the minimum</returns>
        internal static DarlResult Minimum(DarlResult res1, DarlResult res2)
        {
            if (res1.unknown || res2.unknown)
                return new DarlResult(-1.0, true);
            DarlResult res = new DarlResult
            {
                cuts = new Interval[cutCount]
            };
            for (int n = 0; n < cutCount; n++)
                res.cuts[n] = Interval.min(res1.cuts[n], res2.cuts[n]);
            res.weight = Math.Min(res1.weight, res2.weight);
            return res;
        }
        /// <summary>
        /// Returns the maximum of two fuzzy numbers
        /// </summary>
        /// <param name="res1">Fuzzy number 1</param>
        /// <param name="res2">Fuzzy number 2</param>
        /// <returns>Fuzzy number representing the maximum</returns>
        internal static DarlResult Maximum(DarlResult res1, DarlResult res2)
        {
            if (res1.unknown || res2.unknown)
                return new DarlResult(-1.0, true);
            DarlResult res = new DarlResult
            {
                cuts = new Interval[cutCount]
            };
            for (int n = 0; n < cutCount; n++)
                res.cuts[n] = Interval.max(res1.cuts[n], res2.cuts[n]);
            res.weight = Math.Min(res1.weight, res2.weight);
            return res;
        }

        /// <summary>
        /// Returns the Modulus of one fuzzy number by another
        /// </summary>
        /// <param name="res1">Fuzzy number 1</param>
        /// <param name="res2">Fuzzy number 2</param>
        /// <returns>Fuzzy number representing the modulus</returns>
        /// <remarks>Not yet implemented</remarks>
        internal static DarlResult Modulus(DarlResult res1, DarlResult res2)
        {
            if (res1.unknown || res2.unknown)
                return new DarlResult(-1.0, true);
            DarlResult res = new DarlResult
            {
                cuts = new Interval[cutCount]
            };
            for (int n = 0; n < cutCount; n++)
                res.cuts[n] = Interval.max(res1.cuts[n], res2.cuts[n]);
            res.weight = Math.Min(res1.weight, res2.weight);
            return res;
        }

        /// <summary>
        /// Returns one fuzzy number rounded by another
        /// </summary>
        /// <param name="res1">Fuzzy number 1</param>
        /// <param name="res2">Fuzzy number 2</param>
        /// <returns>Fuzzy number representing the rounded value</returns>
        /// <remarks>Not yet implemented</remarks>
        internal static DarlResult Round(DarlResult res1, DarlResult res2)
        {
            if (res1.unknown || res2.unknown)
                return new DarlResult(-1.0, true);
            DarlResult res = new DarlResult
            {
                cuts = new Interval[cutCount]
            };
            for (int n = 0; n < cutCount; n++)
                res.cuts[n] = Interval.max(res1.cuts[n], res2.cuts[n]);
            res.weight = Math.Min(res1.weight, res2.weight);
            return res;
        }

        /// <summary>
        /// synchronizes the values and cuts representation.
        /// </summary>
        /// <remarks>May involve approximation.</remarks>
        /// <param name="cutsToValues">if true values are initialized from cuts, otherwise the inverse</param>
        public void Normalise(bool cutsToValues)
        {
            approximate = false;
            if (IsNumeric() && !IsUnknown())
            {
                if (cutsToValues)
                {
                    values.Clear();
                    //singleton
                    if (cuts[cutCount - 1].IsSingleton() && cuts[0].IsSingleton())
                    {
                        values.Add(cuts[0].lower);
                        return;
                    }
                    //interval
                    if (cuts[cutCount - 1].Equals(cuts[0]))
                    {
                        values.Add(cuts[0].lower);
                        values.Add(cuts[0].upper);
                        return;
                    }
                    //triangular - cuts may line up, or may not where anything like
                    //multiplication or division has taken place. Approximate using the 
                    //lines from the peak through the 0.5 truth level.
                    if (cuts[cutCount - 1].IsSingleton())
                    {
                        double lowerMid = cuts[(cutCount - 1) / 2].lower;
                        double centre = cuts[cutCount - 1].lower;
                        values.Add(lowerMid - (centre - lowerMid));
                        values.Add(centre);
                        double upperMid = cuts[(cutCount - 1) / 2].upper;
                        values.Add(upperMid + (upperMid - centre));
                        if (lowerMid - (centre - lowerMid) != cuts[0].lower ||
                            upperMid + (upperMid - centre) != cuts[0].upper)
                            approximate = true;
                    }
                    else // trapezoid - approximate as above
                    {
                        double lowerMid = cuts[(cutCount - 1) / 2].lower;
                        double lowerTop = cuts[cutCount - 1].lower;
                        double upperTop = cuts[cutCount - 1].upper;
                        values.Add(lowerMid - (lowerTop - lowerMid));
                        values.Add(lowerTop);
                        values.Add(upperTop);
                        double upperMid = cuts[(cutCount - 1) / 2].upper;
                        values.Add(upperMid + (upperMid - upperTop));
                        if (lowerMid - (lowerTop - lowerMid) != cuts[0].lower ||
                            upperMid + (upperMid - upperTop) != cuts[0].upper)
                            approximate = true;
                    }
                }
                else
                {
                    if (values.Count == 0 || values.Count > 4)
                        return;
                    cuts = new Interval[cutCount];
                    double lower = (double)values[0];
                    double upper = (double)values[values.Count - 1];
                    double lowMid = (double)values[values.Count > 2 ? 1 : 0];
                    double highMid = (double)values[values.Count / 2];
                    cuts[0] = new Interval(lower, upper);
                    cuts[cutCount - 1] = new Interval(lowMid, highMid);
                    double tmp1 = lowMid - lower;
                    double tmp2 = upper - highMid;
                    for (int n = 0; n < cutCount - 1; n++)
                    {
                        double tmp3 = n / (double)(cutCount - 1);
                        cuts[n] = new Interval(lower + tmp1 * tmp3,
                            upper - tmp2 * tmp3);
                    }
                }
            }
        }
        /// <summary>
        /// Used to create hash-tables of results
        /// </summary>
        /// <returns>Hash value</returns>
        public override int GetHashCode()
        {
            if (this.IsNumeric())
                return this.values.GetHashCode();
            else
                return this.categories.GetHashCode();
        }
        /// <summary>
        /// Calculates the center of gravity of the numeric values
        /// </summary>
        /// <returns>the center of gravity</returns>
        internal double CofG()
        {
            double centroid = 0.0;
            for (int n = 0; n < cutCount; n++)
                centroid += cuts[n].Mean();
            return centroid / cutCount;
        }


        /// <summary>
        /// number of alpha cuts used to represent fuzzy numbers
        /// </summary>
        internal const int cutCount = 11;
        /// <summary>
        /// Indicates approximation has taken place in calculating the values.
        /// </summary>
        /// <remarks>Under some circumstances the coordinates of the fuzzy number 
        /// in "values" may not exactly represent the "cuts" values.</remarks>
        public bool approximate;
        /// <summary>
        /// Indicates the set should be considered unbounded on the lower side
        /// </summary>
        public bool leftUnbounded;
        /// <summary>
        /// Indicates the set should be considered unbounded on the upper side
        /// </summary>
        public bool rightUnbounded;
        #region IComparable Members
        /// <summary>
        /// Compares results in order to sort them
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="obj" /> in the sort order. Zero This instance occurs in the same position in the sort order as <paramref name="obj" />. Greater than zero This instance follows <paramref name="obj" /> in the sort order.
        /// </returns>
        public int CompareTo(object obj)
        {
            if (obj is DarlResult)
            {
                DarlResult res = obj as DarlResult;
                if (IsNumeric())
                {
                    return FindCentre().CompareTo(res.FindCentre());
                }
            }
            return 0;
        }

        #endregion
        private double FindCentre()
        {
            if (IsNumeric())
            {
                switch (HowFuzzy())
                {
                    case Fuzzyness.singleton:
                        return (double)this.values[0];
                    case Fuzzyness.interval:
                        return ((double)values[0] + (double)values[1]) / 2.0;
                    case Fuzzyness.triangle:
                        return (double)values[1];
                    case Fuzzyness.trapezoid:
                        return ((double)values[1] + (double)values[2]) / 2.0;
                }
            }
            return double.NaN;
        }

        /// <summary>
        /// optional identifier used for sets
        /// </summary>
        public string identifier;

        /// <summary>
        /// returns the interval at a particular confidence level or cut for numeric Results
        /// </summary>
        internal Interval IntervalAtConfidence(double confidence)
        {
            if (numeric && confidence >= 0 && confidence <= 1.0)
            {
                int level = (int)(confidence * 10);
                double remainder = confidence - (double)level / 10;
                if (remainder > -0.001 || remainder < 0.001)
                    return cuts[level];
                //linearly interpolate between the neighboring cuts
                remainder *= 10.0;
                return new Interval(remainder * (cuts[level + 1].lower - cuts[level].lower), remainder * (cuts[level].upper - cuts[level + 1].upper));
            }
            return null;
        }
        /// <summary>
        /// Displays contents of the result
        /// </summary>
        /// <returns>a formatted string</returns>
        public override string ToString()
        {
            string result = $"{name}, ";
            if (unknown)
            {
                result += "unknown";
            }
            else if (numeric)
            {
                for (int n = 0; n < values.Count; n++)
                {
                    if (n > 0)
                        result += ",";
                    result += values[n].ToString();
                }
                if (values.Count > 1)
                    result += "f(" + result + ")";
                if (this.approximate)
                    result = "~" + result;
            }
            else if (dataType == DataType.categorical)
            {
                int seq = 0;
                if (categories.Count == 1)
                {
                    result += categories.Keys.First();
                }
                else
                {
                    foreach (string cat in categories.Keys)
                    {
                        string separator = "";
                        if (seq++ != 0)
                            separator = ", ";
                        result += separator + cat + " : " + ((double)categories[cat]).ToString("#.##");
                    }
                }
            }
            else if (dataType == DataType.textual)
            {
                result += stringConstant;
            }
            return result;
        }
        /// <summary>
        /// Displays the contents of the result with numeric values formatted
        /// </summary>
        /// <param name="format">format string <see cref="Double.ToString()"/></param>
        /// <returns>the formatted string</returns>
        public string ToString(string format)
        {
            string result = "";
            if (unknown)
            {
                result = "unknown";
            }
            else if (numeric)
            {
                for (int n = 0; n < values.Count; n++)
                {
                    if (n > 0)
                        result += ",";
                    result += ((double)values[n]).ToString(format);
                }
                if (values.Count > 1)
                    result = "f(" + result + ")";
                if (this.approximate)
                    result = "~" + result;
            }
            else if (dataType == DataType.categorical)
            {
                int seq = 0;
                foreach (string cat in categories.Keys)
                {
                    string separator = "";
                    if (seq++ != 0)
                        separator = ", ";
                    result += separator + cat + " : " + ((double)categories[cat]).ToString(format);
                }
            }
            else if (dataType == DataType.textual)
            {
                result = stringConstant;
            }

            return result;
        }

        public string ToStringContent()
        {
            string result = string.Empty;
            if (unknown)
            {
                result += "unknown";
            }
            else if (dataType == DataType.numeric)
            {
                for (int n = 0; n < values.Count; n++)
                {
                    if (n > 0)
                        result += ",";
                    result += ((double)values[n]).ToString("N2");
                }
                if (values.Count > 1)
                    result += "f(" + result + ")";
                if (this.approximate)
                    result = "~" + result;
            }
            else if (dataType == DataType.categorical)
            {
                int seq = 0;
                if (categories.Count == 1)
                {
                    result += categories.Keys.First();
                }
                else
                {
                    foreach (string cat in categories.Keys)
                    {
                        string separator = "";
                        if (seq++ != 0)
                            separator = ", ";
                        result += separator + cat + " : " + ((double)categories[cat]).ToString("#.##");
                    }
                }
            }
            else if (dataType == DataType.textual)
            {
                result += stringConstant;
            }
            else if (dataType == DataType.temporal)
            {
                long ticks = (long)((double)values[0] * 10000000.0);
                var ts = new DarlTime(new DateTime(ticks));
                result += ts.dateTime.ToShortDateString();
            }
            return result;
        }


        /// <summary>
        /// updates the public data representation if revision is more accurate
        /// </summary>
        /// <param name="revision">Potentially more accurate data</param>
        internal void Update(DarlResult revision)
        {
            if (revision.unknown)
                return;
            if (revision.weight < weight)
                return;
            if (revision.weight > weight || unknown)
            {
                Copy(revision);
                return;
            }
            // both have same weight
            if (numeric)
            {
                double gap = 0.0;
                // if they agree, tighten the range
                if (Interval.Overlap(cuts[0], revision.cuts[0], ref gap))
                {
                    if (Area() >= revision.Area())
                        Copy(revision);
                }
                else // broaden the range
                {
                    for (int n = 0; n < DarlResult.cutCount; n++)
                    {
                        this.cuts[n] = Interval.Conjunction(this.cuts[n], revision.cuts[n]);
                    }
                }
                this.Normalise(true);
            }
            else
            {
                foreach (string cat in revision.categories.Keys)
                {
                    if (this.categories.ContainsKey(cat))
                        this.categories[cat] = Math.Max((double)this.categories[cat], (double)revision.categories[cat]);
                    else
                        this.categories.Add(cat, revision.categories[cat]);
                }
            }
        }
        /// <summary>
        /// Copy revision into this
        /// </summary>
        /// <param name="revision">result to be copied</param>
        internal void Copy(DarlResult revision)
        {
            this.approximate = revision.approximate;
            this.numeric = revision.numeric;
            this.identifier = revision.identifier;
            this.unknown = false;
            if (numeric)
            {
                this.values.Clear();
                this.values = new List<object>(revision.values);
                for (int n = 0; n < DarlResult.cutCount; n++)
                    this.cuts[n] = revision.cuts[n];
            }
            else
            {
                this.categories.Clear();
                this.categories = new Dictionary<string, double>(revision.categories);
            }
            weight = revision.weight;
        }
        /// <summary>
        /// returns the area under the fuzzy number.
        /// </summary>
        internal double Area()
        {
            double res = 0.0;
            if (numeric)
            {
                for (int n = 0; n < cutCount; n++)
                {
                    res += cuts[n].length;
                }
            }
            return res;
        }

        /// <summary>
        /// Look up the crisp value in the attached table
        /// </summary>
        /// <param name="list">The table</param>
        /// <returns>The looked up value, an interpolated value or an unknown result</returns>
        internal DarlResult TableLookup(SortedList<double, double> list)
        {
            Normalise(true);
            if (unknown || !numeric)
                return new DarlResult(0.0, true);
            double x = CofG();
            //consider the well-behaved case first
            if (list.ContainsKey(x))
                return new DarlResult(list[x]);
            //handle out of range
            if (x < list.Keys[0] || x > list.Keys[list.Count - 1])
                return new DarlResult(0.0, true);
            //interpolate - perform binary search
            int range = list.Count / 2;
            int pos = range;
            while (true)
            {
                if (x > list.Keys[pos])
                {
                    if (x > list.Keys[pos + 1]) //continue search
                    {
                        range = Math.Max(range / 2, 1);
                        pos += range;
                    }
                    else //found it
                    {
                        double x1 = list.Keys[pos];
                        double x2 = list.Keys[pos + 1];
                        double y1 = list[x1];
                        return new DarlResult((x - x1) * (list[x2] - y1) / (x2 - x1) + y1);
                    }
                }
                else
                {
                    if (x < list.Keys[pos - 1]) //continue search
                    {
                        range = Math.Max(range / 2, 1);
                        pos -= range;
                    }
                    else //found it
                    {
                        double x1 = list.Keys[pos - 1];
                        double x2 = list.Keys[pos];
                        double y1 = list[x1];
                        return new DarlResult((x - x1) * (list[x2] - y1) / (x2 - x1) + y1);
                    }
                }

            }
        }

        /// <summary>
        /// returns the support of the given sets.
        /// </summary>
        /// <param name="res1">The res1.</param>
        /// <param name="res2">The res2.</param>
        /// <returns>A result encompassing the given results</returns>
        public static DarlResult Support(DarlResult res1, DarlResult res2)
        {
            double min = Math.Min((double)res1.values[0], (double)res2.values[0]);
            double max = Math.Max((double)res1.values.Last(), (double)res2.values.Last());
            return new DarlResult(min, max);
        }

        /// <summary>
        /// Returns the practical support of the given sets
        /// </summary>
        /// <param name="res1"></param>
        /// <param name="res2"></param>
        /// <returns></returns>
        public static DarlResult PracticalSupport(DarlResult r1, DarlResult r2)
        {
            double min = 0;
            double max = 0;
            if (r1.values.Count == 0)
            {
                if (r2.values.Count == 0)
                    throw new ArgumentOutOfRangeException("Both arguments contain no values");
                return r2;
            }
            else
            {
                //min processing
                if (!double.IsInfinity((double)r1.values[0]))
                {
                    if (r2.values.Count == 0)
                    {
                        min = (double)r1.values[0];
                    }
                    if (!double.IsInfinity((double)r2.values[0]))
                    {
                        min = Math.Min((double)r1.values[0], (double)r2.values[0]);
                    }
                    else if (r2.values.Count > 1)
                    {
                        min = Math.Min((double)r1.values[0], (double)r2.values[1]);
                    }
                }
                else
                {
                    if (r1.values.Count == 1)
                    {
                        //what if r2 is infinite too?
                        return r2;
                    }
                    else// > 1
                    {
                        if (r2.values.Count == 0)
                        {
                            min = (double)r1.values[1];
                        }
                        if (!double.IsInfinity((double)r2.values[0]))
                        {
                            min = Math.Min((double)r1.values[1], (double)r2.values[0]);
                        }
                        else if (r2.values.Count > 1)
                        {
                            if (!double.IsInfinity((double)r1.values[1]) && !double.IsInfinity((double)r2.values[1]))
                            {
                                min = Math.Min((double)r1.values[1], (double)r2.values[1]);
                            }
                            else
                            {
                                throw new ArgumentOutOfRangeException("No practical range.");
                            }
                        }
                    }
                }
                //max processing
                if (!double.IsInfinity((double)r1.values.Last()))
                {
                    if (r2.values.Count == 0)
                    {
                        max = (double)r1.values.Last();
                    }
                    if (!double.IsInfinity((double)r2.values.Last()))
                    {
                        max = Math.Max((double)r1.values.Last(), (double)r2.values.Last());
                    }
                    else if (r2.values.Count > 1)
                    {
                        max = Math.Max((double)r1.values.Last(), (double)r2.values[r2.values.Count - 2]);
                    }
                }
                else
                {
                    if (r1.values.Count == 1)
                    {
                        //what if r2 is infinite too?
                        return r2;
                    }
                    else// > 1
                    {
                        if (r2.values.Count == 0)
                        {
                            max = (double)r1.values[r1.values.Count - 2];
                        }
                        if (!double.IsInfinity((double)r2.values.Last()))
                        {
                            max = Math.Max((double)r1.values[r1.values.Count - 2], (double)r2.values.Last());
                        }
                        else if (r2.values.Count > 1)
                        {
                            max = Math.Max((double)r1.values[r1.values.Count - 2], (double)r2.values[r2.values.Count - 2]);
                        }
                    }
                }
            }
            return new DarlResult(min, max);
        }

        public DarlResult Convert(DataType toType)
        {
            switch (toType)
            {
                case DataType.numeric:
                    switch (dataType)
                    {
                        case DataType.categorical:
                            return new DarlResult(0.0, true); //no conversion
                        case DataType.textual:
                            {
                                if (double.TryParse(stringConstant, out double val))
                                {
                                    var d = new DarlResult(val)
                                    {
                                        weight = weight
                                    };
                                    return d;
                                }
                                return new DarlResult(0.0, true); //no conversion
                            }
                        case DataType.temporal:
                            return new DarlResult(0.0, true); //no conversion
                    }
                    break;
                case DataType.categorical:
                    return new DarlResult(0.0, true); //no conversion
                case DataType.textual:
                    switch (dataType)
                    {
                        case DataType.numeric:
                            {
                                var res = new DarlResult("", values.Count > 0 ? values[0].ToString() : "", DataType.textual)
                                {
                                    weight = weight
                                };
                                return res;
                            }

                        case DataType.categorical:
                            {

                                DarlResult res = new DarlResult("", Value == null ? string.Empty : Value.ToString(), DataType.textual)
                                {
                                    weight = weight
                                };
                                return res;
                            }

                    }
                    break;
                case DataType.temporal:
                    switch (dataType)
                    {
                        case DataType.categorical:
                            return new DarlResult(0.0, true); //no conversion
                        case DataType.textual:
                            {
                                if (DarlTime.TryParse(stringConstant, out DarlTime val))
                                {
                                    DarlResult d = new DarlResult("", val, DataType.temporal)
                                    {
                                        weight = weight
                                    };
                                    return d;
                                }
                                return new DarlResult(0.0, true); //no conversion
                            }
                    }
                    break;
            }
            return this;
        }
        public DarlTime ToDarlTime()
        {
            return new DarlTime((double)Value);
        }





        //Makes use of Allen, J, Maintaining Knowledge about temporal intervals, 1983 ACM 26(11)
        //and Schockaert S, et al., An Efficient Characterization of Fuzzy Temporal Interval Relations 2006 IEEE Int. Conf.
        #region temporal_operators

        /// <summary>
        /// returns the truth of the statement res1 occurs during res2
        /// </summary>
        /// <param name="res1"></param>
        /// <param name="res2"></param>
        /// <returns></returns>
        public static DarlResult During(DarlResult res1, DarlResult res2)
        {
            if (res1.unknown || res2.unknown)
                return new DarlResult(-1.0, true);
            if (!res1.temporal || !res2.temporal)
                throw new MetaRuleException("passing non temporal parameters to a temporal operator");
            if (res2.values.Count == 1)
                return new DarlResult(0.0, false); //an interval can't be contained in a time point
            var t1 = res1.Quadrify();
            var t2 = res2.Quadrify();
            if ((double)t1.values[0] == (double)t2.values[0] && (double)t1.values[1] == (double)t2.values[1] && (double)t1.values[2] == (double)t2.values[2] && (double)t1.values[3] == (double)t2.values[3])
                return new DarlResult(0.0, false); //during excludes strict equality
            if ((double)t1.values[0] >= (double)t2.values[3] || (double)t1.values[3] <= (double)t2.values[0])
                return new DarlResult(0.0, false); //no overlap
            if ((double)t1.values[0] > (double)t2.values[0] && (double)t1.values[1] > (double)t2.values[1] && (double)t1.values[2] < (double)t2.values[2] && (double)t1.values[3] < (double)t2.values[3])
                return new DarlResult(1.0, false); //res1 is completely during res2
            //The remaining possibilities are for sets where the contained set overlaps the container one end or the other.
            return Intersection(t1, t2);
        }


        /// <summary>
        /// returns the truth of the statement res1 is after res2
        /// </summary>
        /// <param name="res1"></param>
        /// <param name="res2"></param>
        /// <returns></returns>
        public static DarlResult After(DarlResult res1, DarlResult res2)
        {
            if (res1.unknown || res2.unknown)
                return new DarlResult(-1.0, true);
            if (!res1.temporal || !res2.temporal)
                throw new MetaRuleException("passing non temporal parameters to a temporal operator");
            var t1 = res1.Quadrify();
            var t2 = res2.Quadrify();
            if ((double)t1.values[0] > (double)t2.values[3])
                return new DarlResult(1.0, false);
            if ((double)t1.values[1] <= (double)t2.values[2])
                return new DarlResult(0.0, false);
            return !Intersection(t1, t2);
        }

        /// <summary>
        /// returns the truth of the statement res1 is before res2
        /// </summary>
        /// <param name="res1"></param>
        /// <param name="res2"></param>
        /// <returns></returns>
        public static DarlResult Before(DarlResult res1, DarlResult res2)
        {
            if (res1.unknown || res2.unknown)
                return new DarlResult(-1.0, true);
            if (!res1.temporal || !res2.temporal)
                throw new MetaRuleException("passing non temporal parameters to a temporal operator");
            var t1 = res1.Quadrify();
            var t2 = res2.Quadrify();
            if ((double)t1.values[3] < (double)t2.values[0])
                return new DarlResult(1.0, false);
            if ((double)t1.values[2] >= (double)t2.values[1])
                return new DarlResult(0.0, false);
            return !Intersection(t1, t2);
        }

        /// <summary>
        /// returns the truth of the statement res1 overlaps res2
        /// 
        /// </summary>
        /// <param name="res1"></param>
        /// <param name="res2"></param>
        /// <returns></returns>
        public static DarlResult Overlapping(DarlResult res1, DarlResult res2)
        {
            if (res1.unknown || res2.unknown)
                return new DarlResult(-1.0, true);
            if (!res1.temporal || !res2.temporal)
                throw new MetaRuleException("passing non temporal parameters to a temporal operator");
            var t1 = res1.Quadrify();
            var t2 = res2.Quadrify();
            //consider before side
            if ((double)t1.values[1] < (double)t2.values[0] && (double)t1.values[3] > (double)t2.values[1] && (double)t1.values[3] < (double)t2.values[3])
                return new DarlResult(1.0, false);
            //after side
            if ((double)t1.values[1] < (double)t2.values[2] && (double)t1.values[2] > (double)t2.values[2] && (double)t1.values[3] > (double)t2.values[3])
                return new DarlResult(1.0, false);
            if ((double)t1.values[0] == (double)t2.values[0] && (double)t1.values[1] == (double)t2.values[1] && (double)t1.values[2] == (double)t2.values[2] && (double)t1.values[3] == (double)t2.values[3])
                return new DarlResult(0.0, false); //overlapping excludes strict equality
            if ((double)t1.values[0] >= (double)t2.values[3] || (double)t1.values[3] <= (double)t2.values[0])
                return new DarlResult(0.0, false); //no overlap
            var res = new DarlResult(1.0 - ((double)Before(t1, t2).values[0] + (double)After(t1, t2).values[0] + (double)During(t1, t2).values[0]), false);
            if (!res.unknown)
                return res;
            return new DarlResult(0.0, false);
        }

        public static DarlResult Age(DarlResult res1, DarlResult now)
        {
            if (res1.IsUnknown() || now.IsUnknown() || res1.GetWeight() == 0.0 || now.GetWeight() == 0.0 || res1.values.Count == 0 || now.values.Count == 0)
                return new DarlResult(-1.0, true);
            if (!res1.temporal || !now.temporal)
                throw new MetaRuleException("passing non temporal parameters to a temporal operator");
            var duration = new DarlResult("duration", DarlResult.DataType.duration);
            switch (res1.values.Count)
            {
                case 1:
                    return new DarlResult(-1.0, true);
                case 2:
                    if ((double)now.values[0] > (double)res1.values[1])
                        return new DarlResult(-1.0, true);
                    switch (now.values.Count)
                    {
                        case 1:
                            duration.values.Add((double)now.values[0] - (double)res1.values[0]);
                            duration.Normalise(false);
                            return duration;
                        case 2:
                            duration.values.Add((double)now.values[0] - (double)res1.values[0]);
                            duration.values.Add(Math.Min((double)now.values[1] - (double)res1.values[0], (double)res1.values[1] - (double)res1.values[0]));
                            duration.Normalise(false);
                            return duration;
                        case 3:
                            duration.values.Add((double)now.values[0] - (double)res1.values[0]);
                            duration.values.Add(Math.Min((double)now.values[1] - (double)res1.values[0], (double)res1.values[1] - (double)res1.values[0]));
                            duration.values.Add(Math.Min((double)now.values[2] - (double)res1.values[0], (double)res1.values[1] - (double)res1.values[0]));
                            duration.Normalise(false);
                            return duration;
                        case 4:
                            duration.values.Add((double)now.values[0] - (double)res1.values[0]);
                            duration.values.Add(Math.Min((double)now.values[1] - (double)res1.values[0], (double)res1.values[1] - (double)res1.values[0]));
                            duration.values.Add(Math.Min((double)now.values[2] - (double)res1.values[0], (double)res1.values[1] - (double)res1.values[0]));
                            duration.values.Add(Math.Min((double)now.values[3] - (double)res1.values[0], (double)res1.values[1] - (double)res1.values[0]));
                            duration.Normalise(false);
                            return duration;
                        default:
                            return new DarlResult(-1.0, true);
                    }
                case 3:
                    if ((double)now.values[0] > (double)res1.values[2])
                        return new DarlResult(-1.0, true);
                    switch (now.values.Count)
                    {
                        case 1:
                            duration.values.Add((double)now.values[0] - (double)res1.values[0]);
                            duration.values.Add(Math.Min((double)now.values[0] - (double)res1.values[0], (double)res1.values[1] - (double)res1.values[0]));
                            duration.Normalise(false);
                            return duration;
                        case 2:
                            duration.values.Add((double)now.values[0] - (double)res1.values[0]);
                            duration.values.Add(Math.Min((double)now.values[0] - (double)res1.values[0], (double)res1.values[1] - (double)res1.values[0]));
                            duration.values.Add(Math.Min((double)now.values[1] - (double)res1.values[0], (double)res1.values[1] - (double)res1.values[0]));
                            duration.Normalise(false);
                            return duration;
                        case 3:
                        case 4:
                            duration.values.Add((double)now.values[0] - (double)res1.values[0]);
                            duration.values.Add(Math.Min((double)now.values[1] - (double)res1.values[0], (double)res1.values[1] - (double)res1.values[0]));
                            duration.values.Add(Math.Min((double)now.values[2] - (double)res1.values[0], (double)res1.values[1] - (double)res1.values[0]));
                            duration.values.Add(Math.Min((double)now.values[3] - (double)res1.values[0], (double)res1.values[1] - (double)res1.values[0]));
                            duration.Normalise(false);
                            return duration;
                        default:
                            return new DarlResult(-1.0, true);
                    }
                case 4:
                    if ((double)now.values[0] > (double)res1.values[3])
                        return new DarlResult(-1.0, true);
                    switch (now.values.Count)
                    {
                        case 1:
                            duration.values.Add((double)now.values[0] - (double)res1.values[0]);
                            duration.values.Add(Math.Min((double)now.values[1] - (double)res1.values[0], (double)res1.values[1] - (double)res1.values[0]));
                            duration.values.Add(Math.Min((double)now.values[2] - (double)res1.values[0], (double)res1.values[1] - (double)res1.values[0]));
                            duration.Normalise(false);
                            return duration;
                        case 2:
                        case 3:
                        case 4:
                            duration.values.Add((double)now.values[0] - (double)res1.values[0]);
                            duration.values.Add(Math.Min((double)now.values[1] - (double)res1.values[0], (double)res1.values[1] - (double)res1.values[0]));
                            duration.values.Add(Math.Min((double)now.values[2] - (double)res1.values[0], (double)res1.values[1] - (double)res1.values[0]));
                            duration.values.Add(Math.Min((double)now.values[3] - (double)res1.values[0], (double)res1.values[1] - (double)res1.values[0]));
                            duration.Normalise(false);
                            return duration;
                        default:
                            return new DarlResult(-1.0, true);
                    }
            }
            return new DarlResult(-1.0, true);
        }

        private DarlResult DeQuadrify()
        {
            if (values.Count != 4)
                return this;
            var vals = new List<double>();
            foreach(double p in values)
                vals.Add(p);
            vals.Sort();
            values.Clear();
            if(vals[0] == vals[1])
            {
                if(vals[1] == vals[2])
                {
                    if(vals[2] == vals[3])
                    {
                        values.Add(vals[3]);
                    }
                    else
                    {
                        values.Add(vals[2]);
                        values.Add(vals[3]);
                    }
                }
                else
                {
                    values.Add(vals[1]);
                    values.Add(vals[2]);
                    if(vals[2] != vals[3])
                    {
                        values.Add(vals[3]);
                    }
                }
            }
            else
            {
                values.Add(vals[0]);
                if (vals[1] == vals[2])
                {
                    if (vals[2] == vals[3])
                    {
                        values.Add(vals[3]);
                    }
                    else
                    {
                        values.Add(vals[2]);
                        values.Add(vals[3]);
                    }
                }
                else
                {
                    values.Add(vals[1]);
                    values.Add(vals[2]);
                    if (vals[2] != vals[3])
                    {
                        values.Add(vals[3]);
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// returns the truth of the statement res1 equals res2 exactly
        /// 
        /// </summary>
        /// <param name="res1"></param>
        /// <param name="res2"></param>
        /// <returns></returns>
        public static DarlResult TempEqual(DarlResult res1, DarlResult res2)
        {
            if (res1.unknown || res2.unknown)
                return new DarlResult(-1.0, true);
            if (!res1.temporal || !res2.temporal)
                throw new MetaRuleException("passing non temporal parameters to a temporal operator");
            var t1 = res1.Quadrify();
            var t2 = res2.Quadrify();
            if ((double)t1.values[0] == (double)t2.values[0] && (double)t1.values[1] == (double)t2.values[1] && (double)t1.values[2] == (double)t2.values[2] && (double)t1.values[3] == (double)t2.values[3])
                return new DarlResult(1.0, false);
            return new DarlResult(0.0, false);
        }

        /// <summary>
        /// convert a result to its 4 value representation
        /// </summary>
        private DarlResult Quadrify()
        {
            var res = new DarlResult(DataType.temporal, this.weight);
            switch (values.Count)
            {
                default:
                    return this;
                case 1:
                    {
                        res.values.Add(this.values[0]);
                        res.values.Add(this.values[0]);
                        res.values.Add(this.values[0]);
                        res.values.Add(this.values[0]);
                        return res;
                    }
                case 2:
                    res.values.Add(this.values[0]);
                    res.values.Add(this.values[0]);
                    res.values.Add(this.values[1]);
                    res.values.Add(this.values[1]);
                    return res;
                case 3:
                    res.values.Add(this.values[0]);
                    res.values.Add(this.values[1]);
                    res.values.Add(this.values[1]);
                    res.values.Add(this.values[2]);
                    return res;
            }
        }

        /// <summary>
        /// For a pair of temporal results, Find the Y value of the Min intersection of the outer edges.
        /// </summary>
        /// <param name="other">The other result.</param>
        /// <returns></returns>
        /// <remarks>Must both be 4 value representations.
        /// source www.ambrsoft.com/MathCalc/Line/TwoLinesIntersection/TwoLinesIntersection.htm
        /// </remarks>
        public static DarlResult Intersection(DarlResult res1, DarlResult res2)
        {
            var confidenceProd = res1.weight * res2.weight;
            //first left side
            var l = ((double)res1.values[0] - (double)res2.values[0]) * confidenceProd * -1 /
                ((double)res1.values[1] * res2.weight - (double)res1.values[0] * res2.weight - (double)res2.values[1] * res1.weight + (double)res2.values[0] * res1.weight);
            var r = ((double)res1.values[2] - (double)res2.values[2]) * confidenceProd * -1 /
                ((double)res1.values[3] * res2.weight - (double)res1.values[2] * res2.weight - (double)res2.values[3] * res1.weight + (double)res2.values[2] * res1.weight);
            if (double.IsNaN(l) || double.IsNaN(r))
                return new DarlResult(1.0, false);
            if (double.IsNegativeInfinity(l) || double.IsNegativeInfinity(r) || double.IsPositiveInfinity(l) || double.IsPositiveInfinity(r))
                return new DarlResult(0.0, false);
            return new DarlResult(Math.Min(l, 1 - r), false);
        }
        #endregion

    }

    public static class DarlResultExtension
    {
        public static bool Exists(this DarlResult dr)
        {
            return ((object)dr) != null;
        }
    }


}


