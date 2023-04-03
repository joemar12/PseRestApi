using AutoMapper;
using PseRestApi.Core.ResponseModels;
using PseRestApi.Domain.Entities;

namespace PseRestApi.Core.Mappers;

public class HistoricalTradingDataMappingProfile : Profile
{
    public HistoricalTradingDataMappingProfile()
    {
        CreateMap<StockCompany, SecurityInfo>(MemberList.Source);
        CreateMap<StockHeader, HistoricalTradingData>(MemberList.Source);
    }
}