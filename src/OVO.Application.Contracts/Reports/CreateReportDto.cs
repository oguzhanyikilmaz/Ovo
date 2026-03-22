namespace OVO.Reports;

public class CreateReportDto
{
    public string ContentType { get; set; } = default!;

    public string ContentId { get; set; } = default!;

    public string Reason { get; set; } = default!;
}
