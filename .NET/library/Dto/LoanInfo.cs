using OneBeyondApi.Model;

namespace OneBeyondApi.Dto;

public class LoanInfo
{
    public required Borrower Borrower { get; init; }
    public BookInfo[] Books{ get; init; } = [];
}

public class BookInfo
{
    public Guid BookId { get; init; }
    public required string Title { get; init; }
}