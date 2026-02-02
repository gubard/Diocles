using System.Text.Json;
using Gaia.Models;
using Gaia.Services;
using Hestia.Contract.Models;
using Hestia.Contract.Services;
using Neotoma.Contract.Models;
using Neotoma.Contract.Services;

namespace Diocles.Services;

public sealed class FileStorageHttpService(
    HttpClient httpClient,
    JsonSerializerOptions options,
    ITryPolicyService tryPolicyService,
    IFactory<Memory<HttpHeader>> headersFactory
)
    : HttpService<NeotomaGetRequest, NeotomaPostRequest, NeotomaGetResponse, NeotomaPostResponse>(
        httpClient,
        options,
        tryPolicyService,
        headersFactory
    ),
        IFileStorageHttpService
{
    protected override NeotomaGetRequest CreateHealthCheckGetRequest()
    {
        return new();
    }
}
