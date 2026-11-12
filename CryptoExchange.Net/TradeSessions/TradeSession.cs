using CryptoExchange.Net.TradeSessions.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.TradeSessions;

/// <summary>
/// Trading strategy
/// </summary>
public abstract partial class TradeSession
{
    private TradeSessionContext? _context;
    private TradeSessionDispatcher? _dispatcher;

    protected TradeSessionContext Context => _context
        ?? throw new InvalidOperationException("Build the strategy before accessing its context.");

    /// <summary>
    /// The active order books managed by the session
    /// </summary>
    protected TradeSessionOrderBooks OrderBooks => Context.OrderBooks;

    protected virtual Task OnStartedAsync(CancellationToken ct) => Task.CompletedTask;

    /// <summary>
    /// Update callback, only called when an interval greater than 0 is set by using
    /// <see cref="TradeSessionBuilder{TStrategy, TSelf, TUser}.AddUpdateLoop(TimeSpan)"/>.
    /// </summary>
    protected virtual Task UpdateAsync(CancellationToken ct) => Task.CompletedTask;

    protected virtual Task OnStoppingAsync(CancellationToken ct) => Task.CompletedTask;

    internal void Attach(TradeSessionContext context, TradeSessionDispatcher dispatcher)
    {
        if (_context != null)
            throw new InvalidOperationException("The strategy already belongs to a host.");

        _context = context;
        _dispatcher = dispatcher;
    }

    internal void Dispatch(Action callback) => _dispatcher!.Dispatch(callback);

    internal Task DispatchAsync(Func<Task> callback) => _dispatcher!.DispatchAsync(callback);

    internal Task InvokeStartedAsync(CancellationToken ct) => OnStartedAsync(ct);

    internal Task InvokeUpdateAsync(CancellationToken ct) => UpdateAsync(ct);

    internal Task InvokeStoppingAsync(CancellationToken ct) => OnStoppingAsync(ct);
}

/// <inheritdoc />
public abstract class TradingStrategy<TUser> : TradeSession
    where TUser : class, ITradingUser
{
    protected new TradingContext<TUser> Context => (TradingContext<TUser>)base.Context;

    /// <summary>
    /// User access
    /// </summary>
    protected TradingSessionUsers<TUser> User => Context.User;
}
