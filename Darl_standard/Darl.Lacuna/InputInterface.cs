using System;
using System.Collections;

namespace Darl.Lacuna
{
	/// <summary>
	/// Used to convert values to and from the doubles used by Differential Evolution
	/// </summary>
	internal class InputInterface
	{
		internal enum DataType {numeric,categorical,presence,arity,textual,semantic,temporal};
		internal DataType type;
		internal InputInterface()
		{
			type = DataType.numeric;
		}
		internal InputInterface(string newType)
		{
			type = DataType.numeric;
			if(newType == "numeric")
				type = DataType.numeric;
			else if(newType == "categorical")
			{
				type = DataType.categorical;
				categories = new ArrayList();
			}
			else if(newType == "presence")
				type = DataType.presence;
			else if(newType == "arity")
				type = DataType.arity;
			if(type == DataType.categorical)
			{
				lowerBound = 0.0;
				upperBound = 0.0;
			}

		}
		/// <summary>
		/// Adds a category
		/// </summary>
		/// <param name="category"></param>
		internal void AddCategory(string category)
		{
			if(type != DataType.categorical && type != DataType.presence)
				throw new Exception("InputInterface: Adding categories to a non categorical input.");
			categories.Add(category);
			upperBound += 1.0;
		}
		internal double upperBound;
		internal double lowerBound;
		private ArrayList categories;
		internal string name;
		internal string Convert(double Value)
		{
			switch(type)
			{
				case DataType.numeric:
					return Value.ToString();
				case DataType.categorical:
					return ConvertCategory(Value);
				case DataType.presence:
					if(Value >= 0)
						return "present";
					else
						return String.Empty;
				case DataType.arity:
					return Math.Floor(Value).ToString();
				default:
					return String.Empty;
			}
		}
		internal int categoryCount
		{
			get{return categories.Count;}
		}

		internal string ConvertCategory(double Value)
		{
			int index = (int)Math.Floor(Value);
			if(index >= categories.Count || index < 0)
				throw new Exception("InputInterface.Convert: value" + index.ToString() + " out of bounds.");
			return (string)categories[index];
		}
	}
}
