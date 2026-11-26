# subscription-billing

Subscription lifecycle for a SaaS, modeled as a rich domain entity in C# / .NET 10 and built test-first with xUnit.

The business rules come from [AdvManager](https://advmanager.com.br), a SaaS I built and run in production.
This repository is a from-scratch, generic rewrite of its subscription management.

## Design principles

- **Rich domain model**: the subscription changes state only through its own business operations.
- **Explicit transitions**: invalid transitions are rejected with a clear error.
- **Deterministic time**: the current time is always passed in, so every rule is testable.
- **Test-driven**: each behavior starts as a failing test.

## Subscription states

| From | Event | To |
|---|---|---|
| — | Start trial | Trial |
| Trial | Request checkout | PendingPayment |
| PendingPayment | Payment confirmed | Active |
| Active | Payment overdue | PastDue (grace period starts) |

## Running the tests

```bash
dotnet test
```
