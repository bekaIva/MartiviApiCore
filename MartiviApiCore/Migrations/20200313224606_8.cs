using Microsoft.EntityFrameworkCore.Migrations;

namespace MartiviApiCore.Migrations
{
    public partial class _8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderAddressId",
                table: "CompletedOrders",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderAddressId",
                table: "CanceledOrders",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompletedOrders_OrderAddressId",
                table: "CompletedOrders",
                column: "OrderAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_CanceledOrders_OrderAddressId",
                table: "CanceledOrders",
                column: "OrderAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_CanceledOrders_OrderAddress_OrderAddressId",
                table: "CanceledOrders",
                column: "OrderAddressId",
                principalTable: "OrderAddress",
                principalColumn: "OrderAddressId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompletedOrders_OrderAddress_OrderAddressId",
                table: "CompletedOrders",
                column: "OrderAddressId",
                principalTable: "OrderAddress",
                principalColumn: "OrderAddressId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CanceledOrders_OrderAddress_OrderAddressId",
                table: "CanceledOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_CompletedOrders_OrderAddress_OrderAddressId",
                table: "CompletedOrders");

            migrationBuilder.DropIndex(
                name: "IX_CompletedOrders_OrderAddressId",
                table: "CompletedOrders");

            migrationBuilder.DropIndex(
                name: "IX_CanceledOrders_OrderAddressId",
                table: "CanceledOrders");

            migrationBuilder.DropColumn(
                name: "OrderAddressId",
                table: "CompletedOrders");

            migrationBuilder.DropColumn(
                name: "OrderAddressId",
                table: "CanceledOrders");
        }
    }
}
