using System.IO;

namespace Stayin.Core;

public class AppartementCreatedEvent : BaseEvent
{
    public override async Task Handle(IDataAccess dataAccess)
    {
        await dataAccess.CreateHousePublicationAsync(new HousePublication() { CreatorId = owner, Id = _id, Status = HouseStatus.Approved, Description = description, Title = title });

        await dataAccess.SaveChangesAsync();
    }


    public required string _id { get; set; }
    public required string owner { get; set; }
    public string? title { get; set; }
    public string? wilaya { get; set; }
    public string? comun { get; set; }
    public string? street { get; set; }
    public string?[]? photos { get; set; }
    public string? description { get; set; }
    public string?[]? perks { get; set; }
    public string?[]? apartementType { get; set; }
    public string? extraInfo { get; set; }
    public string? checkIn { get; set; }
    public string? checkOut { get; set; }
    public int maxGuests { get; set; }
    public int price { get; set; }
    public string?[]? reservedDates { get; set; }
}
