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
This will show you how to get a Bot running in minutes using our example bot model.

You will need a Microsoft™ Azure account which you can get [here](https://azure.microsoft.com).

To begin, go to the [Microsoft Azure Portal](https://portal.azure.com) and log in to your account.

Select _Bot services_.


Select _Add_ and then _Web App Bot_.

Fill in the name of the new bot, which must be as yet unregistered, and fill in the other values.
Choose Echo bot as the type.

![Bot service add](/images/tutorial1.png)


After a short period your Bot connection will be created.

![Bot creation success](/images/tutorial2.png)

You can test this primitive bot just to make sure everything is working.
Note two things are created: a bot service and a web app, both with the same name.

![Echo bot test](/images/tutorial5.png)

Now let's create our own bot to overwrite this.

go to [GitHub](https://github.com/drandysip/DarlCoreBot) and download a copy of the DARL bot.

![The DARL bot](/images/tutorial9.png)

We'll assume you are using Visual Studio.

Load the solution, compile and publish it.

![publishing 1](/images/tutorial3.png)

Choose the web app you created previously.

![publishing 2](/images/tutorial4.png)

Once the bot is published we just have to change it's settings.

Back in the azure portal navigate to the Web app and edit the settings.

![Edit settings 1](/images/tutorial6.png)

There are four settings you need to add. They are

- "DarlAPIAddress", this is always: "https://darl.dev/graphql/"
- "DarlBotModel", this is the name of your model, such as "thousandquestions.model"
- "initialText", This is the first text the bot shows given a new connection, such as "hello from darlbot".
- "DarlAPIKey", This is the API key to access your account on Darl.dev. Find it by logging into Darl.Dev and then making the following query:

```json
{
  getApiKey
}

```

When the settings are complete you should see the following:

![Edit settings 2](/images/tutorial7.png)

Select _Save_ to save the changes.

Now, if you've done this correctly, you can navigate back to the bot service and test it.

![working bot](/images/tutorial8.png)

In order to use this bot in anger you need to connect it to various other channels, such as Skype or Facebook Messenger. This process is described in the Microsoft [bot framework documentation](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-manage-channels?view=azure-bot-service-4.0).




