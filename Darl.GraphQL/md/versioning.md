---
title: Editing and versioning
description: Versioning Bots and Model elements.
output:
  html_document:
    toc: true
    toc_float: true
---


Editing and versioning
====
Whenever you edit a DARL model, ruleset or ML model the local copy only is edited. 
You can test this and modify it, but the model must be saved before it is permanently recorded and modifies a running bot.

![editing and saving](/images/editing_and_saving.png)

The current edited item is shown at the top left of the top bar, and the arrow symbol indicates that a save is required.

# Versioning

You can clone existing Bots, rulesets, etc and create new ones using the [Models](./select_model_ruleset) page.
You can assign any Bot in the Bot framework to any Bot Model using the [Connectivity](./Connectivity) page.
Thus you can support multiple versions of the same Bot, and switch which is in use.