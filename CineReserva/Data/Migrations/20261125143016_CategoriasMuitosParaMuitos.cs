using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineReserva.Data.Migrations
{
    /// <summary>
    /// Filmes/séries passam a poder ter várias categorias (relação N para N).
    /// A ordem é importante para não perder dados:
    ///   1. cria a tabela de ligação FilmeCategorias;
    ///   2. copia para lá a categoria que cada filme já tinha (coluna Filmes.CategoriaId);
    ///   3. só depois apaga a coluna antiga.
    /// </summary>
    public partial class CategoriasMuitosParaMuitos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Tabela de ligação
            migrationBuilder.CreateTable(
                name: "FilmeCategorias",
                columns: table => new
                {
                    FilmeId = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilmeCategorias", x => new { x.FilmeId, x.CategoriaId });
                    table.ForeignKey(
                        name: "FK_FilmeCategorias_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FilmeCategorias_Filmes_FilmeId",
                        column: x => x.FilmeId,
                        principalTable: "Filmes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FilmeCategorias_CategoriaId",
                table: "FilmeCategorias",
                column: "CategoriaId");

            // 2. Copia a categoria atual de cada filme para a tabela nova
            migrationBuilder.Sql(
                "INSERT INTO \"FilmeCategorias\" (\"FilmeId\", \"CategoriaId\") " +
                "SELECT \"Id\", \"CategoriaId\" FROM \"Filmes\" " +
                "WHERE \"CategoriaId\" IN (SELECT \"Id\" FROM \"Categorias\");");

            // 3. Agora já se pode apagar a coluna antiga
            migrationBuilder.DropForeignKey(
                name: "FK_Filmes_Categorias_CategoriaId",
                table: "Filmes");

            migrationBuilder.DropIndex(
                name: "IX_Filmes_CategoriaId",
                table: "Filmes");

            migrationBuilder.DropColumn(
                name: "CategoriaId",
                table: "Filmes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Volta a ter só uma categoria por filme: fica a de Id mais baixo de cada filme.
            migrationBuilder.AddColumn<int>(
                name: "CategoriaId",
                table: "Filmes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            // Filmes sem nenhuma categoria ficam com a primeira categoria que existir
            migrationBuilder.Sql(
                "UPDATE \"Filmes\" SET \"CategoriaId\" = COALESCE(" +
                "(SELECT MIN(\"CategoriaId\") FROM \"FilmeCategorias\" WHERE \"FilmeId\" = \"Filmes\".\"Id\"), " +
                "(SELECT MIN(\"Id\") FROM \"Categorias\"), 0);");

            migrationBuilder.CreateIndex(
                name: "IX_Filmes_CategoriaId",
                table: "Filmes",
                column: "CategoriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Filmes_Categorias_CategoriaId",
                table: "Filmes",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropTable(
                name: "FilmeCategorias");
        }
    }
}
