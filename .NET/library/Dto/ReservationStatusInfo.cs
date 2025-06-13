namespace OneBeyondApi.Dto;

public class ReservationStatusInfo
{
    public required int PositionInQueue { get; set; }
    public required DateTime EstimatedAvailableDate { get; set; }
}