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
- Replace the endpoint name with '${this.flag}' to dynamically generate the endpoint URL.
- For the payload, use the same structure as the C# method parameters, but convert them to TypeScript types.
