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

        CreateMap<IEnumerable<StockHeader>, Stock>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(x => x))
            .ForMember(dest => dest.LastTradeDate, opt => opt.MapFrom(x => (x.FirstOrDefault() ?? new StockHeader()).LastTradedDate))
            .ForMember(dest => dest.PercentChange, opt => opt.MapFrom(src => (src.FirstOrDefault() ?? new StockHeader()).PercChangeClose))
            .ForMember(dest => dest.Volume, opt => opt.MapFrom(src => (src.FirstOrDefault() ?? new StockHeader()).TotalVolume))
            .ForMember(dest => dest.SecurityName, opt => opt.Ignore())
            .ForMember(dest => dest.Symbol, opt => opt.Ignore());

        CreateMap<StockHeaderResponse, Stock>()
            .IncludeMembers(x => x.Records)
            .ForMember(dest => dest.SecurityName, opt => opt.Ignore())
            .ForMember(dest => dest.Symbol, opt => opt.Ignore());

        CreateMap<StockCompany, Stock>()
            .ForMember(dest => dest.SecurityName, opt => opt.MapFrom(src => src.SecurityName))
            .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Symbol))
            .ForMember(dest => dest.Price, opt => opt.Ignore())
            .ForMember(dest => dest.LastTradeDate, opt => opt.Ignore())
            .ForMember(dest => dest.PercentChange, opt => opt.Ignore())
            .ForMember(dest => dest.Volume, opt => opt.Ignore());

        CreateMap<HistoricalTradingData, StockPrice>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.LastTradePrice))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency));

        CreateMap<HistoricalTradingData, Stock>()
            .ForMember(dest => dest.SecurityName, opt => opt.MapFrom(src => src.SecurityInfo.SecurityName))
            .ForMember(dest => dest.PercentChange, opt => opt.MapFrom(src => src.PercChangeClose))
            .ForMember(dest => dest.Volume, opt => opt.MapFrom(src => src.TotalVolume))
            .ForMember(dest => dest.LastTradeDate, opt => opt.MapFrom(src => src.LastTradedDate))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => new List<HistoricalTradingData>() { src }));
    }
}
