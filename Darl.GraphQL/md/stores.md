---
title: Stores in the bot system
description: How Stores work in the DARL Bot System.
output:
  html_document:
    toc: true
    toc_float: true
---


Stores in the bot system
===

# Stores
Stores are general mechanisms for getting data into and out of a ruleset.
The act as a "lookup" mechanism, and can be used to call external rulesets or log results.

A Store has the general format:

```darl
<store name>[<list of parameters>]
```

where the list of parameters resolve to a list of strings.

The bot system contains the following stores:

|Name |Usage | Read/Write |
----|----|----|
|UserData |Bot user data collected by the Bot Framework |Read/Write|
|ConversationData |Bot conversation data collected by the Bot Framework |Read/Write |
|PrivateConversationData |Bot private conversation data collected by the Bot Framework |Read/Write |
|Bot |Constants, like the name of the bot or your website set through bot model editing. |Read only |
|Value |Values collected through the current text sequence |Read only |
|Call |Interface to call a ruleset |Write only |
|Word |Gets a word definition from WordNet |Read only |
|Rest |Calls a remote REST interface if secured or current user has access |Read/Write |
|Collateral |Gets a predefined piece of MarkDown and returns it |Read only|

## Value Store

The value store accesses values embedded in chat strings. Each value is a pair of the identifier for the kind of value, and the value itself.
Identifiers are those defined in (). They start with the text _value:_ and they define the kind of value required in a hierarchical fashion like other lineages.
To extract a value of a particular kind from the values extracted from the string use that indicator.
For instance
```darl
Value["value:text"]
```

returns the first text value extracted from the string.

### Multiple values of the same kind.
If you want to access anything other than the first value of a type that is a kind of the type specified, you can supply a second integer parameter.
That integer represents the zero-based index of the value sought.
So, for instance to access the second text value in a chat string use:
```darl
Value["value:text",1]
```
