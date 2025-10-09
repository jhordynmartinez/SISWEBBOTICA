using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SISWEBBOTICA.Migrations
{
    /// <inheritdoc />
    public partial class AgregarConfiguracionStockNegativo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PermitirStockNegativo",
                table: "Boticas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PermitirStockNegativo",
                table: "Boticas");
        }
    }
}
