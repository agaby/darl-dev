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
