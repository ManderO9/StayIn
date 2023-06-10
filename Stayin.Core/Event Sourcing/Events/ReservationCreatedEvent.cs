namespace Stayin.Core;

public class ReservationCreatedEvent : BaseEvent
{
    public override async Task Handle(IDataAccess dataAccess)
    {

        var housePublication = await dataAccess.GetHousePublicationById(appartement!);

        await dataAccess.AddRentalAsync(new Rental()
        {
            Id = _id!,
            HousePublicationId = appartement!,
            RenterId = user!,
            StartedDate = checkIn,
            EndedDate = checkOut,
            PublicationTitle = housePublication?.Title
        });
        await dataAccess.SaveChangesAsync();
    }

    public string? _id { get; set; }
    public string? appartement { get; set; }
    public string? user { get; set; }
    public string? checkIn { get; set; }
    public string? checkOut { get; set; }
    public string? numberOfGuests { get; set; }
    public string? name { get; set; }
    public string? phone { get; set; }
    public string? email { get; set; }
    public string? price { get; set; }
    public string? reservedDates { get; set; }
}
