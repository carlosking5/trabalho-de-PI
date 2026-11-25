using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineReserva.Data.Migrations
{
    /// <summary>
    /// Acrescenta o pagamento às reservas: método (Cartão / MB WAY) e estado (Pendente / Pago / Cancelado).
    /// Os dados do cartão não são guardados em lado nenhum.
    /// As reservas que já existiam foram confirmadas no sistema antigo (sem pagamento), por isso
    /// ficam como Pago — e as canceladas como Cancelado — para os lugares e o total faturado não mudarem.
    /// </summary>
    public partial class PagamentoReservas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstadoPagamento",
                table: "Reservas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MetodoPagamento",
                table: "Reservas",
                type: "INTEGER",
                nullable: true);

            // Reservas antigas: Estado 2 = Cancelada -> pagamento 2 = Cancelado; as outras -> 1 = Pago.
            // O método fica NULL (não se sabe como foram pagas).
            migrationBuilder.Sql(
                "UPDATE \"Reservas\" SET \"EstadoPagamento\" = CASE WHEN \"Estado\" = 2 THEN 2 ELSE 1 END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstadoPagamento",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "MetodoPagamento",
                table: "Reservas");
        }
    }
}
