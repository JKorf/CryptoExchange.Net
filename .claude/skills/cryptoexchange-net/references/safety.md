# Operational considerations

## Credentials and account identity

The concrete exchange client controls credentials, signing, account selection, and environments. Required permissions, passphrases, wallet signers, and account identifiers vary by library. Capability metadata declares authentication requirements, not that the intended account is configured or authorized. Keep credentials out of source, logs, and examples. Select the exchange and environment explicitly before resolving authenticated operations.

## Orders, cancellation, and positions

Shared contracts normalize fields without guaranteeing identical execution semantics. Confirm supported quantity units, order types, time in force, client-order-id rules, reduce-only behavior, and position or margin mode on the selected implementation. Rounding to an increment does not validate minimum notional, balance, leverage, or exchange filters. A limit order can execute immediately; its price does not make it intrinsically safe.

Cancellation can race with fills. Reconcile order and trade state after cancellation rather than treating its acknowledgement as proof of no execution. `ICloseFullPosition` targets the whole position. Shared APIs do not supply a cross-exchange position-mode transaction or automatically translate unsupported semantics.

## Ambiguous and partial outcomes

A timeout, disconnect, or cancellation after submission can leave an order, transfer, or withdrawal accepted remotely. Check exchange state and use supported correlation/client identifiers before deciding whether to repeat an account-changing request. A client identifier is not a universal idempotency guarantee.

Inspect both the outer result and each item where an endpoint returns batch or nested outcomes. Successful operations across exchanges are independent: partial success has no automatic rollback. Keep exchange/account identity and exchange-specific identifiers with every result; a shared abstraction does not make identifiers interchangeable.

## Market data and request coordination

REST snapshots, cached symbol catalogs, and socket updates have different freshness and completeness guarantees. Handle subscription interruption and resynchronization according to the concrete implementation. Gross price differences exclude fees, executable size, inventory, funding, and transfer delays.

Rate-limit admission controls local request acceptance, not order validity or account state. A coalesced public GET may share an in-flight request; it does not provide atomic multi-exchange reads. Pagination can observe changing account state between pages. Keep cancellation and partial results explicit in workflows that combine multiple calls.
