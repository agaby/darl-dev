Using Alexa with Rulesets
===

Our Darl bot system takes a two stage approach: working out what a user wants, and then either answering directly or routing them to a ruleset. 
Alexa takes the same approach, routing users to "skills".
Since Alexa is primarily voice-based, it requires a list of utterances to expect in order to limit the ambiguity of a completely free form system.
It would be very difficult to take a Bot model and mine all the possible utterances, but this would also be just reproducing Alexa's existing functionality.
The situation is different with Rulesets, however. 

# Rulesets as Skills

A ruleset is a block of functionality that takes a set of inputs and calculates or infers a response. As you'll know, the runtime engine analyses a ruleset and prioritizes inputs by their "salience".
This is therefore an ideal Alexa Skill. 
You can make a ruleset function as an Alexa skill with no programming, but some configuration.
The key thing that makes this simple is that a ruleset already contains a set of expected utterances, and this set is limited, so it is relatively easy to map texts to what Alexa calls "intents" which are groups or responses with the same meaning, or yielding the same data.

# Interaction Models

The way that you specify the intents and associated utterances in the Alexa development environment is via a piece of json.

A skeleton version of this can be generated with the following GraphQL:
```
{
    alexaInteractionModel(name: "The ruleset name", invocationName: "The invocation name for your skill")
}
```
Each intent will have a limited set of sample utterances associated with each possible input. You can expand these to improve accuracy or functionality.

For instance, the system might generate a sample "true". You can add further variations, like "that's true", "the answer is true" etc.

Do not change the names or the number of intents, however.


