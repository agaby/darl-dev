---
title: Connectivity
description: Connecting bots to external services.
output:
  html_document:
    toc: true
    toc_float: true
---


Connectivity
=====
This determines how a model interacts with other remote services, and must be set up per model.

![Connectivity overview](/images/connectivity_overview.png)

# Azure Bot Services

You can connect this model to multiple bots in Azure. Each row corresponds to an external Bot reference in the ABS.

![Connectivity ABS](/images/connectivity_ABS.png)

Obtaining these values is described in the [tutorial](./tutorial)

# Seller center connection

In the near future our bots will support the Microsoft seller center connection as a way of monetizing rule sets.
Enter your seller center data here.

![Connectivity seller center](/images/connectivity_seller_center.png)

# Zendesk connection

If you have a zendesk account you can save feedback from the bots to it, by way of rulesets. 
see the _report_bug.rule_ ruleset as an example of this.
Set up you account data here.

![Connectivity zendesk](/images/connectivity_zendesk.png)

# Sendgrid connection

If you have a sendgrid account you can use it to send emails from your bots, by way of rulesets. 
see the _call_andy.rule_ ruleset as an example of this.
Set up you account data here.

![Connectivity sendgrid](/images/connectivity_sendgrid.png)

# Twilio connection

If you have a twilio account you can use it to send SMS texts from your bots, by way of rulesets. 
Set up you account data here.

![Connectivity twilio](/images/connectivity_twilio.png)

# Save connectivity
This will save the changes you made to connectivity to your model and to the routing mechanisms in DarlBot.




