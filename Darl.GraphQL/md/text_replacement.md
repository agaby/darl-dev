---
title: Reporting with DARL
description: How to create reports in the DARL Bot System.
output:
  html_document:
    toc: true
    toc_float: true
---

Reporting with DARL
===

Often we want to generate a textual result that combines input or inferred data with prewritten text.

This is done with darl using the document function.

For instance:

```
output textual time;
if anything then time will be Bot["time"];
if anything then response will be document("%% time %% UTC",{time});

```
The output of this DARL fragment would look like:

    11:08 UTC

In this example we declare a temporary output called "time", use it to hold the current time,
and then insert it into a format string.

The _document_ function takes two parameters, the format string, and a list of inputs or outputs occurring in that string. Using an input or output without declaring it will create a runtime error.

# the format string

The format string can be written directly as a literal string, can be a predeclared string constant, or can be a textual input or output.

For long text in rulesets it makes sense to use preloaded data to contain the format string and to prepare the text using an external text editor.

The string contains either simple strings or markdown text with the following special characters.

## Value insertion

The characters '%%' are used to surround an input or output name that will be inserted into the final text.

## Conditional sections

A categorical output must be used for these. You can conditionally show different sections of text, which can include value insertions and other conditional sections.

The format is:

    %% { <category i/o>.<category> < conditional section of text > <category i/o>.<category> } %%

The Parking appeal UK demo ruleset illustrates this in action. The following is a fragment of the whole format string.

The output _grounds_ is a categorical output, and the subsection shown bracketed by _grounds.res_ is shown if grounds has a truth value > 0.5;

```
To whom it may concern, 

On %% date_of_ticket %% at %% time_of_ticket %% my vehicle, registration number %% reg_number %% received a ticket from your authority.

%% {grounds.res %%Thank you for taking into account my appeal. I am appealing on the basis that it is obvious that the date %% correct_date %% was misstated to be %% incorrect_date %% on a permit for residents. I arrived at %% time_of_arrival %% and as I was in a rush when placing a valid permit, selected %% incorrect_date %% rather than %% correct_date %% on the permit. 

As I do not have a history of persistently incorrectly using permits, I feel that the rules should be consistently applied and my representation should be accepted. 

I believe that it is impossible the permits, which stated, %% incorrect_date %% were used on another day, since the last %% incorrect_date %% was clearly prior to the issue of the permit. It is therefore clear that this PCN was issued for a mis-stated date, making the PCN trivial and inconsistent with other offenses for which the fine is issued usually. 

Furthermore, in an almost identical case, London Tribunals (formerly PATAS, Parking and Traffic Appeals Service) allowed an appeal, stating the clear precedent that this type of error is 'de minimis': 

BH05920B "The PCN was issued for parking in a resident's permit space without displaying a valid permit. The appellant was a visitor. She obtained and displayed a visitor's scratch-card voucher. This required her to scratch off the day of the week, date, month and year. She made a mistake with the date, scratching off 21 instead of 22; all the other details were correct. The council claimed that this error invalidated the voucher. Held: This was the wrong approach. No contravention had occurred. The mistake was de minimis and did not invalidate the voucher. Given the combination of correct information given, the voucher could not have been used on any other day. While the adjudicator did not necessarily criticise the enforcement officer for issuing the PCN, the council should have appreciated that a minor and genuine error had occurred and canceled it. Appeal allowed" 

Though I of course apologize, I believe that the council should exercise fairness in canceling a ticket that, according to the guidance, is perfectly justified to be canceled. I will of course endeavor to avoid this again, but I feel that the issue of a ticket is a disproportionate action inconsistent with PATAS precedent since I had valid permits and used them in an honest way that isn't harmful to the %% council %% community.%% grounds.res } %%

```


