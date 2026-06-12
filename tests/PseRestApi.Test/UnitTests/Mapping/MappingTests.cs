using AutoFixture;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using PseRestApi.Core.ResponseModels;
using PseRestApi.Domain.Entities;
using Xunit;
using ManualMapper = PseRestApi.Core.Mappers.ManualMapper;

namespace PseRestApi.Test.UnitTests.Mapping;

public class MappingTests
{
    private readonly IFixture _fixture;

    public MappingTests()
    {
        _fixture = new Fixture();
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
        var destination = ManualMapper.MapToStock(source);

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
        }
        ;
    }

    private string GenerateRandomDoubleString()
    {
        var rnd = new Random();
        var seed = rnd.NextDouble();
        return (seed * 1000).ToString();
    }

    [Fact]
    public void ShouldMapFromApiResponseToStockPriceArchiveData()
    {
        var source = _fixture.Build<StockFromFrames>()
            .With(x => x.Value, () => GenerateRandomDoubleString())
            .With(x => x.Change, () => GenerateRandomDoubleString())
            .With(x => x.Price, () => GenerateRandomDoubleString())
            .With(x => x.PercentChange, () => GenerateRandomDoubleString())
            .Create();

        var destination = ManualMapper.MapToHistoricalTradingData(source);
        double? totalValue = string.IsNullOrEmpty(source.Value) ? null : double.Parse(source.Value);
        double? changeClose = string.IsNullOrEmpty(source.Change) ? null : double.Parse(source.Change);
        double? lastTradePrice = string.IsNullOrEmpty(source.Price) ? null : double.Parse(source.Price);
        double? percChangeClose = string.IsNullOrEmpty(source.PercentChange) ? null : double.Parse(source.PercentChange);
        using (new AssertionScope())
        {
            destination.Should().NotBeNull();
            destination.Symbol.Should().Be(source.StockSymbol);
            destination.TotalVolume.Should().Be(source.Volume);
            destination.TotalValue.Should().Be(totalValue);
            destination.LastTradePrice.Should().Be(lastTradePrice);
            destination.PercChangeClose.Should().Be(percChangeClose);
            destination.ChangeClose.Should().Be(changeClose);
            destination.Currency.Should().Be("PHP");
        }
    }
}
