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
Value["value:text","1"]
```
Note that all parameters to a store are strings.

## Graph store

The graph store accesses a graph database built into Darl.Dev. This is configured as a knowledge store and is accessible to all users.
The graph store makes great use of lineages, and all objects stored in the knowledge stores are annotated with the lineages of the kind of object they are.
For instance people are annotated as
_noun:00,2,00_
and organizations are annotated as:
_noun:01,2,07,10_
This is to permit the system to reason and draw inferences based on the kind of objects referenced.

There are four operations defined on the Graph store that you can use. 
In each case if an object of a particular kind is not found a Darl value of _unknown_ is returned:


### Attribute

This returns attributes that are a kind of the given lineage associated with the object.
```darl
output textual val;
if anything then val will be Value["value:text"];
if anything then response will be Graph["attribute",val,"noun:00,2,00","noun:01,4,09,01,3,4,5"]
```
This will look for a person with the name in _val_ extracted from the incoming text, who is also a person, responding with any attributes recorded for that person that are a kind of the lineage given as the last parameter.
The lineage _noun:01,4,09,01,3,4,5_ corresponds to a biography, so this will return any biographical data.

### Text

This returns all the attributes associated with the object as text.
```darl
output textual val;
if anything then val will be Value["value:text"];
if anything then response will be Graph["text",val,"noun:00,2,00"]
```
This will look for a person with the name in _val_ extracted from the incoming text, who is also a person, responding with all the attributes recorded for that person.

### Links

This returns a list of the names of objects linked to the given object.

```darl
output textual val;
if anything then val will be Value[\"value:text\"];
if anything then response will be Graph["links",val,"noun:00,2,00"];
```

### Path

This details the names of the objects that form a path between any two given objects.

```darl
output textual val1;
output textual val2;
if anything then val1 will be Value["value:text","0"];
if anything then val2 will be Value["value:text","1"];
if anything then response will be Graph["path",val1,val2,"noun:","noun:"];
```

This extracts two text values from the input text and seeks to find a path between them. the values can be any kind of _thing_.


