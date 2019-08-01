---
title: Collateral page
description: Adding collateral to bots.
output:
  html_document:
    toc: true
    toc_float: true
---

Collateral page
======
Bot channels typically use markdown for display purposes. So a store of pre-made markdown text is useful.
These files can be displayed through a bot by using the "Collateral" [store](./stores).
```darl
if anything then response will be Collateral["index.md"];
```


This page enables you to upload your markdown collateral.

![Collateral image](/images/collateral.png)

## Accessing Collateral in your account via the API
```json
{
 collateral
  {
    name
    value
  }
}
```


