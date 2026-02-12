using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiBaseCore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueTaxIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TaxId",
                table: "Usuarios",
                column: "TaxId",
                unique: true,
                filter: "[TaxId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_TaxId",
                table: "Usuarios");
        }
    }
}
