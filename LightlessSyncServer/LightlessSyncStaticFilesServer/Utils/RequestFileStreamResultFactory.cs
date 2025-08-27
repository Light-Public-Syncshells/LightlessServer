using LightlessSyncShared.Metrics;
using LightlessSyncShared.Services;
using LightlessSyncShared.Utils.Configuration;
using LightlessSyncStaticFilesServer.Services;

namespace LightlessSyncStaticFilesServer.Utils;

public class RequestFileStreamResultFactory
{
    private readonly LightlessMetrics _metrics;
    private readonly RequestQueueService _requestQueueService;
    private readonly IConfigurationService<StaticFilesServerConfiguration> _configurationService;

    public RequestFileStreamResultFactory(LightlessMetrics metrics, RequestQueueService requestQueueService, IConfigurationService<StaticFilesServerConfiguration> configurationService)
    {
        _metrics = metrics;
        _requestQueueService = requestQueueService;
        _configurationService = configurationService;
    }

    public RequestFileStreamResult Create(Guid requestId, Stream stream)
    {
        return new RequestFileStreamResult(requestId, _requestQueueService,
            _metrics, stream, "application/octet-stream");
    }
}