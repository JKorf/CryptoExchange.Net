using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.Trackers.UserData.Objects
{
    /// <summary>
    /// Tracker for symbols used in UserDataTracker
    /// </summary>
    public class UserDataSymbolTracker
    {
        private readonly ILogger _logger;
        private readonly List<SharedSymbol> _trackedSymbols;
        private readonly bool _onlyTrackProvidedSymbols;
        private readonly object _symbolLock = new object();

        /// <summary>
        /// ctor
        /// </summary>
        public UserDataSymbolTracker(ILogger logger, UserDataTrackerConfig config)
        {
            _logger = logger;
            _trackedSymbols = config.TrackedSymbols?.ToList() ?? [];
            _onlyTrackProvidedSymbols = config.OnlyTrackProvidedSymbols;
        }

        /// <summary>
        /// Get currently tracked symbols
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SharedSymbol> GetTrackedSymbols()
        {
            lock (_symbolLock)            
                return _trackedSymbols.ToList();            
        }

        /// <summary>
        /// Check whether a symbol is in the tracked symbols list and should be processed
        /// </summary>
        public bool ShouldProcess(SharedSymbol symbol)
        {
            if (!_onlyTrackProvidedSymbols)
                return true;

            return _trackedSymbols.Any(y => y.TradingMode == symbol!.TradingMode && y.BaseAsset == symbol.BaseAsset && y.QuoteAsset == symbol.QuoteAsset);
        }

        /// <summary>
        /// Update the tracked symbol list with potential new symbols
        /// </summary>
        public void UpdateTrackedSymbols(IEnumerable<SharedSymbol> symbols, bool addByUser = false)
        {
            if (!addByUser && _onlyTrackProvidedSymbols)
                return;

            lock (_symbolLock)
            {
                foreach (var symbol in symbols.Distinct())
                {
                    if (!_trackedSymbols.Any(x => x.TradingMode == symbol.TradingMode && x.BaseAsset == symbol.BaseAsset && x.QuoteAsset == symbol.QuoteAsset))
                    {
                        _trackedSymbols.Add(symbol);
                        _logger.LogDebug("Adding {TradingMode}.{BaseAsset}/{QuoteAsset} to symbol tracking list", symbol.TradingMode, symbol.BaseAsset, symbol.QuoteAsset);
                    }
                }
            }
        }

        /// <summary>
        /// Remove a symbol from the list
        /// </summary>
        public void RemoveTrackedSymbol(SharedSymbol symbol)
        {
            lock (_symbolLock)
            {
                var symbolToRemove = _trackedSymbols.SingleOrDefault(x => x.TradingMode == symbol.TradingMode && x.BaseAsset == symbol.BaseAsset && x.QuoteAsset == symbol.QuoteAsset);
                if (symbolToRemove != null)
                    _trackedSymbols.Remove(symbolToRemove);
            }
        }
    }
}
