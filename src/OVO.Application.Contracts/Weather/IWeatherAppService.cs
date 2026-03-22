using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace OVO.Weather;

public interface IWeatherAppService : IApplicationService
{
    Task<WeatherDto> GetAsync(string? city = null);
}
