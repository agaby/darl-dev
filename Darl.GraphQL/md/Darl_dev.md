The Darl.dev site
====

The [Darl.dev](https://darl.dev) site serves two purposes:

+ To host the DARL GraphQL API 
+ To act as a development site for users of the API.

You can access the GraphQL endpoint programmatically at [https://darl.dev/graphql](https://darl.dev/graphql).

The rest of the site is concerned with making it possible for you to develop applications using our API.

# Accounts
The darl.dev site is _tenanted_. This means that each user gets a separate account with their own data objects such as rulesets, machine learning models and bot models.
When you first sign up a set of default objects are created for you. You can delete these or modify them as you choose.

## Anonymous access
When you first access the site without registering and logging in, you will have access to a set of demo objects. While you can view these and do things like evaluating rulesets, you cannot do anything involving _mutation_. I.e. you can't change anything.

## Registering

When you select the Log in/Register button You'll be presented with this dialog.

![Sign in register](/images/sign_in_register.png)

If you choose one of the social providers such as Google, Facebook or Twitter then you will get taken to their pages to log in, and if you have not used our site before through the same social provider you will be registered and an account set ip for you.
We will not tweet or post on your behalf. We will retrieve your email address which we will use for billing should you ever exceed the resource limits.

If you choose to register directly with us using an email address and password, then a verification email will be sent to you.
While, of course, it shows marvelous cunning to use one of those short term email address sites, it does rather defeat the whole purpose of the site. We will only ever contact you to tell you about new developments and to remind you in the unlikely event you exceed the usage limits. If your email address no longer works we will assume you don't need our services and block your access.
 
The registration dialog looks like this:

![Register](/images/register.png)

# Access to the API
When you are logged in to the site you are also logged in to the API, so you can access it directly via the various interfaces. 

However, this won't work if you want to access the API from your web server or a mobile app. For this case we provide each user with an API key.
To use this add the following header to your accesses:
```
"Authorization": "Basic <your api key here>"
```

You can find your API key with the query ```GetAPIKey``` while logged in through one of our interfaces.

You can change the API key by calling the mutation ```ResetAPIKey```

# Editing interfaces

Because GraphQL APIs have consistent interfaces and are self-documenting, there are a range of tools already built that work with any GraphQL API. We've incorporated three of these and one interface specific to DARL.

## The home page containing a Playground interface

The playground interface is described [here](https://github.com/prisma/graphql-playground).

Briefly it enables you to develop _Queries_ and _Mutations_ and use them directly on live data. The Playground interface is preferred over the GraphiQL interface, which we also support, because it is possible to set up headers, so you can test access to the API by logging out and setting the header value in Playground.

You can enable headers by clicking on the top right cog-shaped settings button and setting ```"request.credentials": "include"```.

![playground](/images/darl_dev_index.png)

## The Voyager interface

This shows diagrammatically the objects supported by our API, their fields and how they are related.
full details can be found [here](https://github.com/APIs-guru/graphql-voyager).

![voyager](/images/darl_dev_voyager.png)

## The GraphiQL interface
This interface is included because it may be more familiar than playground. You can read more about it [here](https://github.com/graphql/graphiql).

![graphiql](/images/darl_dev_graphiql.png)

## The DARL editor

Although you can access all the data items in the API through the interfaces, including the text of rulesets, it is much more convenient to edit DARL in a dedicated syntax-checking editor.
The DARL editor plug-in is described in the [DARL editor](./darl_editor) page, and the DARL language is described in the [DARL](./darl) page.
Depending on whether or not you are logged in the drop down selection shows either your rule sets or the demo collection.

If the demo collection, the save button, which calls a _mutation_, will not work.

![darl editor](/images/darl_dev_darl_editor.png)

### Editing using an API key
If you are logged in you can only edit records associated with your account.  You can edit on behalf of another account if you know the APIKey.
Pass the API key as a parameter. For instance:

```
https://darl.dev/darleditor?apikey=api_key_here"
```

## Bug or request reporting

You can talk to the support team through the API using the following mutation:

```
mutation
{
  createSupportRequest(customerName: "Dr Andy", customerEmail: "support@darl.ai", text: "there's something wrong")
}
```



