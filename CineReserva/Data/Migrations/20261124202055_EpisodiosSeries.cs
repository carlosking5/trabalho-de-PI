using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineReserva.Data.Migrations
{
    /// <inheritdoc />
    public partial class EpisodiosSeries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EpisodioFinal",
                table: "Filmes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EpisodioInicial",
                table: "Filmes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Temporada",
                table: "Filmes",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EpisodioFinal",
                table: "Filmes");

            migrationBuilder.DropColumn(
                name: "EpisodioInicial",
                table: "Filmes");

            migrationBuilder.DropColumn(
                name: "Temporada",
                table: "Filmes");
        }
    }
}
