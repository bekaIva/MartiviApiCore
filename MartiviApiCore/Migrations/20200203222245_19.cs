using Microsoft.EntityFrameworkCore.Migrations;

namespace MartiviApiCore.Migrations
{
    public partial class _19 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompletedOrderId",
                table: "OrderedProducts",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CompletedOrders",
                columns: table => new
                {
                    CompletedOrderId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    Payment = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: true),
                    OrderTimeTicks = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedOrders", x => x.CompletedOrderId);
                    table.ForeignKey(
                        name: "FK_CompletedOrders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderedProducts_CompletedOrderId",
                table: "OrderedProducts",
                column: "CompletedOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedOrders_UserId",
                table: "CompletedOrders",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderedProducts_CompletedOrders_CompletedOrderId",
                table: "OrderedProducts",
                column: "CompletedOrderId",
                principalTable: "CompletedOrders",
                principalColumn: "CompletedOrderId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderedProducts_CompletedOrders_CompletedOrderId",
                table: "OrderedProducts");

            migrationBuilder.DropTable(
                name: "CompletedOrders");

            migrationBuilder.DropIndex(
                name: "IX_OrderedProducts_CompletedOrderId",
                table: "OrderedProducts");

            migrationBuilder.DropColumn(
                name: "CompletedOrderId",
                table: "OrderedProducts");
        }
    }
}
