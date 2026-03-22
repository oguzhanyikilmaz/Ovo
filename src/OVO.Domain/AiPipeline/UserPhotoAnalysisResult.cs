namespace OVO.AiPipeline;

public class UserPhotoAnalysisResult
{
    public double QualityScore { get; set; }

    public bool HasFace { get; set; }

    public bool IsFullBody { get; set; }
}
