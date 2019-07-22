---
title: Ruleset Run
description: Trying out a DARL Ruleset.
output:
  html_document:
    toc: true
    toc_float: true
---


Ruleset Run
==============

This enables you to test rulesets interactively.
A ruleset is run through the same inference and questionnaire engine used in the bots.
You can see the order questions are asked and check the logic of your implementation, along with the text displayed at each juncture.
How the same set of questions is displayed visually in a bot is dependent on the channel used.

![Ruleset run](/images/ruleset_run.png)

# Show saliences

As the questionnaire is answered, the list of unknown inputs is shown, along with their calculated saliences.
The state of outputs, whether satisfied or not, is also displayed.