namespace OVO.Weather;

public class WeatherDto
{
    public string City { get; set; } = default!;

    public double TemperatureC { get; set; }

    public string Condition { get; set; } = default!;

    public string OutfitHint { get; set; } = default!;
}
