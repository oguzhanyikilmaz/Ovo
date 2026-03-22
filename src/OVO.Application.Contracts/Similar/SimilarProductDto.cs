namespace OVO.Similar;

public class SimilarProductDto
{
    public string Vendor { get; set; } = default!;

    public string Title { get; set; } = default!;

    public decimal PriceTry { get; set; }

    public string Url { get; set; } = default!;

    public double Similarity { get; set; }
}
