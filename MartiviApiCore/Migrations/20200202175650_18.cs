using Microsoft.EntityFrameworkCore.Migrations;

namespace MartiviApiCore.Migrations
{
    public partial class _18 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanceledOrderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "CanceledOrderId",
                table: "OrderedProducts",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CanceledOrders",
                columns: table => new
                {
                    CanceledOrderId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    Payment = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: true),
                    OrderTimeTicks = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanceledOrders", x => x.CanceledOrderId);
                    table.ForeignKey(
                        name: "FK_CanceledOrders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderedProducts_CanceledOrderId",
                table: "OrderedProducts",
                column: "CanceledOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CanceledOrders_UserId",
                table: "CanceledOrders",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderedProducts_CanceledOrders_CanceledOrderId",
                table: "OrderedProducts",
                column: "CanceledOrderId",
                principalTable: "CanceledOrders",
                principalColumn: "CanceledOrderId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderedProducts_CanceledOrders_CanceledOrderId",
                table: "OrderedProducts");

            migrationBuilder.DropTable(
                name: "CanceledOrders");

            migrationBuilder.DropIndex(
                name: "IX_OrderedProducts_CanceledOrderId",
                table: "OrderedProducts");

            migrationBuilder.DropColumn(
                name: "CanceledOrderId",
                table: "OrderedProducts");

            migrationBuilder.AddColumn<int>(
                name: "CanceledOrderId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
