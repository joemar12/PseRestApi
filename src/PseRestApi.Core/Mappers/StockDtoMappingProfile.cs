using AutoMapper;
using PseRestApi.Core.Dto;
using PseRestApi.Core.ResponseModels;
using PseRestApi.Domain.Entities;

namespace PseRestApi.Core.Mappers;

public class StockDtoMappingProfile : Profile
{
    public StockDtoMappingProfile()
    {
        CreateMap<StockHeader, StockPrice>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.LastTradePrice))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency));
        CreateMap<StockHeader, Stock>()
            .ForMember(dest => dest.LastTradeDate, opt => opt.MapFrom(x => x.LastTradedDate))
            .ForMember(dest => dest.PercentChange, opt => opt.MapFrom(src => src.PercChangeClose))
            .ForMember(dest => dest.Volume, opt => opt.MapFrom(src => src.TotalVolume));
        CreateMap<IEnumerable<StockHeader>, Stock>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(x => x))
            .ForMember(dest => dest.LastTradeDate, opt => opt.MapFrom(x => (x.FirstOrDefault() ?? new StockHeader()).LastTradedDate));
        CreateMap<StockHeaderResponse, Stock>()
            .IncludeMembers(x => x.Records);

        CreateMap<StockCompany, Stock>()
            .ForMember(dest => dest.SecurityName, opt => opt.MapFrom(src => src.SecurityName))
            .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Symbol));

        CreateMap<StockSummaryResponse, Stock>()
            .ForMember(dest => dest.PercentChange, opt => opt.MapFrom(src => src.PercChangeClose))
            .ForMember(dest => dest.Volume, opt => opt.MapFrom(src => src.TotalVolume));

        CreateMap<HistoricalTradingData, StockPrice>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.LastTradePrice))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency));

        CreateMap<HistoricalTradingData, Stock>()
            .ForMember(dest => dest.SecurityName, opt => opt.MapFrom(src => src.SecurityInfo.SecurityName))
            .ForMember(dest => dest.PercentChange, opt => opt.MapFrom(src => src.PercChangeClose))
            .ForMember(dest => dest.Volume, opt => opt.MapFrom(src => src.TotalVolume))
            .ForMember(dest => dest.LastTradeDate, opt => opt.MapFrom(src => src.LastTradedDate))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src));
    }

}
public class HistoricalTradingDataMappingProfile : Profile
{
    public HistoricalTradingDataMappingProfile()
    {
        CreateMap<StockCompany, SecurityInfo>()
            .ForMember(dest => dest.SecurityId, opt => opt.MapFrom(src => src.SecurityId))
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
            .ForMember(dest => dest.SecurityName, opt => opt.MapFrom(src => src.SecurityName))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName))
            .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Symbol))
            .ForMember(dest => dest.SecurityStatus, opt => opt.MapFrom(src => src.SecurityStatus));
    }
}