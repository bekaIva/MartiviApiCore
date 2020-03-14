using Microsoft.EntityFrameworkCore.Migrations;

namespace MartiviApiCore.Migrations
{
    public partial class _4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderAddressId",
                table: "Orders",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrderAddress",
                columns: table => new
                {
                    OrderAddressId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsPrimary = table.Column<bool>(nullable: false),
                    CustomerName = table.Column<string>(nullable: true),
                    AddressType = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    MobileNumber = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAddress", x => x.OrderAddressId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderAddressId",
                table: "Orders",
                column: "OrderAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderAddress_OrderAddressId",
                table: "Orders",
                column: "OrderAddressId",
                principalTable: "OrderAddress",
                principalColumn: "OrderAddressId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderAddress_OrderAddressId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "OrderAddress");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderAddressId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderAddressId",
                table: "Orders");
        }
    }
}
