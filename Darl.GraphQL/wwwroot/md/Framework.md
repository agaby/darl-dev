---
title: The DARL Framework
description: Putting DARL into Bot responses.
output:
  html_document:
    toc: true
    toc_float: true
---

The DARL Framework
====
Each response to a detected phrase can include DARL code.
This code is just a fragment, containing one or more rules, and does not need to contain a ruleset definition, etc.

The items here are used to compose the skeleton that is used to wrap these fragments of DARL.
The wrapper is composed of this code and a constructed set of inputs, outputs, stores, etc constructed from the elements on this page.

# The ruleset framework
This contains the skeleton for the ruleset. Unlike the rule sets you create to embody knowledge, where you are required to define the inputs and outputs, the I/O is added to the framework automatically.
This means that the actual skeleton used to wrap the DARL fragments associated with each phrase consists of this skeleton, plus everything in the I/O definitions.

The comment 
```darl
/%% rule_insertion_point %%/ 
```
locates the place where everything is inserted. 

# Model settings
Every bot has the need for constants that are defined in one place, and potentially accessed in many. 
The model settings contain name-value pairs, where the values can have all the usual types.
Each name should be unique.
To access these values in a DARL fragment use the "Bot" store.

```darl
if anything then response will be Bot["name"];
```

# I/O definitions
This is where you define the inputs, outputs, stores and constants used by the DARL fragments.
When you create a new model a standard set of these are loaded too.
In general you won't have to change these. The standard outputs, response and link, contain the textual response sent back to the user and any links to external references respectively.
The Stores contains the standard set of stores built into the Bot.

# save changes
Selecting this saves the changes to the local copy of the Bot model. As usual to modify the stored versions used by your bots you must save the entire model from the top bar.
