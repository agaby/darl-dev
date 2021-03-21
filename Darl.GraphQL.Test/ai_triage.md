%% {type.analytical %% 
The problem you have described is analytic in nature. 
This means there is already an algorithm, equation or set of rules for deciding how it should function. 
In almost all cases you will get better results without using AI. 
There are two possible exceptions to this. 

One is that there is an algorithm, but it is ridiculously expensive in computer time or computer memory, so a best estimate system may be better even if the results are sometimes inaccurate. In this case try our AI advisor again, but this time say that there is no analytic solution. 

The other is the case where there is an analytical solution, but it changes frequently, so that using conventional programming provided by a conventional software development department causes an unacceptable delay every time something changes. 

In this case using soft computing techniques and a rules engine such as DARL may result in a system that can be updated rapidly by non-technical users, such as those who make the decisions, to change things themselve
%% type.analytical} %%

%% {type.expert %% 
The problem you have described may be amenable to soft computing techniques. 
These are an array of techniques that include [Fuzzy logic expert systems](https://en.wikipedia.org/wiki/Fuzzy_logic), as supplied by [DARL](https://darl.ai), 
but also [Robotic Process Automation](https://en.wikipedia.org/wiki/Robotic_process_automation), 
[Case-based reasoning](https://en.wikipedia.org/wiki/Case-based_reasoning) and [Bayesian Belief Networks](https://en.wikipedia.org/wiki/Bayesian_network). 
They are characterized by the fact that the experience of a human who already does this task can be used to build an automated version of it. 
Some tools (DARL) are easier to use than others, and require less experienced technical input. The tools vary in their sophistication too.  
RPA tools are generally poor at handling uncertain or vague data.
%% type.expert} %%

%% {type.super %% 
The problem you have described seems to be amenable to supervised learning. 
You have indicated that the results are not business critical, and some unexplained bad classifications or predictions are acceptable. 
This is the big growth area in modern AI with a huge range of products available. 
For supervised learning you need a training set of data that has been “marked up” to include the results you want, and the values you think can be used to generate the result. Supervised learning techniques generally then create a “blackbox” model that creates the result that you want when presented with your data. 
This process may fail if there is no way to predict the result based on the data, i.e. if the data and the result are completely unrelated. 
The system may not get the prediction or classification right every time, and because it’s blackbox it won’t be able to tell you why. 
More importantly, in most cases your training data is a subset of some much larger set. 
The training set may not be representative of this larger set, and various problems under the heading of “overtraining” can arise that may result in the system giving you great results for examples it knows and poor results for ones it doesn’t. 
The art of ML is balancing these requirements. Great strides have been taken in recent years to automatically adapt the ML systems to minimize these errors. 
The old adage of “Garbage in, Garbage out” is still true, however.
%% type.super} %%

%% {type.super_whitebox %% 
The problem you have described seems to be amenable to “whitebox” machine learning. 
This means that you have a data set of examples and desired outputs, or can generate one, and would like to build a model that relates the inputs to the outputs. 
As with all examples of machine learning, the process may fail if there is no way to predict the result based on the data, i.e. if the data and the result are completely unrelated. 
The system may not get the prediction or classification right every time, but since this is whitebox machine learning you should be able to understand why. 
If the system rightly or wrongly makes a decision affecting a human customer, you will be able to explain why, should they insist on the nascent “right to an explanation”. 
There are not as many whitebox system vendors as the more common blackbox system vendors. 
DARL has a range of whitebox algorithms that machine learn to human readable if…then rules. 
At DARL we use fuzzy logic, and thus possibility theory as our algebra of uncertainty. 
There are tools that will learn from data and create Bayesian Belief Networks which rely on probability theory as the algebra of uncertainty.
%% type.super_whitebox} %%
%% {type.unsuper %% 
The problem you have described seems to be amenable to unsupervised machine learning. 
This means that you have, or can generate a dataset of vectors, containing numbers or categories but no means of tagging them. 
Usually this is a first exploratory phase with new data. 
Sometimes your data will naturally cluster into identifiable groups, and unsupervised learning can be used to find the center and bounds of each cluster. 
DARL uses the Fuzzy KNN algorithm to identify the boundaries of such clusters. The clusters found may or may not be useful to you, but if they are, they can form the basis of a bigger system where the cluster an input vector belongs to can be used in further processing. 
Unsupervised learning, using different algorithms, is also used in deep learning as part of a composite process, where multiple unsupervised learning layers slowly adapt to repeated input data and build structures that can be used to recognize patterns that would be otherwise impossible to detect. 
This process is an extremely large consumer of computing power, but underlies the recent success of ML in visual and sequence processing tasks.
%% type.unsuper} %%

%% {type.critic_whitebox %%
The problem you have described seems to be amenable to Whitebox reinforcement learning. 
Reinforcement learning is used where training data is not available, but a system to be optimized exists. 
Depending on practicality, either the real system, or a simulation of the system are used in the process. 
A simple example of using the real system is A/B testing as used in marketing to adapt the choice of adverts for different audiences. 
Since reinforcement learning will often produce poor examples as part of the learning process, in most circumstances a simulation of the system must be used. 
Reinforcement learning can optimize the parameters of a system, in which case Genetic Algorithms, Simulated Annealing or Differential Genetic Algorithms may be used. 
Often an entire functional element is required to optimize the system, in which case Genetic Programming or Deep reinforcement learning are suitable algorithms. 
Of the two, only Genetic Programming is a Whitebox algorithm, meaning that once the missing functional element is created it can be easily understood and analyzed to understand how it works.  
Such a system may have to generate intermediate states in order to fulfill the requirements of the simulation. 
An example might be programming a robotic arm to perform part of a manufacturing process, where several movements may be required to achieve the desired goal. 
Dr Andy created the first GP based financial trading system generator and wrote the first papers on using GP with fuzzy logic rules. 
The DARL system is organized so that DARL and DASL rule sets can be generated in an interactive way with an external simulation. 
%% type.critic_whitebox} %%

%% {type.critic %%
The problem you have described seems to be amenable to reinforcement learning. 
Reinforcement learning is used where training data is not available, but a system to be optimized exists. 
Depending on practicality, either the real system, or a simulation of the system are used in the process. 
A simple example of using the real system is A/B testing as used in marketing to adapt the choice of adverts for different audiences. 
Since reinforcement learning will often produce poor examples as part of the learning process, in most circumstances a simulation of the system must be used. 
Reinforcement learning can optimize the parameters of a system, in which case Genetic Algorithms, Simulated Annealing or Differential Genetic Algorithms may be used. 
Often an entire functional element is required to optimize the system, in which case Genetic Programming or Deep reinforcement learning are suitable algorithms. 
Such a system may have to generate intermediate states in order to fulfill the requirements of the simulation. 
An example might be programming a robotic arm to perform part of a manufacturing process, where several movements may be required to achieve the desired goal. 
Dr Andy created the first GP based financial trading system generator and wrote the first papers on using GP with fuzzy logic rules. 
The DARL system is organized so that DARL and DASL rule sets can be generated in an interactive way with an external simulation.
%% type.critic} %%

%% {type.nlp %%
The problem you have described seems to be amenable to Natural Language Processing. 
There are two main threads to NLP, the first is to use grammar, dictionaries, thesauri and analytical techniques to construct a solution, the other is to short circuit this by using mostly blackbox techniques to learn relationships directly from examples. 
This problem seems to be amenable to the first thread. Dr Andy’s IP has a suite of tools that can perform NLP on text generated through chatbots, SMS messages, emails and many other sources. 
Some of these are available directly through the API at [https://darl.dev](https://darl.dev), others will require consultation with us. 
%% type.nlp} %%

%% {type.other %%
The problem you described doesn’t seem to fall into any of the categories we’ve set so far. 
This is really interesting. Please write a short description of your problem and send it to [suport@darl.ai](mailto:support@darl.ai). 
We’ll get in contact with you to talk about any solutions we see, and perhaps extend this expert system to cover it. 
%% type.other} %%

%% {type.assoc %%
The problem you have described seems to be amenable to Association Learning. 
This generally applies to selections of a few elements out of a much larger range. 
The obvious example of this is shopping baskets, where a vendor may sell a range of thousands of products, but only a small selection appears in a given shopping basket. 
Association learning is used in recommendation systems for on-line retailers, and for product placement in bricks and mortar retailers. The standard algorithm for this is [Apriori](https://en.wikipedia.org/wiki/Apriori_algorithm). 
DARL has an implementation of this that generates DARL rule sets from the source data. Please contact us [support@darl.ai](mailto:support@darl.ai) if you’d like to trial it.
%% type.assoc} %%

%% {type.assoc_whitebox %%
The problem you have described seems to be amenable to Association Learning. 
This generally applies to selections of a few elements out of a much larger range. 
The obvious example of this is shopping baskets, where a vendor may sell a range of thousands of products, but only a small selection appears in a given shopping basket. 
Association learning is used in recommendation systems for on-line retailers, and for product placement in bricks and mortar retailers. The standard algorithm for this is [Apriori](https://en.wikipedia.org/wiki/Apriori_algorithm). 
DARL has an implementation of this that generates DARL rule sets from the source data. Please contact us [support@darl.ai](mailto:support@darl.ai) if you’d like to trial it.
%% type.assoc_whitebox} %%

%% {type.super_nlp %%
The problem you have described seems to be amenable to Natural Language Processing. 
There are two main threads to NLP, the first is to use grammar, dictionaries, thesauri and analytical techniques to construct a solution, 
the other is to short circuit this by using mostly blackbox techniques to learn relationships directly from examples. 
This problem seems to be amenable to the second thread. 
Dr Andy’s IP has a suite of tools that can perform NLP on text generated through chatbots, SMS messages, emails and many other sources. 
Some of these are available directly through the API at [https://darl.dev](https://darl.dev), others will require consultation with us. 
Alternatively there are a growing number of Machine Learning tools mostly based on the LSTM algorithm that attempt to learn structural relationships within language without presupposing any structure in that language. 
These require large amounts of processing to train but are efficient to reuse. 
In general, these are the preferred option where non-standard grammars and usages are likely to be encountered as in colloquial speech.
%% type.super_nlp} %%

