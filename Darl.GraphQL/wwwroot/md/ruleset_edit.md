---
title: Editing Rulesets
description: How to Edit a DARL Ruleset.
output:
  html_document:
    toc: true
    toc_float: true
---


Editing Rulesets
===
A ruleset consists of darl code and various ancillary items, such as input/output formatting, the text to display for data elicitation and ruleset metadata.
The edit page is broken up into several tabbed areas that can be opened to reveal parts of the ruleset model.


![ruleset editor](/images/ruleset_edit.png)

# Rules
This DARL editor displays the full code associated with the ruleset.
[Details on using the DARL editor](darl_editor)
Various elements in the DARL code, for instance the inputs and outputs need to be synchronized between the different tabs.
This occurs whenever the ruleset is saved. The DARL code is always the canonical source. 
![ruleset edit rules](/images/ruleset_edit_rules.png)

# IO Format
This section permits you to edit how a ruleset input, and thus a potential question for the user, is formatted.
![ruleset edit io](/images/ruleset_edit_io.png)

# Default questions
This determines the number of questions asked at a time. For bots this is 1. In other applications some other number might be chosen.
# inputs
This displays the inputs listed in the DARL code. The names and types are read-only, but you can set other parameters that determine how the inputs are displayed and validation performed on them.
## Edit increment
for numeric inputs this determines the incremental value used in a control that supports increments.
The value '0' turns this off.
## Max length
For textual inputs the maximum text length. The value '0' turns this off.
## Min value 
Minimum range value for a numeric input.
The initial value of this is determined by the lowest bounds of any fuzzy sets specified for this input in the DARL code.
## Max value
Maximum range value for a numeric input.
The initial value of this is determined by the highest bounds of any fuzzy sets specified for this input in the DARL code.
## Regular expression
Specifies a regular expression used to check the format of any text entered, for instance an email address or a phone number.
## Show sets
For numeric inputs, where sets are defined, rather than asking for a numeric value, the user will be asked for one of the set names if this is true.

# Outputs
This displays and edits formatting data for the outputs of a rule set.
![io outputs](/images/ruleset_edit_io_outputs.png)
## Hide result
Outputs marked true are not reported back to the user.
## Display Type
choices are
### Text
The output value is reported as text
### ScoreBar
The numeric output value is reported as a score bar, where supported.
### Link
The output is formatted as a link.
### Redirect 
The output is interpreted as the name of another ruleset to call.
This is superseded by 'Call' stores.
### Bar Color
An RGB value specifying the score bar color, where supported.
### Bar max val
The upper bound of the score bar, where supported
### Bar min val
The lower bound of the score bar, where supported.
### Uncertainty
Since DARL is a fuzzy logic expert system, all inferences come with certainties and tolerances.
Usually only the central or dominant value is reported. Selecting True will add this data where supported.
## Value format
For numeric values this determines the numeric display format, for instance the number of decimal places, following the microsoft format.

#Language
This displays and edits the text items associated with the inputs and outputs.
![language text](/images/ruleset_edit_language.png)
The _text_ field contains the text that will be displayed. 
The text items are determined by the ruleset and also contain a set of fixed items that determine the text that appears before questions are asked, and before results are supplied.
these are:

|Name|Purpose|
---|---|
|Format.preamble|Text provided before the questions are presented|
|Format.questionHeader|Text provided immediately before the first question|
|Format.resultHeader|Text provided once the inference process is complete and before the results are displayed.|

## Categorical io
Where categorical inputs are specified, the displayed text for each is specified by the item with the name 
```
<io name>.<category name in the code>
```
## Variants
Where supported alternate texts in different languages can be provided.
### Language
The ISO standard language
### Text
The text in that language

## Default language
The ISO standard language specifier that is used as default. If empty "en_US will be employed.

# Preloaded Data
As a ruleset is run, the inference engine attempts to satisfy the outputs of the rule set by acquiring the value of the various inputs.
You can preload the data that is supplied to one or more inputs.


# Actions at completion

# Test data

# Metadata
Each ruleset can be annotated with a range of metadata that can be used to classify and potentially market rulesets.

## Name
The name of the ruleset.

## Description
A textual description of the rulesets purchase and features.

## Version
Version of this copy of the rule set.

## Author
The name of the author of the ruleset.

## Copyright
Copyright information for this ruleset.

## License
License information, either the text of the license or reference to a standard license type like "MIT".

## Image Url
An image hosted externally used in association with this ruleset. In future this may be used for a directory of rulesets.

## Price
If supported on the channel used, it may be possible to charge for the use of this ruleset. Specify the numeric price you expect here. (numbers and decimal point only.)

## Currency
The currency for the above price.

# Save Rulest
Saves all changes to the current loaded ruleset model. The changes will only become permanent once the whole model is saved.

# Run the Form
This has the same affect as selecting _Run_ on the side bar.

 



