# ThinkBase

ThinkBase is a SaaS system that allows you to build and reuse Knowledge Graphs.

We have an especial focus on _inference_ in this system, in that our Knowledge graphs both represent objects, attributes and relationships, and allow you to reason with them. 

This is most useful when your knowledge graph is intended to contain some kind of model that responds to data. 

ThinkBase can represent both kinds of Knowledge Graph:  graphs that represent real world objects and relationships and also graphs that represent a methodology to create knowledge.

Knowledge Graphs are part of the movement to supply AI functionality that can be easily understood and that can explain itself.

We've focused specially on  making the graphs both easy to construct and understand. We've created a web-based 2D graphical interface to do this, but are extending that to Virtual Reality in the near future.

# DARL
Whereas ThinkBase is relatively new, DARL (Doctor Andy's rule language) has been developed over some years.  

DARL is a language and set of tools for reasoning in the presence of uncertainty.
The principle business of AI and ML is handling uncertainty.
The most common aspect of uncertainty that drives the use of AI and ML is _model uncertainty_; the fact that for a particular problem there may be no defined analytical method for solving it.
Alternatively, the true state of a system may be unknown or known only within bounds. 

DARL has got two tools for handling such problems: heuristics and machine learning.

Where humans already have a set of heuristics to model an uncertain system, DARL can encapsulate them as a set of Fuzzy Logic rules.
Where only example data is available, DARL can machine learn relationships.

Uniquely, DARL uses the _same_ representation for knowledge derived from either method.

DARL is used as the constraint and inference language inside ThinkBase.

# Exploiting your Knowledge Graph

There are two principle ways you can use your KG in the real world.

## ChatBots

ThinkBase has a built in bot interface which can easily be used with the Microsoft Bot Framework and through that a range of portals, such as FaceBook messenger, Slack, Twitter, etc.
We are producing tooling to talk to a range of other portals, such as Discord and Alexa.

## Programmatic access

Underneath the hood, our 2D graphical interface and our VR engine talk to a GraphQL interface.

GraphQL is a replacement for REST interfaces created by FaceBook, that gives you access to extensible, self-documenting functionality delivered through a single POST endpoint.

 We have constructed a wide set of built in queries and mutations (commands that change data)  that allow you to do everything the UI can do programmatically.




