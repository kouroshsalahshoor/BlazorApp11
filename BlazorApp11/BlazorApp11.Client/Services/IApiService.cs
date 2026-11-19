using BlazorApp11.Client.Models;

namespace BlazorApp11.Client.Services;

public interface IApiService
{
    Task<IEnumerable<Band>> CallLocalApiAsync();

    Task<IEnumerable<Band>> CallRemoteApiAsync();

}