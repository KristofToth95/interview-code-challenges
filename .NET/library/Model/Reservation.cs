namespace OneBeyondApi.Model;

public class Reservation
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Book Book { get; set; } = null!;
    public Guid BorrowerId { get; set; }
    public Borrower Borrower { get; set; } = null!;
    public DateTime ReservedAt { get; set; }
    public bool IsFulfilled { get; set; }
}