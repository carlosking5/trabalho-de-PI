using CineVerse.Domain;
using Microsoft.EntityFrameworkCore;

namespace CineVerse.Infrastructure;

internal static class BookingModel
{
    public static void Configure(ModelBuilder model)
    {
        var cinemas = model.Entity<Cinema>().ToTable("Cinemas");
        cinemas.HasData(new Cinema(1, "CineVerse Demo", "Lisboa"));

        var rooms = model.Entity<Room>().ToTable("Rooms");
        rooms.HasOne<Cinema>().WithMany().HasForeignKey(room => room.CinemaId).OnDelete(DeleteBehavior.Restrict);
        rooms.HasIndex(room => new { room.CinemaId, room.Name }).IsUnique();
        rooms.HasData(new Room(1, 1, "Sala 1"));

        var seats = model.Entity<Seat>().ToTable("Seats",
            table => table.HasCheckConstraint("CK_Seat_Number", "Number > 0"));
        seats.HasOne<Room>().WithMany().HasForeignKey(seat => seat.RoomId).OnDelete(DeleteBehavior.Restrict);
        seats.HasAlternateKey(seat => new { seat.Id, seat.RoomId });
        seats.HasIndex(seat => new { seat.RoomId, seat.Row, seat.Number }).IsUnique();
        seats.HasData(from row in Enumerable.Range(0, 5)
                      from number in Enumerable.Range(1, 8)
                      select new Seat(row * 8 + number, 1, ((char)('A' + row)).ToString(), number, true));

        var screenings = model.Entity<Screening>().ToTable("Screenings",
            table => table.HasCheckConstraint("CK_Screening_Price", "PriceCents >= 0"));
        screenings.HasOne<Title>().WithMany().HasForeignKey(item => item.TitleId).OnDelete(DeleteBehavior.Restrict);
        screenings.HasOne<Room>().WithMany().HasForeignKey(item => item.RoomId).OnDelete(DeleteBehavior.Restrict);
        screenings.HasAlternateKey(item => new { item.Id, item.RoomId });
        screenings.HasIndex(item => new { item.RoomId, item.StartsAtUtc }).IsUnique();

        var reservations = model.Entity<Reservation>().ToTable("Reservations", table =>
        {
            table.HasCheckConstraint("CK_Reservation_Total", "TotalCents >= 0");
            table.HasCheckConstraint("CK_Reservation_Status", "Status IN ('Pending', 'Confirmed', 'Cancelled', 'Expired')");
        });
        reservations.Property(item => item.Status).HasConversion<string>();
        reservations.HasAlternateKey(item => new { item.Id, item.ScreeningId });
        reservations.HasOne<ApplicationUser>().WithMany().HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Restrict);
        reservations.HasOne<Screening>().WithMany().HasForeignKey(item => item.ScreeningId).OnDelete(DeleteBehavior.Restrict);

        var reservedSeats = model.Entity<ReservationSeat>().ToTable("ReservationSeats",
            table => table.HasCheckConstraint("CK_ReservationSeat_Price", "UnitPriceCents >= 0"));
        reservedSeats.HasKey(item => new { item.ReservationId, item.SeatId });
        reservedSeats.HasAlternateKey(item => new { item.ReservationId, item.ScreeningId, item.SeatId, item.RoomId });
        reservedSeats.HasOne<Reservation>().WithMany().HasForeignKey(item => new { item.ReservationId, item.ScreeningId })
            .HasPrincipalKey(item => new { item.Id, item.ScreeningId });
        reservedSeats.HasOne<Screening>().WithMany().HasForeignKey(item => new { item.ScreeningId, item.RoomId })
            .HasPrincipalKey(item => new { item.Id, item.RoomId }).OnDelete(DeleteBehavior.Restrict);
        reservedSeats.HasOne<Seat>().WithMany().HasForeignKey(item => new { item.SeatId, item.RoomId })
            .HasPrincipalKey(item => new { item.Id, item.RoomId }).OnDelete(DeleteBehavior.Restrict);

        var allocations = model.Entity<ScreeningSeatAllocation>().ToTable("ScreeningSeatAllocations");
        allocations.HasKey(item => new { item.ScreeningId, item.SeatId });
        allocations.HasIndex(item => item.ExpiresAtUtc);
        allocations.HasOne<ReservationSeat>().WithMany()
            .HasForeignKey(item => new { item.ReservationId, item.ScreeningId, item.SeatId, item.RoomId })
            .HasPrincipalKey(item => new { item.ReservationId, item.ScreeningId, item.SeatId, item.RoomId });

        var tickets = model.Entity<Ticket>().ToTable("Tickets");
        tickets.HasIndex(item => item.Code).IsUnique();
        tickets.HasIndex(item => new { item.ReservationId, item.SeatId }).IsUnique();
        tickets.HasOne<ReservationSeat>().WithMany().HasForeignKey(item => new { item.ReservationId, item.SeatId });
    }
}
