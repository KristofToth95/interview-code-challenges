using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess;

public class Fine
{
    public Guid Id { get; set; }
    public Guid BorrowerId { get; set; }
    public Borrower Borrower { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = null!;
    public DateTime IssuedAt { get; set; }
}