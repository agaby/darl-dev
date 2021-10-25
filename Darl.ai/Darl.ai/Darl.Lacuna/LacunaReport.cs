using System;
using System.Reflection;
using System.Text;

namespace Darl.Lacuna
{
	/// <summary>
	/// Used to report lacunae in Rulesets
	/// </summary>
    [Serializable]
	public class LacunaReport
	{
		public DateTime date;
		public string generator = Assembly.GetExecutingAssembly().FullName;
		public OutputReport [] outputs;
        public string fileName;

        public override string ToString()
        {
            StringBuilder build = new StringBuilder();
            string newline = "\n";
            string tab = "    ";
            build.Append(" Lacuna Rule Set Analysis, ");
            build.Append(date.ToString() + newline);
            build.Append(" Rule set name = ");
            build.Append(fileName + newline);
            string[] genstrings = generator.Split(new char[] { ' ' });
            build.Append(" Generator ");
            build.Append(genstrings[0]);
            build.Append(" " + genstrings[1] + newline);
            foreach (OutputReport output in outputs)
            {
                build.Append(" Output: " + output.name + newline);
                if (output.lacunae.Length == 0)
                    build.Append(tab + "No Lacunae found" + newline);
                else
                {
                    foreach (SingleLacuna lacuna in output.lacunae)
                    {
                        build.Append(tab + "Lacuna found, minimum confidence: " + lacuna.minimumConfidence.ToString("0.00") + newline);
                        foreach (LacunaInput input in lacuna.inputs)
                        {
                            if (input.inputType == "categorical" || input.inputType == "presence")
                                build.Append(tab + tab + "Input " + input.name + " category: " + input.category + newline);
                            else
                                build.Append(tab + tab + "Input " + input.name + " lower bound: " + input.minValue + ", upper bound: " + input.maxValue + newline);
                        }
                    }
                }
            }
            return build.ToString();
        }
	}
	/// <summary>
	/// A report for a given output
	/// </summary>
    [Serializable]
	public class OutputReport
	{
		public string name;
		public SingleLacuna [] lacunae;
	}
	/// <summary>
	/// A report on a lacuna
	/// </summary>
    [Serializable]
	public class SingleLacuna
	{
		public LacunaInput[] inputs;
		public double minimumConfidence;
		public int examples;
	}
	/// <summary>
	/// Details for that individual input
	/// </summary>
    [Serializable]
	public class LacunaInput
	{
		/// <summary>
		/// Lower bound of the numeric lacuna
		/// </summary>
		public string minValue;
		/// <summary>
		/// upper bound of the numeric lacuna
		/// </summary>
		public string maxValue;
		/// <summary>
		/// Name opf the input
		/// </summary>
		public string name;
		/// <summary>
		/// lacuna category if categorical
		/// </summary>
		public string category;
		/// <summary>
		/// Textual version of type of input
		/// </summary>
		public string inputType;
	}

}
