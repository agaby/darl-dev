---
title: Tutorial
description: Tutorial on creating a Bot.
output:
  html_document:
    toc: true
    toc_float: true
---

Tutorial
===
This will show you how to get a Bot running in minutes using our example bot model, and then how to change the responses to something appropriate to your needs.

You will need a Microsoft™ Azure account which you can get [here](https://azure.microsoft.com).

To begin, go to the [Microsoft Azure Portal](https://portal.azure.com) and log in to your account.

Select _Bot services_.

![Bot services in portal](/images/bot_services_in_portal.png)

Select _Add_ and then _Bot Channels Registration_.


![Bot service add](/images/Add_Bot_in_portal.png)

Select _Create_.

Fill in the name of the new bot, which must be as yet unregistered, and fill in the other values.

![Bot channels registration](/images/bot_channels_reg.png)

Either set the appid and password yourself, or auto create.

Select _Create_ again.

After a short period your Bot connection will be created.

Select the bot you have just created, go to _settings_ and then above the App ID, click on _manage_.

Now, in Darl.ai, select the model you wish to use for the bot on the models page.

Select _Open model_ and then choose _Connectivity_ on the left side bar.

![Bot connectivity](/images/bot_connectivity.png)

Click the _+row_ button and fill in the App ID and the password from the portal settings. You may need to create a new pasword.

Now click _Save connectivity_ at the bottom of the page.

Back on the _Portal Settings_ page, set the messaging endpoint to "https://darlbot.com/api/messages"

![messaging endpoint](/images/messaging_endpoint.png)

Save the changes to the bot in the portal.

Now, if everything has worked, you should be able to test the new bot using the _Test in web chat_ link.

![Test in web chat](/images/test_in_webchat.png)

You can make changes to the selected bot using the [Edit tree](edit_tree) page.

You can switch the bot model used by deleting the appropriate row in the Connectivity Bot Framework's connection, (making note of the App ID and password) opening the preferred model on the models page, going back to connectivity, now for the new model,  and adding the removed credentials to that bot model.


