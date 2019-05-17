---
title: The law of the excluded middle
description: How this logical idea does not apply to open ended circumstances..
output:
  html_document:
    toc: true
    toc_float: true
---


The law of the excluded middle
=================

[Wikipedia](https://en.wikipedia.org/wiki/Law_of_excluded_middle) has a helpful article on this subject. In classical logic either a proposition is true, or its inverse must be true.

This formulation is not used in DARL. Why not?

The fundamental answer is that this is because DARL is concerned with uncertain knowledge. 
DARL operates at two levels. 
At the level of logical evaluation inside a rule DARL uses fuzzy logic, which as a superset of Boolean logic most certainly conforms to the law of the excluded middle. 
At the rule level, however, an individual rule is unlikely to contain all the information determining an output. 
DARL does not have a boolean output type. Instead it treats booleans as examples of categorical types. There are strong reasons for this.

Consider this example: 

A person wants to get from town A, say Buckingham, to town D, say Huntingdon.
One possible route is Buckingham, Milton Keynes(B), Bedford (C), Huntingdon.

So we might say:

if first_town is A, and second_town is B and third_town is C and fourth_town is D then successful_journey will be true;

So, what happens if our driver gets confused and drives from Buckingham to Brackley on the first leg of the journey?

According to the law of the excluded middle we ought to say that successful_journey should be false. In fact DARL will return "unknown". 

Who's right? DARL or classical logic?

In reality there is another route from Brackley to Northampton to Bedford to Huntingdon. Our single rule did not consider all the real world possibilities, so it was not true to say that driving to Brackley made successful_journey false.

In the general case unless we know that our rules cover all the possibilities, the correct answer, if no rule fires for a given output, is "unknown".


