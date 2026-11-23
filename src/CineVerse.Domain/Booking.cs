namespace CineVerse.Domain;

public sealed record Cinema(int Id, string Name, string City);
public sealed record Room(int Id, int CinemaId, string Name);
public sealed record Seat(int Id, int RoomId, string Row, int Number, bool Available);
public sealed record Screening(int Id, int TitleId, int RoomId, DateTime StartsAtUtc, int PriceCents);

public enum ReservationStatus { Pending, Confirmed, Cancelled, Expired }

public sealed record Reservation(Guid Id, string UserId, int ScreeningId, ReservationStatus Status,
    DateTime CreatedAtUtc, int TotalCents);
public sealed record ReservationSeat(Guid ReservationId, int ScreeningId, int SeatId, int RoomId,
    int UnitPriceCents);
public sealed record ScreeningSeatAllocation(int ScreeningId, int SeatId, int RoomId,
    Guid ReservationId, DateTime? ExpiresAtUtc);
public sealed record Ticket(Guid Id, Guid ReservationId, int SeatId, string Code,
    DateTime IssuedAtUtc, DateTime? ValidatedAtUtc = null);
