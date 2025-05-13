using WebApplication2.Models;


namespace WebApplication2.Services;

public interface IDBBooking
{
    Task<BookingInfoDTO> GetBookingInfo(int id);
    Task AddBooking(AddBookingDTO booking);
}