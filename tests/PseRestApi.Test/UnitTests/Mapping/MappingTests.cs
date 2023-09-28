using AutoFixture;
using AutoMapper;
using FluentAssertions;
using FluentAssertions.Execution;
using PseRestApi.Core.Dto;
using PseRestApi.Core.Mappers;
using PseRestApi.Core.ResponseModels;
using PseRestApi.Domain.Entities;
using Xunit;

namespace PseRestApi.Test.UnitTests.Mapping;
public class MappingTests
{
    private readonly IFixture _fixture;
    private readonly IMapper _mapper;

    public MappingTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<StockDtoMappingProfile>();
            cfg.AddProfile<HistoricalTradingDataMappingProfile>();
        });

        _mapper = new Mapper(mapperConfig);
        _fixture = new Fixture();
    }

    [Fact]
    public void ShouldMapSecurityNameAndSymbolFromApiResponse()
    {
        var source = _fixture.Create<StockCompany>();
        var destination = _mapper.Map<Stock>(source);

        using (new AssertionScope())
        {
            destination.Should().NotBeNull();
            destination.SecurityName.Should().Be(source.SecurityName);
            destination.Symbol.Should().Be(source.Symbol);
        }
    }

    [Fact]
    public void ShouldMapStockPriceDetailsFromApiResponse()
    {
        var mockResponse = _fixture.Create<StockHeaderResponse>();
        var destination = _mapper.Map<Stock>(mockResponse);

        var expected = mockResponse.Records.FirstOrDefault()!;
        using (new AssertionScope())
        {
            destination.Should().NotBeNull();
            destination.AsOfDate.Should().Be(expected.LastTradedDate);
            destination.PercentChange.Should().Be(expected.PercChangeClose);
            destination.Volume.Should().Be(expected.TotalVolume);
            destination.Price.Should().NotBeEmpty();

            var destPrice = destination.Price.FirstOrDefault()!;
            destPrice.Price.Should().Be(expected.LastTradePrice);
            destPrice.Currency.Should().Be(expected.Currency);
        };
    }

    [Fact]
    public void ShouldMapStockDtoFromArchiveData()
    {
        var securityInfo = _fixture
            .Build<SecurityInfo>()
            .Without(x => x.HistoricalTradingData)
            .Create();
        var source = _fixture
            .Build<HistoricalTradingData>()
            .Without(x => x.SecurityInfo)
            .Create();
        source.SecurityInfo = securityInfo;
        var destination = _mapper.Map<Stock>(source);

        using (new AssertionScope())
        {
            destination.Should().NotBeNull();
            destination.SecurityName.Should().Be(source.SecurityInfo.SecurityName);
            destination.Symbol.Should().Be(source.Symbol);
            destination.AsOfDate.Should().Be(source.LastTradedDate);
            destination.PercentChange.Should().Be(source.PercChangeClose);
            destination.Volume.Should().Be(source.TotalVolume);
            destination.Price.Should().NotBeEmpty();

            var destPrice = destination.Price.FirstOrDefault()!;
            destPrice.Price.Should().Be(source.LastTradePrice);
            destPrice.Currency.Should().Be(source.Currency);
        };
    }

    [Fact]
    public void ShouldMapFromApiResponseToSecurityInfoArchiveData()
    {
        var source = _fixture.Create<StockCompany>();
        var destination = _mapper.Map<SecurityInfo>(source);
        using (new AssertionScope())
        {
            destination.Should().NotBeNull();
            destination.SecurityStatus.Should().Be(source.SecurityStatus);
            destination.CompanyId.Should().Be(source.CompanyId);
            destination.Symbol.Should().Be(source.Symbol);
            destination.CompanyName.Should().Be(source.CompanyName);
            destination.SecurityId.Should().Be(source.SecurityId);
            destination.SecurityName.Should().Be(source.SecurityName);
        }
    }

    [Fact]
    public void ShouldMapFromApiResponseToStockPriceArchiveData()
    {
        var source = _fixture.Create<StockHeader>();
        source.CurrentPe = "1234";

        double? currentPeParsed = double.TryParse(source.CurrentPe, out var pe) ? pe : null;
        var destination = _mapper.Map<HistoricalTradingData>(source);
        using (new AssertionScope())
        {
            destination.Should().NotBeNull();
            destination.SqLow.Should().Be(source.SqLow);
            destination.FiftyTwoWeekHigh.Should().Be(source.FiftyTwoWeekHigh);
            destination.ChangeClose.Should().Be(source.ChangeClose);
            destination.ChangeClosePercChangeClose.Should().Be(source.ChangeClosePercChangeClose);
            destination.LastTradedDate.Should().Be(source.LastTradedDate);
            destination.TotalValue.Should().Be(source.TotalValue);
            destination.LastTradePrice.Should().Be(source.LastTradePrice);
            destination.SqHigh.Should().Be(source.SqHigh);
            destination.Currency.Should().Be(source.Currency);
            destination.PercChangeClose.Should().Be(source.PercChangeClose);
            destination.FiftyTwoWeekLow.Should().Be(source.FiftyTwoWeekLow);
            destination.SqPrevious.Should().Be(source.SqPrevious);
            destination.Symbol.Should().Be(source.Symbol);
            destination.CurrentPe.Should().Be(currentPeParsed);
            destination.SqOpen.Should().Be(source.SqOpen);
            destination.AvgPrice.Should().Be(source.AvgPrice);
            destination.TotalVolume.Should().Be(source.TotalVolume);
        }
    }
    [Fact]
    public void ShouldMapToNullCurrentPeIfSourceValueIsInvalid()
    {
        var source = _fixture.Create<StockHeader>();
        source.CurrentPe = "ddf";
        var destination = _mapper.Map<HistoricalTradingData>(source);
        using (new AssertionScope())
        {
            destination.Should().NotBeNull();
            destination.CurrentPe.Should().BeNull();
        }
    }
}
