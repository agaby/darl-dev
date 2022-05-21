# Knowledge and Ignorance
The DARL language is unique in that its central concern is to represent what is known and unknown about a particular data item, and thus an inference based on a set of data items with varying certainty.

Using Possibility theory, DARL can represent fuzzy numbers, fuzzy categorical variables, fuzzy times and degrees of truth.

The GraphAttribute class and the internal class DarlVar are set up to handle varying degrees of unknowing->knowing

|Type|||
|---|---|---|
|1  |Datatype set|All that is known is the datatype|
|2	|Categories enumerated; numeric ranges/sets defined	|We know the values the input may hold but not the actual values|
|3	|More than one category set or a fuzzy numeric value	|The value is known within a range possibly with a density function|
|4	|A single category or single numeric value	|The exact value/state is known|


DARL treats times and durations as fuzzy values with the same characteristics as numeric values. 

Booleans in DARL are just seen as a special case of categorical variables. See, however, the comments on [the excluded middle](Excluded_middle.md).


Orthogonal to this definition of uncertainty, each GraphAttribute / DarlVar contains a confidence value.  This is scaled from 0-1 with one representing complete plausibility, and 0 representing total implausibility.


This can be thought of as the degree of truth associated with the assertion that a given variable’s value accurately represents the real-world value it models.


Inside DARL inference processing, these two forms of confidence are handled individually and separately. So, it is entirely possible to get a result from DARL that is numerically crisp, i.e., a single value but with a low confidence, or a range of possible categorical values, with high certainty.

When specifying a GraphAttribute for use in a knowledge Graph it should fall into type 2, though type 1 is permissible.

In the knowledge graph inference process, there may be a running chat conversation. Where a variable is encountered of type 1 or 2 , and that value is necessary to continue inference, the system will attempt to elicit the value from the conversation. In a simple Knowledge graph evaluation the value will be treated as unknown and will affect the inference process accordingly.
