using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Website_QuanLyKhoHangThucPham.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToStoreOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "StoreOrders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoreOrders_UserId",
                table: "StoreOrders",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreOrders_AspNetUsers_UserId",
                table: "StoreOrders",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreOrders_AspNetUsers_UserId",
                table: "StoreOrders");

            migrationBuilder.DropIndex(
                name: "IX_StoreOrders_UserId",
                table: "StoreOrders");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "StoreOrders");
        }
    }
}
