using CryptoExchange.Net.Objects;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Shared interfaces utilities
    /// </summary>
    public static class SharedUtils
    {
        /// <summary>
        /// Get client information including supported features
        /// </summary>
        public static SharedClientInfo GetClientInfo(PlatformInfo platformInfo, ISharedClient client)
        {
            return new SharedClientInfo
            {
                Exchange = client.Exchange,
                TypeName = client.GetType().Name,
                SupportedEnvironments = platformInfo.SupportedEnvironments,
                SupportedTradingModes = client.SupportedTradingModes,
                CentralizationType = platformInfo.CentralizationType,
                Features = GetAllEndpointOptions(client)
            };
        }

        /// <summary>
        /// Get all supported endpoints for a client
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static EndpointOptions[] GetAllEndpointOptions(ISharedClient client)
        {
            var clientType = client.GetType();
            var result = new List<EndpointOptions>();
            if (client is IAssetsRestClient assetClient)
            {
                result.Add(assetClient.GetAssetOptions);
                result.Add(assetClient.GetAssetsOptions);
            }
            if (client is IBalanceRestClient balanceClient)
                result.Add(balanceClient.GetBalancesOptions);
            if (client is IDepositRestClient depositClient)
            {
                result.Add(depositClient.GetDepositAddressesOptions);
                result.Add(depositClient.GetDepositsOptions);
            }
            if (client is IKlineRestClient klineClient)
                result.Add(klineClient.GetKlinesOptions);
            if (client is IOrderBookRestClient orderBookClient)
                result.Add(orderBookClient.GetOrderBookOptions);
            if (client is IRecentTradeRestClient recentTradeClient)
                result.Add(recentTradeClient.GetRecentTradesOptions);
            if (client is ITradeHistoryRestClient tradeHistoryClient)
                result.Add(tradeHistoryClient.GetTradeHistoryOptions);
            if (client is IWithdrawalRestClient withdrawalClient)
                result.Add(withdrawalClient.GetWithdrawalsOptions);
            if (client is IWithdrawRestClient withdrawClient)
                result.Add(withdrawClient.WithdrawOptions);
            if (client is IFeeRestClient feeClient)
                result.Add(feeClient.GetFeeOptions);
            if (client is IBookTickerRestClient bookTickerClient)
                result.Add(bookTickerClient.GetBookTickerOptions);
            if (client is ITransferRestClient transferClient)
                result.Add(transferClient.TransferOptions);

            if (client is ISpotOrderRestClient spotOrderClient)
            {
                result.Add(spotOrderClient.PlaceSpotOrderOptions);
                result.Add(spotOrderClient.CancelSpotOrderOptions);
                result.Add(spotOrderClient.GetClosedSpotOrdersOptions);
                result.Add(spotOrderClient.GetOpenSpotOrdersOptions);
                result.Add(spotOrderClient.GetSpotOrderOptions);
                result.Add(spotOrderClient.GetSpotOrderTradesOptions);
                result.Add(spotOrderClient.GetSpotUserTradesOptions);
            }
            if (client is ISpotSymbolRestClient spotSymbolClient)
                result.Add(spotSymbolClient.GetSpotSymbolsOptions);
            if (client is ISpotTickerRestClient spotTickerClient)
            {
                result.Add(spotTickerClient.GetSpotTickerOptions);
                result.Add(spotTickerClient.GetSpotTickersOptions);
            }
            if (client is ISpotTriggerOrderRestClient spotTriggerOrderClient)
            {
                result.Add(spotTriggerOrderClient.CancelSpotTriggerOrderOptions);
                result.Add(spotTriggerOrderClient.GetSpotTriggerOrderOptions);
                result.Add(spotTriggerOrderClient.PlaceSpotTriggerOrderOptions);
            }
            if (client is ISpotOrderClientIdRestClient spotOrderClientIdClient)
            {
                result.Add(spotOrderClientIdClient.CancelSpotOrderByClientOrderIdOptions);
                result.Add(spotOrderClientIdClient.GetSpotOrderByClientOrderIdOptions);
            }

            if (client is IFundingRateRestClient fundingRateClient)
                result.Add(fundingRateClient.GetFundingRateHistoryOptions);
            if (client is IFuturesOrderRestClient futuresOrderClient)
            {
                result.Add(futuresOrderClient.CancelFuturesOrderOptions);
                result.Add(futuresOrderClient.ClosePositionOptions);
                result.Add(futuresOrderClient.GetClosedFuturesOrdersOptions);
                result.Add(futuresOrderClient.GetFuturesOrderOptions);
                result.Add(futuresOrderClient.GetFuturesOrderTradesOptions);
                result.Add(futuresOrderClient.GetFuturesUserTradesOptions);
                result.Add(futuresOrderClient.GetOpenFuturesOrdersOptions);
                result.Add(futuresOrderClient.GetPositionsOptions);
                result.Add(futuresOrderClient.PlaceFuturesOrderOptions);
            }
            if (client is IFuturesSymbolRestClient futuresSymbolClient)
                result.Add(futuresSymbolClient.GetFuturesSymbolsOptions);
            if (client is IFuturesTickerRestClient futuresTickerClient)
            {
                result.Add(futuresTickerClient.GetFuturesTickerOptions);
                result.Add(futuresTickerClient.GetFuturesTickersOptions);
            }
            if (client is IIndexPriceKlineRestClient indexPriceKlineClient)
                result.Add(indexPriceKlineClient.GetIndexPriceKlinesOptions);
            if (client is ILeverageRestClient leverageClient)
            {
                result.Add(leverageClient.GetLeverageOptions);
                result.Add(leverageClient.SetLeverageOptions);
            }
            if (client is IMarkPriceKlineRestClient markPriceKlineClient)
                result.Add(markPriceKlineClient.GetMarkPriceKlinesOptions);
            if (client is IOpenInterestRestClient openInterestClient)
                result.Add(openInterestClient.GetOpenInterestOptions);
            if (client is IPositionHistoryRestClient positionHistoryClient)
                result.Add(positionHistoryClient.GetPositionHistoryOptions);
            if (client is IPositionModeRestClient positionModeClient)
            {
                result.Add(positionModeClient.SetPositionModeOptions);
                result.Add(positionModeClient.GetPositionModeOptions);
            }
            if (client is IFuturesTpSlRestClient futuresTpSlClient)
            {
                result.Add(futuresTpSlClient.SetFuturesTpSlOptions);
                result.Add(futuresTpSlClient.CancelFuturesTpSlOptions);
            }
            if (client is IFuturesTriggerOrderRestClient futuresTriggerOrderClient)
            {
                result.Add(futuresTriggerOrderClient.CancelFuturesTriggerOrderOptions);
                result.Add(futuresTriggerOrderClient.GetFuturesTriggerOrderOptions);
                result.Add(futuresTriggerOrderClient.PlaceFuturesTriggerOrderOptions);
            }
            if (client is IFuturesOrderClientIdRestClient futuresOrderClientIdClient)
            {
                result.Add(futuresOrderClientIdClient.GetFuturesOrderByClientOrderIdOptions);
                result.Add(futuresOrderClientIdClient.CancelFuturesOrderByClientOrderIdOptions);
            }

            if (client is IBalanceSocketClient balanceSocketClient)
                result.Add(balanceSocketClient.SubscribeBalanceOptions);
            if (client is IBookTickerSocketClient bookTickerSocketClient)
                result.Add(bookTickerSocketClient.SubscribeBookTickerOptions);
            if (client is IKlineSocketClient klineSocketClient)
                result.Add(klineSocketClient.SubscribeKlineOptions);
            if (client is IOrderBookSocketClient orderBookSocketClient)
                result.Add(orderBookSocketClient.SubscribeOrderBookOptions);
            if (client is ITickerSocketClient tickerSocketClient)
                result.Add(tickerSocketClient.SubscribeTickerOptions);
            if (client is ITickersSocketClient tickersSocketClient)
                result.Add(tickersSocketClient.SubscribeAllTickersOptions);
            if (client is ITradeSocketClient tradeSocketClient)
                result.Add(tradeSocketClient.SubscribeTradeOptions);
            if (client is IUserTradeSocketClient userTradeSocketClient)
                result.Add(userTradeSocketClient.SubscribeUserTradeOptions);

            if (client is ISpotOrderSocketClient spotOrderSocketClient)
                result.Add(spotOrderSocketClient.SubscribeSpotOrderOptions);

            if (client is IFuturesOrderSocketClient futuresOrderSocketClient)
                result.Add(futuresOrderSocketClient.SubscribeFuturesOrderOptions);
            if (client is IPositionSocketClient positionSocketClient)
                result.Add(positionSocketClient.SubscribePositionOptions);

            return result.ToArray();
        }

        public static T[] ApplySymbolFilter<T>(T[] symbols, GetSymbolsRequest request) where T : SharedSpotSymbol
        {
            IEnumerable<T> resultData = symbols;
            if (request.TradingMode != null)
                resultData = resultData.Where(x => x.TradingMode == request.TradingMode);
            if (request.BaseAssetType != null)
                resultData = resultData.Where(x => x.BaseAssetType == request.BaseAssetType);
            if (request.QuoteAssetType != null)
                resultData = resultData.Where(x => x.QuoteAssetType == request.QuoteAssetType);
            if (request.BaseAssetSubType != null)
                resultData = resultData.Where(x => x.BaseAssetSubType == request.BaseAssetSubType);
            if (request.QuoteAssetSubType != null)
                resultData = resultData.Where(x => x.QuoteAssetSubType == request.QuoteAssetSubType);
            return resultData.ToArray();
        }
    }
}
