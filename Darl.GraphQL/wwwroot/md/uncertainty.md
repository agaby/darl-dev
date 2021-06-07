# Uncertainty
A moment's thought would tell you that the principle  concern of AI is uncertainty.

Nobody would use AI for a problem if an analytical solution existed with tractable characteristics.

The results of machine learning inferences are always uncertain. 

We live in an analog world, and ignore the fact that all machine computation is effectively integer arithmetic.

The young, bought up in a digital world, imagine that digital models can be indistinguishable from reality.

We assume that the differences between reality and a 64 bit float representation will always be negligible. There are many cicumstance where this is not true, especially where modeled systems exhibit Chaos.

All measurements from the analog world contain a tolerance. Mechanical engineers and those computer scientists old enough to have used an analog computer are very aware of this.

ThinkBase and DARL give you mechanisms for representing these uncertainties and ensure that all processing, both numeric and logical, handles and carries through uncertainty to the results.

## Use of [Fuzzy Logic](fuzzy_sets.md)

DARL and ThinkBase use fuzzy logic and fuzzy arithmetic. Each statement (rule) can have a 0-1 certainty value attached. 

Each categorical input can contain one or more categories, each annotated with a certainty figure.

Each numeric or temporal input can be defined as a fuzzy number or time, using our representational methodology.

All operators in DARL, barring those that process text, can correctly pass through these uncertainties correctly calculated according to the rules of fuzzy logic and fuzzy arithmetic.

## use of [DarlVar](DarlVar.md)
 
ThinkBase's uncertainty handling is best accessed programmatically.
Our GraphQL interface uses the DarlVar structure for variables, and DarlVars are designed to carry a single data item of all the supported types, containing uncertainty information where available.
For instance, for an inferred numeric output, the double array _values_ will contain the fuzzy number representing the inferred value.
In each case, the central most plausible value is represented as a string in the _value_ property.
 

 

