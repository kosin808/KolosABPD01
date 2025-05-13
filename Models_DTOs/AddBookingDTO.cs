namespace WebApplication2.Models;

public class AddBookingDTO
{
    public int BookingId { get; set; }
    public int GuestId { get; set; }
    public int EmployeeId { get; set; }
    public List<BookingAttractionInput> Bookings { get; set; } =  new List<BookingAttractionInput>();
    
}

public class BookingAttractionInput
{
    public string AttractionName { get; set; }
    public int Amount { get; set; }
}