using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CineVerse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Titles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Kind = table.Column<string>(type: "TEXT", nullable: false),
                    Genre = table.Column<string>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<double>(type: "REAL", nullable: false),
                    Synopsis = table.Column<string>(type: "TEXT", nullable: false),
                    Image = table.Column<string>(type: "TEXT", nullable: false),
                    Popularity = table.Column<int>(type: "INTEGER", nullable: false),
                    Episode = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Titles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Titles",
                columns: new[] { "Id", "Episode", "Genre", "Image", "Kind", "Name", "Popularity", "Score", "Synopsis", "Year" },
                values: new object[,]
                {
                    { 1, null, "Ficção científica", "horizonte.jpg", "Film", "Horizonte Perdido", 100, 8.9000000000000004, "Uma astronauta deteta um sinal impossível junto de uma estrela em colapso e tem de decidir se regressa à Terra ou segue a transmissão.", 2025 },
                    { 2, "T01 · EP04", "Suspense", "cidade.jpg", "Series", "Cidade em Silêncio", 95, 8.5999999999999996, "Um detetive investiga uma série de desaparecimentos nas ruas de Lisboa.", 2025 },
                    { 3, null, "Aventura", "nuvens.jpg", "Film", "Acima das Nuvens", 90, 8.0999999999999996, "Uma expedição fica isolada na montanha quando a tempestade apaga o caminho de regresso.", 2024 },
                    { 4, "T02 · EP03", "Drama", "meia-noite.jpg", "Series", "Depois da Meia-Noite", 85, 8.4000000000000004, "Duas desconhecidas encontram-se no último metro e reconstroem uma noite que mudou as suas vidas.", 2025 },
                    { 5, "T01 · EP08", "Fantasia", "verao.jpg", "Anime", "O Verão das Estrelas", 80, 8.6999999999999993, "Durante as férias, um rapaz segue uma estrela que caiu nos campos da sua aldeia.", 2025 },
                    { 6, null, "Ficção científica", "orbita.jpg", "Film", "Última Órbita", 75, 7.9000000000000004, "Um piloto aceita uma última missão para resgatar uma tripulação em órbita.", 2024 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Titles_Popularity",
                table: "Titles",
                column: "Popularity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Titles");
        }
    }
}
