using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;

namespace OVO.Weather;

[AllowAnonymous]
public class WeatherAppService : OVOAppService, IWeatherAppService
{
    public virtual Task<WeatherDto> GetAsync(string? city = null)
    {
        var resolved = string.IsNullOrWhiteSpace(city) ? "İstanbul" : city!;
        return Task.FromResult(new WeatherDto
        {
            City = resolved,
            TemperatureC = 12,
            Condition = "Yağmurlu",
            OutfitHint = "Trençkot ve bot önerdik."
        });
    }
}
