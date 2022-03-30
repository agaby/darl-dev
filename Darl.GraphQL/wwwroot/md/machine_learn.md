
# Machine Learning of Knowledge Graphs

You can machine learn Knowledge Graphs directly from data. In order to do so, you have to supply that data, and a description of the data items and how to extract them from the data.
Machine learning data is typically composed of a series of examples. We'll call these patterns. You can load XML, Json or CSV data containing patterns.

You can preload the data, pattern by pattern, or load an entire set of examples at training time. In each case, the system will need to know how to extract patterns, and how to find the individual data items, and what type to apply to them.

There is a repository of Machine learning models you can use with ThinkBase at [https://github.com/thinkbase-ai/ml_examples](https://github.com/thinkbase-ai/ml_examples).

This repository contains a set of examples of pre-coded machine learning data sets and data descriptions.

They are written in the GraphQL query language, a cut-down form of Json, and can be pasted into the GraphQL plaground UI you can find at [https://darl.dev/api](https://darl.dev/api)

See the video:

[![Machine learning for creating Knowledge Graphs with ThinkBase](https://img.youtube.com/vi/DF-V9PCvqHM/1.jpg)](https://www.youtube.com/watch?v=DF-V9PCvqHM)


Although these examples all run quickly, machine learning can be time consuming. The machine learning interface is therefore provided as a GraphQL subscription. 
You start the subscription by supplying the GraphQL source, and the system notifies you when the process is complete.

These examples can simply be pasted into the GraphQL playground interface.

## Extracting patterns.
Tree shaped data representations, like XML and Json can encode patterns in a variety of ways.  In order to extract the data, a path is required to identify each pattern, and a separate, relative path is required to locate each data item relative to the patterns. 
ThinkBase uses XPath for XML and JPath for Json to describe these paths. 
For CSV data it is assumed that each row contains a pattern, so the patternpath is not needed, and that a header is present that can be used to identify each column.
The relative paths are therefore required to match these column names.

## An example script

The following is an example script for running the build command, that will create a new KG called "iris.graph" from the data provided, using the data items specified.

```json
subscription
{
	build(
		name: "iris.graph"
		data: "<Data goes here in XML, Json or CSV>"
		patternPath: "/irisdata/Iris"
		dataMaps:
		[
			{
				objId: "sepal_length"
				dataType: NUMERIC
				relPath: "sepal_length"
				objectLineage: "appraisal"
			}
		 {
				objId: "sepal_width"
				dataType: NUMERIC
				relPath: "sepal_width"
				objectLineage: "appraisal"
			}
		 {
				objId: "petal_length"
				dataType: NUMERIC
				relPath: "petal_length"
				objectLineage: "appraisal"
			}
			{
				objId: "petal_width"
				dataType: NUMERIC
				relPath: "petal_width"
				objectLineage: "appraisal"
			}
			{
				objId: "class"
				dataType: CATEGORICAL
				relPath: "class"
				objectLineage: "noun:00,1,00,1,0,06,26,18,0,0+noun:01,2,04,2,21"
				target: true
			}
	]
	)
	{
		trainPerformance
		testPerformance
	}
}
```
