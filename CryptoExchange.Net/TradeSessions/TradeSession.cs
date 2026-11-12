using CryptoExchange.Net.TradeSessions.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.TradeSessions;

/// <summary>
/// Trade session
/// </summary>
public abstract partial class TradeSession
{
    private TradeSessionContext? _context;
    private TradeSessionDispatcher? _dispatcher;
    /// <summary>
    /// Logger instance
    /// </summary>
    protected ILogger? _logger;

    /// <summary>
    /// The trade session context
    /// </summary>
    protected TradeSessionContext Context => _context
        ?? throw new InvalidOperationException("Build the session before accessing its context.");

    /// <summary>
    /// The active order books managed by the session
    /// </summary>
    protected TradeSessionOrderBooks OrderBooks => Context.OrderBooks;

    /// <summary>
    /// Starting callback, runs before creating or starting configured trackers
    /// </summary>
    protected virtual Task OnStartingAsync(CancellationToken ct) => Task.CompletedTask;

    /// <summary>
    /// Started callback, runs when all configured trackers are live
    /// </summary>
    protected virtual Task OnStartedAsync(CancellationToken ct) => Task.CompletedTask;

    /// <summary>
    /// Update callback, only called when an interval greater than 0 is set by using
    /// <see cref="TradeSessionBuilder{TTradeSession, TSelf, TUser}.AddUpdateLoop(TimeSpan)"/>.
    /// </summary>
    protected virtual Task UpdateAsync(CancellationToken ct) => Task.CompletedTask;

    /// <summary>
    /// Stopping callback, runs after stopping the update loop, before stopping data updates
    /// </summary>
    protected virtual Task OnStoppingAsync(CancellationToken ct) => Task.CompletedTask;

    /// <summary>
    /// Stopped callback, runs after all configured trackers have been disposed and updates have stopped
    /// </summary>
    protected virtual Task OnStoppedAsync(CancellationToken ct) => Task.CompletedTask;

    internal void Attach(TradeSessionContext context, TradeSessionDispatcher dispatcher, ILogger logger)
    {
        if (_context != null)
            throw new InvalidOperationException("The session already belongs to a host.");

        _context = context;
        _logger = logger;
        _dispatcher = dispatcher;
    }

    internal void Dispatch(Action callback) => _dispatcher!.Dispatch(callback);

    internal Task DispatchAsync(Func<Task> callback) => _dispatcher!.DispatchAsync(callback);

    internal void ReportException(Exception error) => _dispatcher!.ReportException(error);

    internal Task InvokeStartingAsync(CancellationToken ct) => OnStartingAsync(ct);

    internal Task InvokeStartedAsync(CancellationToken ct) => OnStartedAsync(ct);

    internal Task InvokeUpdateAsync(CancellationToken ct) => UpdateAsync(ct);

    internal Task InvokeStoppingAsync(CancellationToken ct) => OnStoppingAsync(ct);

    internal Task InvokeStoppedAsync(CancellationToken ct) => OnStoppedAsync(ct);
}

/// <inheritdoc />
public abstract class TradeSession<TUser> : TradeSession
    where TUser : class, ITradeSessionUser
{
    /// <inheritdoc />
    protected new TradeSessionContext<TUser> Context => (TradeSessionContext<TUser>)base.Context;

    /// <summary>
    /// User access
    /// </summary>
    protected TradeSessionUsers<TUser> User => Context.User;
}
