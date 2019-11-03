---
title: Text sequence elements
description: How the DARL Bot System handles the elements of a text sequence.
output:
  html_document:
    toc: true
    toc_float: true
---

Text sequence elements
===
A text sequence represents one path to a response in the unguided part of Bot processing.

The bot text processor will look for each element in the sequence in the text given by the user, in the order given, but with any number of non-matching words separating each sequence.
At any one time several candidates will be considered until the last element in a sequence is matched and the text is exhausted.

The simplest sequence is just a sequence of words that are matched exactly ignoring case, but sequences may contain several other kinds of items.

# Lineages
Verbs and Nouns form hierarchies based on hypernymy. Each noun in English, or arguably any language, is a kind of another noun, apart from the very root of the tree, generally given the concept "thing".
Each element in the hierarchy often contains more than one word and variants based on plurals, genitives etc.
With our system you can specify one of these elements, rather than a word, and this will match any word and variant at this node including any elements below this node.

The raw material for the hierarchies is obtained from WordNet, with the addition of all the other parts of speech not covered.
The most efficient representation of an individual node is the path from the root to that node.
So, lineages are represented as follows:

```
<part of speech>:<comma delimited path from root>
```

So, for instance the verb "to be" is: 

```
verb:019
```
Whereas "picture" as graphic art is:
```
noun:00,1,00,3,10,00,07,1
```

The [edit tree](./edit_tree) page automatically looks up these concepts for you.

# Variables

The following variables are defined:

|Name |Description|
|---|---|
|value: | A generic value, treated as textual|
|value:number |A generic number, matching any number form |
|value:number,integer |An integer |
|value:number,float|A real number|
|value:choice |One of a set of choices |
|value:choice,boolean | choices limited to "true" and "false" |
|value:time |A time value |
|value:date |A date value|
|value:duration|A duration value, e.g. "week" |
|value:location |A recognized location, e.g. "London" |
|value:currency |A recognized currency value, e.g. "$5.34"|
|value:text |A text value |

# Default
At any part of the tree a single default value can be specified. If no better match can be found the deepest default found is triggered instead.

# Combining elements
A single match element may be a combination of several words, lineages or values separated by the '|' character.
__Do not mix types in an element, or debugging may be very difficult!__

# Path definitions

Define patterns by separating words with the "/" symbol.


All words should be lower case only. Separate alternate choices with the "|" symbol. So, "talk/to/dr|doctor|doc/andy will match "talk to dr andy" and "talk to doctor andy" and "talk to doc andy".
Concepts and special symbols have a ":" character in them. So, _my/name/is/value:_ will match "my name is andy" and extract the "andy" as the value.

Defaults are written as "default:" so _who/is/default:_ will match "who is andy" if "andy" is not specified in another phrase.

