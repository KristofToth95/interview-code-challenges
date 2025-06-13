namespace OneBeyondApi.Dto;

public class BookReturnInfo
{
    public required string Message { get; set; }
    public required FineInfo Fine { get; set; }
    
}

public class FineInfo
{
    public required string Reason { get; set; }
    public required decimal Amount { get; set; }
    public required DateTime IssuedAt { get; set; }
}