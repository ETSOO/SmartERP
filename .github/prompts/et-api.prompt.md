---
agent: "ask"
description: "Generate TS API from c# endpoint declaration"
---

## Task

Convert to TypeScript function.

## Input

Selected code: #selection

## Guidelines

- Follow examples inside /Admin/admin.client/src/api/QueryApi.ts, with same naming style.
- Only deal with the method whose name contains the #selection.
- Don't analyze the dependency of the endpoint, just convert the method declaration to a TypeScript function.
- Replace the endpoint name with '${this.flag}' to dynamically generate the endpoint URL.
- For the payload parameter, use 'ResultPayload' in TS when the return type is 'IActionResult' in C#. Use the same structure as the C# method parameters, but convert them to TypeScript types.
