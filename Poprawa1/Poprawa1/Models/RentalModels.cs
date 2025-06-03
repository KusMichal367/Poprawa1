namespace Poprawa1.Models;

public class Client
{
    public int Id { get; set; }
    public String FirstName { get; set; }
    public String LastName { get; set; }
    public String Adress { get; set; }
}

public class CarRental
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int CarId { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int TotalPrice { get; set; }
    public int Discount { get; set; }
}

public class Car
{
    public int Id { get; set; }
    public String VIN { get; set; }
    public String Name { get; set; }
    public int Seats { get; set; }
    public int PricePerDay { get; set; }
    public int ModelID { get; set; }
    public int ColorID { get; set; }
}

public class Color
{
    public int Id { get; set; }
    public String Name { get; set; }
}

public class Model
{
    public int Id { get; set; }
    public String Name { get; set; }
}

public class RentalDTO
{
    public String VIN { get; set; }
    public String Color { get; set; }
    public String Model { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int TotalPrice { get; set; }

}

public class ClientDTO
{
    public int Id { get; set; }
    public String FirstName { get; set; }
    public String LastName { get; set; }
    public String Adress { get; set; }
    public List<RentalDTO> Rentals { get; set; } = new List<RentalDTO>();
}

public class InputClientDTO
{
    public String FirstName { get; set; }
    public String LastName { get; set; }
    public String Address { get; set; }
}

public class InputDTO
{
    public InputClientDTO Client { get; set; }
    public int CarId { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}