---
title: The Edit Tree page
description: Editing the knowledge of a Bot.
output:
  html_document:
    toc: true
    toc_float: true
---

The Edit Tree page
====
A Bot model consists of several elements, but the main one is the recognition tree.
This holds a tree of words, sets of words, concepts and variables that make up the recognition part of the bot.

Text written or converted from speech uttered by a user is broken into words and applied to the tree.
With each new word a pointer moves down the tree, and when it reaches a leaf node a candidate response is flagged.

So the tree contains the language knowledge of the Bot. It is the bit of the bot that responds to unguided text, i.e. to any text a user may enter.

The Darl Bot system works by processing unguided text until a guided sequence of text can be started. 

This is rather like talking to a financial or legal advisor, where initially the conversation might be about anything, and then as soon as the advisor works out what the question is about, he or she takes over the conversation to elicit the information needed to create a response.

This page is concerned with editing that tree. 

There are two ways yu can do that, through the tree view or the phrase view. Both are selected via tabs at the top left of the page:

# Tree view

## Recognition tree

![Edit tree elements](/images/edit_tree_elements.png)

To the immediate right of the sidebar is the tree view.
By clicking on elements you can open up parts of the tree for inspection.

To the right is the attribute section, which synchronizes with your selection of a place on the tree.
The attribute section determines what the response will be, if any, to a user who's text directs the recognition engine to that part of the tree.
  
Beneath the attribute section is a list of potential lineage matches for the word selected in the tree.

## Editing the tree

A right button click on a tree element brings up a menu.

![tree right button click](/images/edit_tree_rbc.png)

### Create
Generates a new child element beneath the current selection, so in effect another word in the sequence.
### Rename
Changes the text at that location
### Delete
deletes the text at that location
### Cut
Removes that element and the entire sub tree, and puts them into the paste buffer.
#### Copy 
Copies that element and the entire sub tree, and puts them into the paste buffer.
### Paste 
Pastes a sub tree from the paste buffer at that point.

## Add entry at root
This button adds a new root entry, i.e. initial word in the sequence to be matched.


### Text sequence elements
A list of all the things you can enter at each point in the tree is available on the [text sequence elements page](text_sequence_elements)


# Phrase view
Using this view you can try out phrases to see if they are in the model and add or delete them.

You can also look at the default responses of your bot to users and add responses where appropriate.

![Edit tree elements](/images/edit_phrase_elements.png)

## Find phrase
Type a phrase that you want to find or create anew.
If the phrase exists the _exists_ check box is highlighted.

## Pattern
This responds to _Find Phrase_ if a new phrase is entered. This will contain the matching pattern or path in the tree, if _exists_ is checked, or otherwise will contain a best guess as to the right place to put this phrase in the tree.

You can change this default and enter your own path, using the formatting described in _help with patterns_.

## Add

If the path given does not exist in the model, clicking this will add it to the model along with any attributes, such as DARL code, defined in the attribute section.

## Delete
This will remove a given path and attributes from the model if it exists.

## Default handling

As your Bot runs any interactions where a _default_ is triggered are stored along with identification of the model and owner.
These anonymized records are vital for discovering interactions that your bot is not responding to.

A table of these interactions is shown, along with the response the Bot gave. This could be the root default response, or any other default response further down the tree.

![Edit tree elements](/images/edit_default_handling.png)

Clicking on a row highlights it and copies the phrase to the _find phrase_ edit box, which will trigger a look up of the phrase.
Since these records might be old, you may already have added this phrase to the model. In this case the _exists_ check box will be highlighted and you need do nothing more.
Otherwise you can modify the model to respond appropriately.

## Default delete

Clicking on this deletes the default pattern. 

## Search

This searches over texts entered by the user looking for texts that start with the search phrase.


 

# Attributes
This section of the page determines what happens when a particular word sequence is provided by the user.
The possibilities vary from the really simple, to respond with text, to the slightly more complicated, to respond with a call to a ruleset, to the advanced, where you can edit DARL rules to craft a response.
These are selected using tabs.

## Text response
Using the response tab, you can tell the bot to return either a single piece of text, or a randomly selected text from a selection.

### Single response

![single response](/images/text_response.png)

Select string to the right of _single response_. You can the enter any text to respond to the given word sequence.

### Randomly selected responses

A simple trick to make your bot seem more interactive is to have a set of possible responses. 

![Random text response](/images/multi_text_response.png)

In this case the _randomly selected response_ check box is selected and a list edit control enables you to add any number of possible responses.

## Call
You can use this tab in combination with the response tab. This tab determines if a ruleset should be called.
It displays a drop down list of all the rulesets currently defined in your account. 

![Call tab](/images/attribute_call.png)

## Advanced
This tab permits you to edit the DARL rules associated with this word sequence. The other two tabs automatically create DARL rules. This tab enables you to edit the DARL directly.
If you make any changes to the DARL code changes to the other tabs will be ignored.

![Advanced tab](/images/advanced_tab.png)

Using the DARL editor is described here: [Darl editor](darl_editor).

## Save attributes button
Any changes to the attributes are saved by clicking this button.
As usual these changes only become part of the working Bot model, and are not saved to the bot until the save icon on the top bar is clicked.

## Possible lineages list
Whenever a tree element is selected, if it just contain s a simple word, a lookup is made of all the concepts related to this word.
You can replace the word with a concept by clicking on a concept.
This is an extremely powerful tool, because thereafter all words that match this concept, and any concept derived from it, in any grammatic variant will be matched.
This can reduce the size of the tree dramatically, but is most appropriate for those with a knowledge of computational linguistics.









