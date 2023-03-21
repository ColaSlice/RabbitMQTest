namespace CarDealerShip.API.Models;

public class Booking
{
    public int Id { get; set; }
    public string CarName { get; set; } = string.Empty;
    public string CreditCard { get; set; } = string.Empty;
    public string DealershipLocation { get; set; } = string.Empty;
    public string OwnerLocation { get; set; } = string.Empty;
    public int Status { get; set; }
}