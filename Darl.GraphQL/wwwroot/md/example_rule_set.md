Example of creating a simple ruleset
===========

In this example we're going to turn a slightly tongue in cheek flowchart into a ruleset, and thus automatically into a questionnaire.
Karen Hao wrote an article in the MIT technology review called  "[Is it using AI](https://www.technologyreview.com/s/612404/is-this-ai-we-drew-you-a-flowchart-to-work-it-out/)" and including the following flowchart.
It details a set of questions intended to establish whether a device or system contains some element of AI.

![AI flow chart](/images/flow-chart-ai.jpg)

The first step is to list all the decision points in the flowchart, which correspond to inputs.

```darl
  input categorical canitsee {yes,no};
  input categorical canithear {yes,no}; 
  input categorical canitread {yes,no}; 
  input categorical canitmove {yes,no}; 
  input categorical canitreason {yes,no}; 
  input categorical identify {yes,no}; 
  input categorical respond {yes,no}; 
  input categorical transcribe {yes,no}; 
  input categorical readingtype {yes,no}; 
  input categorical readingpassages {yes,no}; 
  input categorical readinganalysing {yes,no}; 
  input categorical movebyitself {yes,no}; 
  input categorical moveseeshears {yes,no}; 
  input categorical movenotprogrammed {yes,no}; 
  input categorical reasonbigdata {yes,no}; 
  input categorical reasondecisions {yes,no}; 
```
All of the questions have yes no answers. I've given each decision point a darl type name.

Next we need two outputs.

```darl
  output categorical firststep {cisee, cihear, ciread, cimove, cireason,nodefinition};
   
  output categorical type {camera,wordproc,compvis,nlp,speech,mic,reading,robot,ml,math,nonreasoning,dumbbot, nothing};
```

The first output is an intermediary output. The flowchart is written so that one of the initial questions ought to be answered. Thereafter the other questions are called dependent on that first choice.

The second output is our result, and represents the message sent before the questionnaire terminates.

I've set a category for each terminating message.

The rules to process the first question are simple:

```darl
  if canitsee is yes then firststep will be cisee;
  if canithear is yes then firststep will be cihear;
  if canitread is yes then firststep will be ciread;
  if canitmove is yes then firststep will be cimove;
  if canitreason is yes then firststep will be cireason;
  if canitreason is no and canitsee is no and canithear is no and canitread is no and canitmove is no then firststep will be nodefinition;
```
As you can see it will ask the "can it see?", "Can it hear?" etc. questions and if none are yes it will set _firststep_ to _nodefinition_.

Finally we set the rules for the rest of the processing following the flowchart. There's one rule for each possible path.

```darl
 if firststep is cisee and identify is yes then type will be compvis;
  if firststep is cisee and identify is no then type will be camera;
  if firststep is cihear and respond is yes then type will be nlp;
  if firststep is cihear and respond is no and transcribe is yes then type will be speech;  
  if firststep is cihear and respond is no and transcribe is no then type will be mic;  
  if firststep is ciread and readingtype is yes and respond is yes then type will be nlp;
  if firststep is ciread and readingtype is no and readingpassages is yes and readinganalysing is yes then type will be nlp;
  if firststep is ciread and readingtype is no and readingpassages is no then type will be reading;
  if firststep is ciread and readingtype is no and readingpassages is yes and readinganalysing is no then type will be reading;
  if firststep is cimove and movebyitself is yes and moveseeshears is yes and movenotprogrammed is yes then type will be robot;
  if firststep is cimove and movebyitself is yes and moveseeshears is yes and movenotprogrammed is no then type will be dumbbot;
  if firststep is cimove and movebyitself is yes and moveseeshears is no then type will be dumbbot;
  if firststep is cimove and movebyitself is no then type will be dumbbot;
  if firststep is cireason and reasonbigdata is no then type will be nonreasoniing;
  if firststep is cireason and reasonbigdata is yes and reasondecisions is yes then type will be ml;
  if firststep is cireason and reasonbigdata is yes and reasondecisions is no then type will be math;
  if firststep is nodefinition then type will be nothing;
```
This determines the logic of the process. We still need to set the text. This can be done via the [Darl text](https://darl.dev/rulesettext) page.

![darl text for is it AI](/images/darl_text_is_it_AI.png)

Finally the very last thing is required because we've included an intermediary output. These are very useful for forcing the rule set into a particular order. In this case we want to ensure one and only one of the first set of questions will be asked.
We don't need to output anything to the user for this output, but outputs are set to be visible by default.

to toggle this, go to [Darl format](https://darl.dev/rulesetformat) and set "hide" to true on the _firststep_ output.


You can find the full example in the default rule sets. You can try the ruleset out on the [rule evaluator page](https://darl.dev/darlevaluator).
