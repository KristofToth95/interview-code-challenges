using Microsoft.EntityFrameworkCore;
using OneBeyondApi.Dto;
using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess;

public class LoanRepository : ILoanRepository
{
    public List<LoanInfo> GetActiveLoans()
    {
        using var context = new LibraryContext();
        var now = DateTime.UtcNow;
        var list = context.Catalogue
            .Include(c => c.Book)
            .Include(c => c.OnLoanTo)
            .Where(c => c.LoanEndDate >= now && c.OnLoanTo != null)
            .GroupBy(c => c.OnLoanTo)
            .Select(g => new LoanInfo
            {
                Borrower = g.Key!,
                Books = g.Select(s => new BookInfo()
                {
                    BookId = s.Book.Id,
                    Title = s.Book.Name,
                }).ToArray(),
            })
            .ToList();
        return list;
    }
    
    public BookStock? GetBookStockById(Guid id)
    {
        using var context = new LibraryContext();

        return context.Catalogue
            .Include(c => c.Book)
            .Include(c => c.OnLoanTo)
            .FirstOrDefault(c => c.Id == id);
    }
    
    public void ReturnBook(BookStock bookStock, out Fine? fine)
    {
        using var context = new LibraryContext();

        fine = null;

        if (bookStock.OnLoanTo == null)
            return;

        if (bookStock.LoanEndDate is DateTime endDate && DateTime.UtcNow > endDate)
        {
            fine = new Fine
            {
                Id = Guid.NewGuid(),
                BorrowerId = bookStock.OnLoanTo.Id,
                Amount = CalculateFine(endDate),
                Reason = "Returned late",
                IssuedAt = DateTime.UtcNow
            };
            context.Fines.Add(fine);
        }

        bookStock.OnLoanTo = null;
        bookStock.LoanEndDate = null;
        context.SaveChanges();
    }

    private decimal CalculateFine(DateTime dueDate)
    {
        var daysLate = (DateTime.UtcNow.Date - dueDate.Date).Days;
        return daysLate * 1000;
    }
    
    public bool AreAllCopiesOnLoan(Guid bookId)
    {
        using var context = new LibraryContext();

        return context.Catalogue
            .Where(bs => bs.Book.Id == bookId)
            .All(bs => bs.OnLoanTo != null);
    }

    public void AddReservation(Guid borrowerId, Guid bookId)
    {
        using var context = new LibraryContext();

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            BookId = bookId,
            BorrowerId = borrowerId,
            ReservedAt = DateTime.UtcNow,
            IsFulfilled = false
        };

        context.Reservations.Add(reservation);
        context.SaveChanges();
    }

    public Reservation? GetBorrowerReservation(Guid borrowerId, Guid bookId)
    {
        using var context = new LibraryContext();

        return context.Reservations
            .Where(r => r.BookId == bookId && r.BorrowerId == borrowerId && !r.IsFulfilled)
            .OrderBy(r => r.ReservedAt)
            .FirstOrDefault();
    }

    public int GetReservationQueuePosition(Reservation reservation)
    {
        using var context = new LibraryContext();

        return context.Reservations
            .Count(r => r.BookId == reservation.BookId && !r.IsFulfilled && r.ReservedAt < reservation.ReservedAt);
    }

    public DateTime? GetEstimatedAvailability(Guid bookId)
    {
        using var context = new LibraryContext();

        return context.Catalogue
            .Where(bs => bs.Book.Id == bookId && bs.OnLoanTo != null)
            .OrderBy(bs => bs.LoanEndDate)
            .Select(bs => bs.LoanEndDate)
            .FirstOrDefault();
    }
}