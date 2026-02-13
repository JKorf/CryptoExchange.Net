using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.Trackers.UserData.Objects
{
    public class UserDataSymbolTracker
    {
        private readonly ILogger _logger;
        private readonly List<SharedSymbol> _trackedSymbols;
        private readonly bool _onlyTrackProvidedSymbols;
        private readonly object _symbolLock = new object();

        public UserDataSymbolTracker(ILogger logger, UserDataTrackerConfig config)
        {
            _logger = logger;
            _trackedSymbols = config.TrackedSymbols?.ToList() ?? [];
            _onlyTrackProvidedSymbols = config.OnlyTrackProvidedSymbols;
        }

        public IEnumerable<SharedSymbol> GetTrackedSymbols()
        {
            lock (_symbolLock)            
                return _trackedSymbols.ToList();            
        }

        public bool ShouldProcess(SharedSymbol symbol)
        {
            if (!_onlyTrackProvidedSymbols)
                return true;

            return _trackedSymbols.Any(y => y.TradingMode == symbol!.TradingMode && y.BaseAsset == symbol.BaseAsset && y.QuoteAsset == symbol.QuoteAsset);
        }

        /// <summary>
        /// Update the tracked symbol list with potential new symbols
        /// </summary>
        /// <param name="symbols"></param>
        public void UpdateTrackedSymbols(IEnumerable<SharedSymbol> symbols)
        {
            if (_onlyTrackProvidedSymbols)
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
    }
}
