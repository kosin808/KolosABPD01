namespace WebApplication2.Models;

public class BookingInfoDTO
{
    public DateTime Date { get; set; }
    public Guest Guest { get; set; }
    public Employee Employee { get; set; }
    public List<Attractions> Attractions { get; set; }
}

public class Guest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public class Employee
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmployeeNumber { get; set; }
}

public class Attractions
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}