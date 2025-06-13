using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OneBeyondApi.DataAccess;
using OneBeyondApi.Dto;

namespace OneBeyondApi.Controllers;

[ApiController]
[Route("[controller]")]
public class LoanController : ControllerBase
{
    private readonly ILogger<LoanController> _logger;
    private readonly ILoanRepository _repository;

    public LoanController(ILogger<LoanController> logger, ILoanRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(List<LoanInfo>), StatusCodes.Status200OK)]
    public ActionResult<List<LoanInfo>> GetActiveLoans()
    {
        return Ok(_repository.GetActiveLoans());
    }

    [HttpPost("return/{bookStockId}")]
    [ProducesResponseType(typeof(BookReturnInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<BookReturnInfo> ReturnBook(Guid bookStockId)
    {
        var bookStock = _repository.GetBookStockById(bookStockId);

        if (bookStock == null)
            return NotFound("Book not found.");

        if (bookStock.OnLoanTo == null)
            return BadRequest("Book is not currently on loan.");

        _repository.ReturnBook(bookStock, out var fine);

        if (fine != null)
        {
            return Ok(new BookReturnInfo()
            {
                Message = "Book returned with a fine.",
                Fine = new ()
                {
                    Amount = fine.Amount,
                    Reason = fine.Reason,
                    IssuedAt = fine.IssuedAt
                }
            });
        }

        return Ok("Book returned successfully.");
    }

    [HttpPost("Reserve")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult ReserveBook(Guid borrowerId, Guid bookId)
    {
        var isAllOnLoan = _repository.AreAllCopiesOnLoan(bookId);
        if (!isAllOnLoan)
            return BadRequest("Book is currently available — no need to reserve.");

        _repository.AddReservation(borrowerId, bookId);

        return Ok("Reservation placed.");
    }

    [HttpGet("ReservationStatus")]
    [ProducesResponseType(typeof(ReservationStatusInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ReservationStatusInfo> GetReservationStatus(Guid borrowerId, Guid bookId)
    {
        var reservation = _repository.GetBorrowerReservation(borrowerId, bookId);

        if (reservation == null)
            return NotFound("No active reservation found for this borrower and book.");

        var queuePosition = _repository.GetReservationQueuePosition(reservation);
        var estimatedAvailable = _repository.GetEstimatedAvailability(bookId);

        return Ok(new
        {
            PositionInQueue = queuePosition + 1,
            EstimatedAvailableDate = estimatedAvailable
        });
    }
}