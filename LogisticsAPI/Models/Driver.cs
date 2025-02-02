namespace LogisticsAPI.Models;

public class DriverInput
{
    public DriverInput(string? firstName, string? lastName, string? homeAddress)
    {
        FirstName = firstName;
        LastName = lastName;
        HomeAddress = homeAddress;
    }

    public DriverInput()
    {
    }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? HomeAddress { get; set; }

    public DriverOutput Identified(int id)
    {
        return new DriverOutput(id, this);
    }
}

public class DriverOutput : DriverInput
{

    public DriverOutput(string? firstName, string? lastName, string? homeAddress, int driverId)
        : base(firstName, lastName, homeAddress)
    {
        DriverId = driverId;
    }

    public DriverOutput(int driverId, DriverInput orig)
        : base(orig.FirstName, orig.LastName, orig.HomeAddress)
    {
        DriverId = driverId;
    }

    public DriverOutput()
    {
    }


    public int DriverId { get; set; }
}