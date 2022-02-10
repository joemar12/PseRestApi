using AutoMapper;
using PseRestApi.Core.Dto;
using PseRestApi.Core.ResponseModels;

namespace PseRestApi.Core.Mappers
{
    public class StockDtoMappingProfile : Profile
    {
        public StockDtoMappingProfile()
        {
            CreateMap<StockHeader, StockPrice>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.headerLastTradePrice))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.currency));
            CreateMap<StockHeader, Stock>()
                .ForMember(dest => dest.LastTradeDate, opt => opt.MapFrom(x => x.lastTradedDate));
            CreateMap<IEnumerable<StockHeader>, Stock>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(x => x))
                .ForMember(dest => dest.LastTradeDate, opt => opt.MapFrom(x => (x.FirstOrDefault() ?? new StockHeader()).lastTradedDate));
            CreateMap<StockHeaderResponse, Stock>()
                .IncludeMembers(x => x.records);

            CreateMap<StockCompany, Stock>()
                .ForMember(dest => dest.SecurityName, opt => opt.MapFrom(src => src.securityName))
                .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.symbol));

            CreateMap<StockSummaryResponse, Stock>()
                .ForMember(dest => dest.PercentChange, opt => opt.MapFrom(src => src.percChangeClose))
                .ForMember(dest => dest.Volume, opt => opt.MapFrom(src => src.totalVolume));
        }
    }
}