using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PseRestApi.Core.ResponseModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PseRestApi.Core.Services
{
    public class PseClient : IPseClient
    {
        private readonly IFlurlClient _client;
        public PseClient(IFlurlClientFactory clientFactory, IOptions<PseApiOptions> options)
        {

            IFlurlClient client = clientFactory.Get(options.Value.BaseUrl);
            client.Settings.BeforeCall = call =>
            {
                call.Request.WithHeader("Referer", options.Value.Referer);
            };
            _client = client;
        }

        public async Task<StockCompanyResponse> FindSecurityOrCompany(string symbol)
        {
            var queryString = new
            {
                method = "findSecurityOrCompany",
                ajax = true,
                start = 0,
                limit = 1,
                query = symbol
            };
            return await _client.Request()
                .AppendPathSegment($"stockMarket/home.html")
                .SetQueryParams(queryString)
                .GetJsonAsync<StockCompanyResponse>();
        }

        public async Task<IEnumerable<StockSummaryResponse>> GetAllStockSummary()
        {
            var queryString = new
            {
                method = "getSecuritiesAndIndicesForPublic",
                ajax = true
            };
            return await _client.Request()
                .AppendPathSegment($"stockMarket/home.html")
                .SetQueryParams(queryString)
                .GetJsonAsync<IEnumerable<StockSummaryResponse>>();
        }

        public async Task<StockHeaderResponse> GetStockHeader(int companyId, int securityId)
        {
            var queryString = new
            {
                method = "fetchHeaderData",
                ajax = true,
                company = companyId,
                security = securityId
            };
            return await _client.Request()
                .AppendPathSegment($"stockMarket/companyInfo.html")
                .SetQueryParams(queryString)
                .GetJsonAsync<StockHeaderResponse>();
        }
    }
}
