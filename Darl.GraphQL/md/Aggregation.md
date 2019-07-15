---
title: Output aggregation
description: How DARL language outputs are aggregated.
output:
  html_document:
    toc: true
    toc_float: true
---
Output aggregation
==============

DARL is a language that takes uncertainty very seriously, since it is intended to infer results in circumstances where inputs and rules bear varying amounts of uncertainty.

The various run time tools for DARL ultimately output [DarlVar](DarlVar) data structures capable of containing multiple forms of uncertainty for each output. The name parameter will contain the output name.
In each case the Value parameter contains a crisp, central value. The uncertainty information is held in other places, depending on the type.

The three output types all aggregate data in different ways:

# Numeric outputs

In the worst case, the DARL inference engine must aggregate multiple fuzzy sets, each with associated confidence values, to create a single crisp Value parameter and a fuzzy set representing the uncertainty.
The central value is determined using the [Center of Gravity method](https://en.wikipedia.org/wiki/Defuzzification).
The Values parameter contains a sequence of doubles delineating the fuzzy uncertainty associated with the result in standard [fuzzy set](fuzzy sets) format.

# Categorical outputs

Again in the worst case the rules may have caused multiple categories to be generated with different confidence levels.
The Value parameter will contain the category name with the highest confidence. Where there is a tie, the category will be that produced by the rule that appears first in source file order.
The Categories parameter will contain a dictionary of name,double pairs containing the alternate categories and their respective confidences.

# Textual outputs

It is entirely possible that more than one rule will fire that controls a textual output. The texts are ordered according to confidence, and the Value parameter will hold the text with the highest confidence. Where there is a tie, the texts will be concatenated, separated by carriage returns if no other whitespace is present. The order will be that of the generating rules in the source document.
