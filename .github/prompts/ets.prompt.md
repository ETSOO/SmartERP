---
agent: "ask"
description: "Generate TS type from c# declaration"
---

## Task

1. Convert to TypeScript type (not interface) with 'export'.

## Guidelines

- Follow TypeScript naming policy.
- No necessary to add 'null' type in TS for nullable types.
- Keep the exact name of reference type, no necessary to generate it.
- Change type name 'QueryIntRQ', 'QueryLongRQ' to 'QueryRQ'.
- Keep all current comments, don't add your comments.
- For DateTime and DateTimeOffset, convert to 'Date | string'.
