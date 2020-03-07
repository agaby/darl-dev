---
title: Documentation index
description: Documentation for the DARL system, Bot Framework and more.
output:
  html_document:
    toc: true
    toc_float: true
---


### Documentation index

DARL is a language and set of tools for reasoning in the presence of uncertainty.
The principle business of AI and ML is handling uncertainty.
The most common aspect of uncertainty that drives the use of AI and ML is _model uncertainty_; the fact that for a particular problem there may be no defined analytical method for solving it.
Alternatively, the true state of a system may be unknown or known only within bounds. 

DARL has got two tools for handling such problems: heuristics and machine learning.

Where humans already have a set of heuristics to model an uncertain system, DARL can encapsulate them as a set of Fuzzy Logic rules.
Where only example data is available, DARL can machine learn relationships.

Uniquely, DARL uses the _same_ representation for knowledge derived from either method.

The DARL tool-set includes a Bot creation service that uses NLP and heuristics to understand requests and offer advice, a Fuzzy logic expert system like inference engine that can be used with incomplete data, and that can create it's own sequence of minimal questions to create an inference, and a set of Machine learning algorithms that create results that are usable in the former engines.

The DARL engines are accessible via the API described in these pages so you can develop intelligent solutions making use of them.


#### Overview

[Using the Darl.dev site](./darl_dev)

[Executive summary](./executive)

[Overview of bot functionality](./overview)

[Tutorial on setting up a bot](tutorial)

[Example of creating a ruleset](example_rule_set)


### GraphQL API
[Using the GraphQL API](./GraphQL)

[Usage examples](./GraphQL_examples)

#### DARL
[The DARL language](./darl)

#### DARL editor
[The DARL editor](./darl_editor)

#### Models and rulesets
[Select the model or ruleset to edit](./select_model_ruleset)

#### Framework
[Edit the darl fragment environment](./framework)

#### Model tree
[Edit the model tree](./edit_tree)

#### Model test
[Test the model in conversation](./model_test)

#### Ruleset edit
[Edit a ruleset](./ruleset_edit)

#### Ruleset run
[Run a ruleset](./ruleset_run)

#### Ruleset documents
[Add a document to format results](./ruleset_documents)

#### Machine learning
[Machine learn a ruleset](./machine_learn)

#### System simulation in time
[How to create a simulation](./simulation)

#### Connectivity
[Set connection data](./connectivity)

#### Collateral
[Add markup to responses](./collateral)

#### Stores
[Extra functionality for Bots](./stores)

#### Text sequences
[The grammar of text sequences](./text_sequence_elements)

#### Editing and versioning

[Saving changes and how to support multiple versions of bots](./versioning)

#### Resources

[Resources to help you use DARL](./resources)

#### Accounts

[About accounts](./accounts)

#### Bot model caching

[Updating models](./bot_model_caching)

#### Creating reports in DARL

[Text replacement and report generation](./text_replacement)

#### Knowledge graphs (Thinkbase)

[Using our knowledge graph technology](./knowledge_graphs)



