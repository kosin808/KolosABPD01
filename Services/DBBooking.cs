using System.Data.Common;
using WebApplication2.Models;
using Microsoft.Data.SqlClient;

namespace WebApplication2.Services;

public class DBBooking : IDBBooking
{
    private readonly string _connectionString;
    
    public DBBooking(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }

    public async Task<BookingInfoDTO> GetBookingInfo(int id)
    {
        string query = @"SELECT 
            SELECT b.date, g.first_name, g.last_name,g.date_of_birth, e.first_name,e.last_name,e.employee_number, a.name,a.price,ba.amount
            FROM Booking b 
                INNER JOIN Guest g ON g.guest_id = b.guest_id
                INNER JOIN Employee e ON e.employee_id = b.employee_id
                INNER JOIN Booking_Attraction ba ON b.appoitment_id = ba.appoitment_id
                INNER JOIN Attraction a ON a.attraction_id = ba.service_id
                WHERE a.attraction_id = @attractionId;";
        
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        command.CommandText = query;
        await connection.OpenAsync();
        
        command.Parameters.AddWithValue("@booking_Id", id);
        var reader = await command.ExecuteReaderAsync();
        
        BookingInfoDTO? bookingInfo = null;

        while (await reader.ReadAsync())
        {
            if (bookingInfo is null)
            {
                bookingInfo = new BookingInfoDTO
                {
                    Date = reader.GetDateTime(reader.GetOrdinal("date")),
                    Guest = new Guest()
                    {
                        FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                        LastName = reader.GetString(reader.GetOrdinal("last_name")),
                        DateOfBirth = reader.GetDateTime(reader.GetOrdinal("date_of_birth")),
                    },
                    Employee = new Employee()
                    {
                        FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                        LastName = reader.GetString(reader.GetOrdinal("last_name")),
                        EmployeeNumber = reader.GetString(reader.GetOrdinal("employee_number")),
                    },
                    Attractions = new List<Attractions>(),
                };
            }
            
            bookingInfo.Attractions.Add(
                new Attractions
                {
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Price = reader.GetDecimal(reader.GetOrdinal("price")),
                });
        }

        if (bookingInfo is null)
            throw new ArgumentException("Appointment not found. ");
        
        return bookingInfo;
    }
    
    public async Task AddBooking(AddBookingDTO booking)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
    
        
        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        try
        {
            
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Employee WHERE  = @employee_id;";
            command.Parameters.AddWithValue("@employee_id", booking.EmployeeId);
            var employeeId = await command.ExecuteScalarAsync();
            if (employeeId is null)
                throw new ArgumentException($"No employee with ID {employeeId} found.");
            
            
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Guest WHERE patient_id = @GuestId;";
            command.Parameters.AddWithValue("GuestId", booking.GuestId);
            var guestExists = await command.ExecuteScalarAsync();
            if (guestExists is null)
                throw new ArgumentException($"Guest with ID {booking.GuestId} not found.");
            
            
            
            command.Parameters.Clear();
            command.CommandText = @"
                    INSERT INTO Attraction
                    VALUES (@AttractionId, @GuestId, @DoctorId, @Date);";
            
            command.Parameters.AddWithValue("@AppointmentId", booking.BookingId);
            command.Parameters.AddWithValue("@GuestId", booking.GuestId);
            command.Parameters.AddWithValue("@EmployeeId", employeeId);
            command.Parameters.AddWithValue("@Date", DateTime.Now);
            
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("An booking with the same ID already exists. " + e.Message);
            }
            

            foreach (var attraction in booking.Bookings)
            {
                command.Parameters.Clear();
                command.CommandText = "SELECT attraction_id FROM Attraction WHERE name = @name;";
                command.Parameters.AddWithValue("@name", attraction.AttractionName);
                
                var attractionIdRes = await command.ExecuteScalarAsync();
                if (attractionIdRes is null)
                    throw new ArgumentException($"Attraction {attraction.AttractionName} was not found.");
                
                command.Parameters.Clear();
                command.CommandText = @"
                    INSERT INTO Bokking_Attraction 
                    VALUES (@BookingId, @AttractionId, @Amount);";
                command.Parameters.AddWithValue("@BookingId", booking.BookingId);
                command.Parameters.AddWithValue("@AttractionId", attractionIdRes);
                command.Parameters.AddWithValue("@Amount", attraction.Amount);
                
                await command.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}