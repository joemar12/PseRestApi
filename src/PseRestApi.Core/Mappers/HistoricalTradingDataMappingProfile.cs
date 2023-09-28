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

        CreateMap<StockFromFrames, HistoricalTradingData>(MemberList.None)
            .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.StockSymbol))
            .ForMember(dest => dest.TotalVolume, opt => opt.MapFrom(src => src.Volume))
            .ForMember(dest => dest.TotalValue, opt => opt.MapFrom(src => src.Value))
            .ForMember(dest => dest.LastTradePrice, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.PercChangeClose, opt => opt.MapFrom(src => src.PercentChange))
            .ForMember(dest => dest.ChangeClose, opt => opt.MapFrom(src => src.Change))
            .ForMember(dest => dest.LastTradedDate, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => "PHP"));

        CreateMap<StockFromFrames, SecurityInfo>(MemberList.None)
            .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.StockSymbol))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.StockName))
            .ForMember(dest => dest.SecurityName, opt => opt.MapFrom(src => src.StockName));
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