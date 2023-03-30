using AutoMapper;
using PseRestApi.Core.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PseRestApi.Core.Services;
public class HistoricalTradingDataSyncService : ISyncService
{
    private readonly IMapper _mapper;
    private readonly IPseClient _client;
    private readonly IAppDbContext _appDbContext;
    public HistoricalTradingDataSyncService(IPseClient client, IMapper mapper, IAppDbContext appDbContext)
    {
        _client = client;
        _mapper = mapper;
        _appDbContext = appDbContext;
    }

    public Task Sync()
    {
        throw new NotImplementedException();
    }
}

public interface ISyncService
{
    Task Sync();
}