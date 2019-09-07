---
title: The Darl Language
description: The DARL programming language syntax.
output:
  html_document:
    toc: true
    toc_float: true
---



The Darl Language
============

'Darl' stands for **D**octor **A**ndy's **R**ule **L**anguage.
It is pronounced like the name *'Daryl'*.

# Introduction
The DARL language is intended to permit both software tools and humans to record and re-use knowledge in the form of "if..then" rules.
The language is simple and intuitive. Given the appropriate tooling, non-programmers should be able to use it.

# Primitive elements
The following primitive elements are defined in DARL.

## Identifiers
Identifiers are names used to identify inputs, outputs, constants, strings, rulesets, mapinputs and mapoutputs.
They follow the convention of C#, namely: 

+ They cannot start with a number
+ They cannot contain spaces
+ They are case sensitive
+ They can only contain letters, numbers and the "_" character.

## Numeric literals
These follow C#, so they can be integers or floating point numbers with optional exponent.

### Infinity
Unbounded sets are denoted by positive or negative infinity as the first or last value. Darl recognizes _Infinity_, _-Infinity_, _-∞_ and _∞_.

## String literals
These are pieces of text delineated with the " or ' characters.

## Terminators
These are characters that delineate different sections of DARL. they are:

+   ; - marking the end of a rule or definition
+   { } - marking the beginning and end of a block - used in set and ruleset definitions 
+   ( ) - marking the beginning and end of a list in functions
+   , - separating elements of a list

## Keywords
These are pieces of text that have specific meaning in the DARL language. they are:
            
"{", "}", "(", ")", ",", ";", ".", "if", "then", "will", "be", "confidence", "input", "output", "numeric", "categorical","arity","presence","string","constant", "or", "and","not","is","*","/","-","+","%","^",";" ">","<",">=","<=","anything","textual", "maximum","minimum","sum","product","fuzzytuple","sigmoid","normprob","round","ruleset","wire","mapinput","mapoutput","pattern"

Keywords are always lower case.

## Composite identifiers
When connecting to inputs or outputs of a ruleset it is necessary to use the name of the ruleset and the input or output name.
This is achieved by concatenating the ruleset name and the i/o name with a "." separator.
```darl           
rulesetname.inputname
```

## Comments
Comments are freely permitted in code. They follow the C#/Java form:
```darl
//single line comment
```

```darl
/*bounded  comment */
```

### Insertion comments
In order to support templating, darl permits special comments that represent a site to be replaced by a named value.
```darl
%% named.value %%
```

# Top level elements
These elements are concerned with inputs and outputs at the highest level, and the definition of rulesets.
Darl permits the creation of multiple rule sets within a file, like functional blocks in a schematic diagram, and these can be wired to "connector" elements that act as inputs and outputs for the aggregate document, called a *map*. 
If a darl source file contains only a single ruleset, then these input output and wire definitions can be left out, and the runtime will automatically generate these elements.

## Mapinputs
These define the inputs at the top or "map" level.
These will accept external values during the machine learning or inference process.
They take two parameters, the first, which is mandatory, is an identifier naming the mapinput. The second optional parameter is a string containing navigation text for machine learning.

```darl
mapinput fred "/fred/text()"
```
## Mapoutputs
These define the outputs at the top or "map" level.
These will generate values during the machine learning or inference process.
They take two parameters, the first, which is mandatory, is an identifier naming the mapoutput. The second optional parameter is a string containing navigation text for machine learning.

```darl
mapoutput bill "/bill/text()"
```

## Wires
These connect up the various elements at the top level.
They take two parameters, the first is an identifier naming the source, the second parameter is an identifier naming the destination. Both may be composite identifiers
Sources can be mapinputs or ruleset outputs. Destinations may be mapoutputs or ruleset inputs.

```darl
wire input1 ruleset1.input1
```
            
```darl
wire ruleset2.output1 ruleset3.input2
```

The DARL parser checks for impossible combinations, such as connecting two ruleset outputs.
The on-line DARL designer makes these checks dynamically.
A further constraint is that connections between rulesets are only possible where the data types match. 

## Rulesets
These define the block of code corresponding to a ruleset.
A ruleset is a unit of processing, with an analogy to the class in conventional programming.
They take two parameters, the first, which is mandatory, is an identifier naming the ruleset, the second optional parameter is a keyword identifying if the ruleset will be subject to machine learning.
Choices here are:

+ manual - The ruleset contents will be created by hand - default, not required.
+ supervised - The ruleset will be created by supervised machine learning
+ unsupervised - The ruleset will be created by unsupervised learning
+ reinforcement - The ruleset will be created by reinforcement learning

```darl
ruleset ruleset1 supervised 
{ 
//ruleset content 
}
```

## The pattern
When the contents of one or more rule sets are to be set by machine learning, then a data source must be connected to the DARL file.
Machine learning is based on learning associations between or within inputs and/or outputs. Generally these data values are presented as a series of patterns. 
The optional pattern element permits you to define how to find those patterns in the language appropriate to the data source. This might be XPath for an XML source, or SQL for a database.
There should only be one pattern element in a DARL source file.
Patterns take one parameter, the string containing the navigation text to identify the pattern.

```darl
pattern "//pattern"
```
# Ruleset level elements
These elements are concerned with inputs and outputs within a ruleset and the contents of the rules.

## inputs
These define the inputs at the ruleset level.
These will accept external values during the machine learning or inference process.
They take several parameters, the first, which is mandatory, is the data type of the input, the second also mandatory parameter is an identifier naming the input.
Choices here are:

+ numeric - the input value will be a number.
+ categorical - the input value will be text falling into one of a set of categories.
+ textual - the input value will be a string.
+ temporal = the input value will be a date and time.

```darl
input numeric fred; 
```
Numeric inputs can optionally contain a series of fuzzy set definitions, while categorical inputs can have a series of categories.
## Fuzzy set definitions
Fuzzy sets require an identifier to name the set and between 1 and 4 numeric literals, in ascending order, to define the set.

```darl
input numeric fred {{small, 1, 2, 3},{medium, 2, 3, 4},{large, 3, 4, 5}};
```

Inputs can have any number of fuzzy sets, though each set name must be unique. Ideally the ruleset writer will choose meaningful names and ensure that the set ranges match the names.
So, as above, the medium set ought to be greater than the small set. This is an element of style that the DARL parser cannot enforce.
If machine learning of a ruleset is employed, fuzzy sets will be generated automatically from the data.

## Category definitions
Categorical variables are very common. They are multi-choice variables, like male/female or single/married/separated/divorced.
 A category definition is a sequence of categories, that can be either identifiers or string literals.

```darl
input categorical fred {true,false};
```
If machine learning of a ruleset is employed, categories will be generated automatically from the data.

## Outputs
These define the outputs at the ruleset level.
These will generate values during the machine learning or inference process.
They take several parameters, the first, which is mandatory, is the data type of the output, the second also mandatory parameter is an identifier naming the input.
Choices here are:

+ numeric - the output value will be a number.
+ categorical - the output value will be text falling into one of a set of categories.
+ textual - the output value will be a textual string
+ temporal - the output value will be a date and time.
```darl
output numeric fred; 
```
Numeric inputs can optionally contain a series of fuzzy set definitions, while categorical inputs can have a series of categories.

## Fuzzy set definitions
Fuzzy sets require an identifier to name the set and between 1 and 4 numeric literals, in ascending order, to define the set.
```darl
output numeric fred {{small, 1, 2, 3},{medium, 2, 3, 4},{large, 3, 4, 5}};
```

Outputs can have any number of fuzzy sets, though each set name must be unique. Ideally the ruleset writer will choose meaningful names and ensure that the set ranges match the names.
So, as above, the medium set ought to be greater than the small set. This is an element of style that the DARL parser cannot enforce.
If machine learning of a ruleset is employed, fuzzy sets will be generated automatically from the data.

## Category definitions
Categorical variables are very common. They are multi-choice variables, like male/female or single/married/separated/divorced.
A category definition is a sequence of categories, that can be either identifiers or string literals.

```darl
output categorical fred {true,false};
```

If machine learning of a ruleset is employed, categories will be generated automatically from the data.

## Constants
This permits the definition of numeric constants.
Since constants are frequently re-used and it is better practice to keep them in one place, DARL prefers to avoid the use of numeric literals inside rules. Instead you should define a numeric constant and use the name of that constant.
A constant has two parameters, the identifier naming the constant and a numeric literal.

```darl
constant Age_related_income_limit 24000;
```

## String Constants

This permits the definition of string constants.
Since constants are frequently re-used and it is better practice to keep them in one place, DARL does allow the use of string literals inside rules. However you should define a string constant and use the name of that constant.
A string constant has two parameters, the identifier naming the constant and a string literal.

```darl
string regex1 "*A|B";
```

## Sequence Constants
This permits the definition of sequence constants.
Since constants are frequently re-used and it is better practice to keep them in one place, DARL does not allow the use of sequence literals inside rules. Instead you should define a sequence constant and use the name of that constant.
A sequence constant has two parameters, the identifier naming the constant and a sequence literal. The latter defines a sequence of string literals and/or lists of string literals.

```darl
sequence seq1 {"fred",{"jane","samantha"},"bill"};
```
The above is interpreted as a sequence of literals as, "fred", followed by "jane" or "samantha", followed by "bill".
Sequences are an extension to DARL used in DAPL.

## Duration constants
This permits the definition of time offsets.
Since constants are frequently re-used and it is better practice to keep them in one place, DARL does not allow the use of duration literals inside rules. Instead you should define a duration constant and use the name of that constant.

```darl
duration trial_duration 30.00:00:0.0;
```

The format of a duration definition is <days>.<hours>:<minutes>:<seconds> where days, hours and minutes are integers and seconds ids a real number.
Examples are:

```
duration trial_duration 30.00:00:0.0; // 30 days
duration trial_duration 0.02:00:0.0; // 2 hours
duration trial_duration 0.00:55:30.5; // 55 minutes and 30.5 seconds
```

# Rule level elements
Part of the flexibility of DARL is the ability to represent many kinds of relationships with one structure
The fundamental structure of a rule is as follows:

```darl
if < conditional expression > then < output identifier > will be < RHS expression > < optional confidence value >;
```

Since outputs can be numeric, categorical, textual or temporal, the RHS expression syntax depends on the type of the output. Choices are:

+ Categorical output: The RHS can only be a category defined in the list of categories for that output or a _categoryof_ expression
+ Numerical output: The RHS can be a fuzzy set defined in the list of sets for that output
+ Numerical output: The RHS can be a numeric expression that is evaluated dynamically.
+ Textual output: the RHS can be a text string, string constant  or a _document_ expression/
+ temporal output the RHS can be a temporal expression.

## Conditional Expressions
This is a fuzzy or Boolean logic expression. The degree of truth associated with it as it is evaluated is used to determine the rules precedence against other rules containing the same output identifier.
There are several logical operators that may be used at the top level:

+ anything: If used this must be the only operator in the top-level conditional expression and always evaluates to truth 1.0.
```darl
if anything then a will be b;
```
+ and: gives the fuzzy logic "and" of the operands either side, implemented as the minimum of their degrees of truth.
```darl
if a is b and c is d then f will be q;
```
+ or: gives the fuzzy logic "or" of the operands either side, implemented as the maximum of their degrees of truth.
```darl
if a is b or c is d then f will be q;
```
+ not: gives the fuzzy logic "not" of the single operand following, implemented as 1 - its degrees of truth.
```darl
if not a is b then f will be q;
```
+ is: evaluates the input's or output's fuzzy value on the left hand side against the expression on the right hand side.
```darl
if a is b then f will be q;
```
Since the input or output could be numeric, categorical or textual (inputs only) there are several possible combinations.

+ The input or output is categorical, the RHS can only be a category defined for that input or output.
```darl
if a is false then b will be true;
```
+ The input or output is numeric, the RHS can be a set defined for that input or output.
```darl
if a is large then b will be true;
```
+ The input or output is numeric, the RHS can be a comparison operation followed by a numeric expression.
```darl
if a is < c + d then b will be true;
```
+ Comparison operators can be >, <, >=, <=, =, !=, interpreted as greater than, less than, greater than or equal to, less than or equal to, equal to and not equal to respectively.
+ The input is textual, the RHS can only be a textual comparison operation.
```darl
if a is match(string) then b will be true;
```
## Numeric Expressions
These are algebraic expressions, following the rules of Fuzzy Arithmetic. See [Introduction to Fuzzy Arithmetic](http://www.amazon.com/Introduction-fuzzy-arithmetic-applications-engineering/dp/0442230079/)
            
The operands can only be numeric inputs or outputs, numeric constants, built in functions or other numeric operators.
The operators available are:

+ +: Addition.
```darl
if a is = b + c then d will be true;
```
+ -: Subtraction.
```darl
if a is = b - c then d will be true;
```
+ *: Multiplication.
```darl
if a is = b * c then d will be true;
```
+ /: Division.
```darl
if a is = b / c then d will be true;
```
+ ^: Power.
```darl
if a is = b ^ c then d will be true;
```
+ %: Modulo.
```darl
if a is = b % c then d will be true;
```

## Temporal expressions
There are a limited set of things that you can logically do with times. For instance, adding two dates is meaningless.
Temporal expressions contain either temporal inputs and outputs or simple additive and subtractive expressions involving the foregoing and _duration_ constants.
The comparison operators >,<,>=,<=,= are available to make decisions based on time.

```darl
input temporal time_now;
input temporal start_time;
output categorical status {running,expired};
duration delay 0.00:00:30.0 //30 seconds
if time_now > start_time + delay then status will be expired;
otherwise if anything status will be running;
```

```darl
input temporal contract_start;
output temporal contract_end;
duration contract_length 30.00:00:0.0; //30 days

if anything then contract_end will be contract_start + contract_length;
```

## Temporal helpers

### Timerange
Is the temporal equivalent of __FuzzyTuple__. A Fuzzy number can be constructed from temporal singletons. For instance:
```Darl
if anything then fuzzy_time will be timerange(start_time, end_time);
```

### Now
 
This returns the current UTC time as a temporal value

```darl
duration sixyears 2190.00:00:00.0;
if anything then qualifyingDate will be now - sixyears; 
```

## Temporal operators
You can compare temporal expressions with temporal operators, that act somewhat like comparisons for numeric expressions.
Creating a set of these is a subject of some academic debate for fuzzy logic.
We follow the general ideas of 
__Allen j (1983):
Maintaining knowledge about temporal intervals.
In communications of the ACM 26 (11), pp. 832–843.__
We have fuzzified a subset of these operators that function as expected with singletons, intervals,triangular and trapezoidal sets.
These are arranged to be orthogonal; i.e. a given pair of fuzzy times the sum of the truths over the set of operators will equal one, and at the most two operators will return degrees of truth > 1.


The operators are:

### During
Compares two fuzzy times to calculate the degree the left element occurs during the right element.

```darl
if event_time is during timerange(start_time, end_time) then alarm will be true;
```
### Before
Compares two fuzzy times to calculate the degree the left element occurs before the right element.
```darl
if event_time is before timerange(start_time, end_time) then alarm will be false;
```

### After
Compares two fuzzy times to calculate the degree the left element occurs after the right element.
```darl
if event_time is after timerange(start_time, end_time) then alarm will be true;
```

### Overlapping
Compares two fuzzy times to calculate the degree the left element overlaps the right element.
```darl
if event_time is overlapping timerange(start_time, end_time) then alarm will be true;
```





## Built in functions
For built in functions, the parameters are separated by commas and can each be numeric expressions.
Built in functions are:

+ sum: The sum of a set of values.
```darl
if a is = sum(b,c,d) then p will be q;
```
+ product: The product of a set of values.
```darl
if a is = product(b,c,d) then p will be q;
```
+ maximum: The greatest of a set of values.
```darl
if a is = maximum(b,c,d) then p will be q;
```
+ minimum: The smallest of a set of values.
```darl
if a is = minimum(b,c,d) then p will be q;
```
+ normprob: The Gaussian probability of a normalized value (average = 0, SD = 1).
```darl
if a is = normprob(b) then p will be q;
```
*Note a single operand.*

+ round: the first operand is rounded to the accuracy set by the 2nd. Non-fuzzy
```darl
if a is = round(b,c) then p will be q;
```
*Note only two operands.*

+ document: for textual i/o this formats a set of values into a template string. see [text replacement](./text_replacement)

+ randomtext: Used principally for bots; this will return one of the choices given at random.

```darl
if anything then response will be randomtext("answer one","answer two","answer three");
```

+ categoryof: sometimes you will have categorical inputs and outputs where the categories are the same or overlap. This enables you to copy categories over that are valid in both of the I/Os.

```darl
input categorical fred {true,false};
output categorical bill {true, false};

if anything then bill will be categoryof(fred);
```



## Optional Confidence Value
Rules can have an optional confidence value, in the range 0.0 to 1.0.
If no confidence is specified, the default confidence is 1.0.
The confidence value corresponds to the maximum degree of truth associated with that rule.
For rule sets created via machine learning, this is associated with the support the data gives to this rule.
For rules created by hand, you can create default behavior, i.e. you can specify a rule that fires if others don't, by defining a rule with low confidence.  

```darl
if anything then b will be false confidence 0.5;
```

## Default handling
During the inference process the rules are grouped by the output they govern. It may well be that none of the rules have > 0 confidence, and so the relevant output is marked as _unknown_.
This is the correct functionality. See [Excluded middle](./excluded_middle) for more.
However the coder may well want a particular output to have a defined value for every circumstance, and this leads to the complicated process of creating rules that cover the states not covered by the existing rules.

The solution is to use another form of the ruleset which is:

```darl
otherwise if anything then b will be true;
```
There should be only one _otherwise_ rule per output. The otherwise rule is triggered only if no other rules fire for that output, and thus provides default functionality.

## Stores
There are circumstances where defining inputs and outputs in a ruleset is unwieldy.
DARL has the _stores_ functionality to handle data input and output within a ruleset, if the environment the inference engine is running in supports stores.
The stores are defined like inputs and outputs, but the environment must support the store specified or a runtime exception will be generated.

Stores can be sinks or sources of information, or both. For instance, when using DARL within the Bot Development System a store is defined containing the local data stored during the conversation. Such a store can be treated as an output and an input. 

A store can have a sequence of parameters that are specific to that store, where the parameters must be provided in the order expected.
Parameters can be string constants, strings, textual outputs or other stores.

A store has the general format:

```darl
<store name>[<list of parameters>]
```

A store definition is just:

```darl
store <store name>;
```

If an environment, such as the BDS supports a store called _UserData_ which is implemented as a name/value pair storage facility, the following code will copy data from one name/value pair to another, where the names are sink and source.

```darl
ruleset storedemo
{
    store UserData;
    if anything then UserData["sink"] will be UserData["source"];
}

```

Stores can be read-only or write-only, in which case reading or writing the wrong way round will not create a syntax error, but will have no effect.
A read to a store that is write-only, or a read of a data item that does not exist will return an _unknown_ value.



 
