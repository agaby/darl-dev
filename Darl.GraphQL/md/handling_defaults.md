---
title: Handling Defaults
description: How DARL handles defaults.
output:
  html_document:
    toc: true
    toc_float: true
---


Handling Defaults
====

In DARL, inputs and outputs are in an "unknown" state until something sets them to another state. In that other state they can have various amounts of uncertainty, but they are considered "known".

The Questionnaire engine takes advantage of this by seeking to make all the outputs known during a questionnaire run. As you add new knowledge by answering questions the engine continually tracks back from those outputs that are still undefined to see what inputs must be made known  in order to satisfy all the outputs.
A corollary of this is that for an output that is categorical or numeric, there normally needs to be at least one rule per category, and possibly per set.

It makes life considerably easier sometimes if you can have a default state that a given output goes into when no other rules fire. 

However, a default state means that whatever the inputs, an output with a default state will be known, so the questionnaire engine won't bother to ask any questions to determine that state.
So this is fine for a straight through inference usage, but problematic for questionnaires.

DARL has the _otherwise_ operator to handle this set of circumstances.

A rule starting with _otherwise if_ rather than _if_ is grammatically identical in all other respects to any other rule, but this rule is held in reserve to fire if no other rules fire for the controlling output.

The inference engine permits only one _otherwise if_ rule per output.

So if we consider this simple rule set:
```darl
ruleset simple
{
    input categorical a {true,false};
    input categorical b {true, false};

    output categorical c {true, false};

    if a is true and b is true then c will be true;
    otherwise if anything then c will be false;
}
```
This will work well with the inference engine, but with the questionnaire engine the value of __c__ will always be false and the questionnaire engine won't ask any questions.

If you wanted to set up the questionnaire engine to run the same simple rule set you should use the following DARL:
```darl
ruleset simple_questionnaire_
{
    input categorical a {true,false};
    input categorical b {true, false};

    output categorical c {true, false};

    if a is true and b is true then c will be true;
    otherwise if a is present and b is present then c will be false;
}
```
If you use this rule set in the questionnaire engine it will prompt for __a__ and __b__.
This is because the _otherwise if_ rule is only satisfied if __a__ and __b__ are present, i.e. _known_.

You can have any number of rules associated with an output, but you only ever need on _otherwise if_ rule.
It is up to you to determine which inputs or outputs must be known before the _otherwise if_ rule triggers. 

See [Excluded middle](./Excluded_middle) for background.




