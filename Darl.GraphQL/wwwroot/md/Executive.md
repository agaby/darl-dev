---
title: DARL Bot System
description: Overview of the DARL Bot System
output:
  html_document:
    toc: true
    toc_float: true
---

DARL Bot System
=======
Dr Andy's IP LLC 
# Introduction
The DARL Bot System (DBS) is intended to be an alternate way of creating Bots exhibiting artificial intelligence to the coding and Machine Learning based solutions created by Microsoft.

It connects and integrates with the Microsoft Bot Framework, but offers an SaaS solution that requires no programming*, just configuration, to function.

Bots created with DBS can be “unguided” as in responding to any query and/or “guided” as in leading the user through a set of pre-planned questions.

*DR Andy’s IP has re-invented the Expert System for the 21st century using DARL, our fuzzy logic inference language. In order to create truly intelligent systems, some rule definitions must be written in this extremely simple declarative language, but this is very much simpler than programming, and accessible to any intelligent professional.
# Using the SaaS facilities
After registration on our site, Darl.ai, new users are presented with a pre-built prototype Bot model and a series of demonstration expert system rulesets. The Bot model is intended to handle the “unguided” part of user interaction processing, and consists of a configurable phrase recognition engine that can respond to simple enquiries, and the ability to hand over to the expert systems for more complicated processing.

A new user can rapidly configure the bot model to respond in the appropriate way for their application. Building expert systems is a more difficult process, but one that typically takes hours and days, rather than the months of previous Expert Systems, using our engine and tools. Facilities also exist to machine learn rulesets from data.

The Bot is integrated with the Microsoft Bot Framework by creating a new Bot in Azure, and copying the URL and secret key to the darl bot model. The new bot is hosted through our platform (on Azure) and immediately available.
# Authentication and charges
The DBS site uses Azure AD to authenticate and register. Each new user is provisioned with a basic set of a bot model and demo expert systems.  Users are identified as two potential types: individual users, who may use a generic AD tenant or a corporate one, and corporate users who own their own AD tenant. Both kinds of users get a free 30 days use of the site at sign up, with unlimited bots, but only 1,000 interactions. 

Individual users are charged $50 per month for using the site with 1,000 free interactions, and corporate users, who register an AD tenant and can have unlimited sub users, will pay $500 per month with 10,000 free interactions. Further interactions for both types are charged at 2 cents each.

Billing is by email invoice with PayPal and other payment facilities in the invoice. Delinquent accounts will be suspended after 14 days, but can be restarted on payment.



