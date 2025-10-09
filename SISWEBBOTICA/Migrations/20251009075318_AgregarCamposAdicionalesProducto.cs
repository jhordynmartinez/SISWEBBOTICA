using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SISWEBBOTICA.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposAdicionalesProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Lote",
                table: "Productos",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Presentacion",
                table: "Productos",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrincipioActivo",
                table: "Productos",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistroSanitario",
                table: "Productos",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion",
                table: "Productos",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lote",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Presentacion",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PrincipioActivo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "RegistroSanitario",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Ubicacion",
                table: "Productos");
        }
    }
}
