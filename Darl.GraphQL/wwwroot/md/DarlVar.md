---
title: Class DarlVar
description: The DarlVar class that holds DARL I/O data.
output:
  html_document:
    toc: true
    toc_float: true
---
Class DarlVar
===

A general representation of a data value containing related uncertainty information from a fuzzy/possibilistic perspective.

# Properties

## approximate
boolean

Indicates approximation has taken place in calculating the values.
Under some circumstances the coordinates of the fuzzy number in "values" may not exactly represent the "cuts" values.


## categories
Dictionary&lt;string, double&gt;

list of categories, each indexed against a truth value.

## dataType
Enum DataType

Gets or sets the type of the data.


## name
string

Gets or sets the name.

## sequence
List&lt;List&lt;string&gt;&gt;

Gets or sets the sequence.

## unknown
boolean

This result is unknown if true.

## Value
string

Single central or most confident value, expressed as a string or double.


## values
List&lt;double&gt;

The array containing the up to 4 values representing the fuzzy number.

Since all fuzzy numbers used by DARL are convex, i,e. their envelope doesn't have any in-folding sections, the user can specify numbers with a simple sequence of doubles. So 1 double represents a crisp or singleton value. 2 doubles represent an interval, 3 a triangular fuzzy set, 4 a trapezoidal fuzzy set. The values must be ordered in ascending value, but it is permissible for two or more to hold the same value.


## weight
double

The confidence placed in this result

# Enumerations

## DataType

The type of data stored in the DarlVar

| values | use | integer value |
|----|----|----|
|numeric|Numeric including fuzzy|0|
|categorical|One or more categories with confidences|1|
|textual| a text string|2|
|sequence|a text sequence|3|

# Example Json

```json
  {
    "name": "string",
    "unknown": true,
    "weight": 0,
    "values": [
      0
    ],
    "categories": {},
    "approximate": true,
    "dataType": 0,
    "sequence": [
      [
        "string"
      ]
    ],
    "Value": "string"
  }
```

