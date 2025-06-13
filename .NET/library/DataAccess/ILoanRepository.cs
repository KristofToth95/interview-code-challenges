using OneBeyondApi.Dto;
using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess;

public interface ILoanRepository
{
    public List<LoanInfo> GetActiveLoans();
    public void ReturnBook(BookStock bookStock, out Fine? fine);

    public BookStock? GetBookStockById(Guid id);
    bool AreAllCopiesOnLoan(Guid bookId);
    void AddReservation(Guid borrowerId, Guid bookId);
    Reservation? GetBorrowerReservation(Guid borrowerId, Guid bookId);
    int GetReservationQueuePosition(Reservation reservation);
    DateTime? GetEstimatedAvailability(Guid bookId);
}