using AutoMapper;
using PseRestApi.Core.ResponseModels;
using PseRestApi.Domain.Entities;

namespace PseRestApi.Core.Mappers;

public class HistoricalTradingDataMappingProfile : Profile
{
    public HistoricalTradingDataMappingProfile()
    {
        CreateMap<StockCompany, SecurityInfo>(MemberList.Source);
        CreateMap<StockHeader, HistoricalTradingData>(MemberList.None)
            .ForMember(dest => dest.CurrentPe, opt => opt.MapFrom<CurrentPeResolver>());
    }
}

public class CurrentPeResolver : IValueResolver<StockHeader, HistoricalTradingData, double?>
{
    public double? Resolve(StockHeader source, HistoricalTradingData destination, double? destMember, ResolutionContext context)
    {
        double? result = double.TryParse(source.CurrentPe, out var currentPe) ? currentPe : null;
        result = result.HasValue &&
                (double.IsNaN(result.Value) ||
                double.IsInfinity(result.Value))
                ? null : result;
        return result;
    }
}