using System.Text.Json;
using Gaia.Models;
using Gaia.Services;
using Hestia.Contract.Models;
using Hestia.Contract.Services;

namespace Diocles.Services;

public sealed class ToDoHttpService(
    HttpClient httpClient,
    JsonSerializerOptions options,
    ITryPolicyService tryPolicyService,
    IFactory<Memory<HttpHeader>> headersFactory
)
    : HttpService<HestiaGetRequest, HestiaPostRequest, HestiaGetResponse, HestiaPostResponse>(
        httpClient,
        options,
        tryPolicyService,
        headersFactory
    ),
        IToDoHttpService
{
    protected override HestiaGetRequest CreateHealthCheckGetRequest()
    {
        return new() { IsGetSelectors = true };
    }
}
