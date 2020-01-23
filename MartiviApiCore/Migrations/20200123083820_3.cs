using Microsoft.EntityFrameworkCore.Migrations;

namespace MartiviApiCore.Migrations
{
    public partial class _3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderedProduct_Orders_OrderId",
                table: "OrderedProduct");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderedProduct",
                table: "OrderedProduct");

            migrationBuilder.RenameTable(
                name: "OrderedProduct",
                newName: "OrderedProducts");

            migrationBuilder.RenameIndex(
                name: "IX_OrderedProduct_OrderId",
                table: "OrderedProducts",
                newName: "IX_OrderedProducts_OrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderedProducts",
                table: "OrderedProducts",
                column: "OrderedProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderedProducts_Orders_OrderId",
                table: "OrderedProducts",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderedProducts_Orders_OrderId",
                table: "OrderedProducts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderedProducts",
                table: "OrderedProducts");

            migrationBuilder.RenameTable(
                name: "OrderedProducts",
                newName: "OrderedProduct");

            migrationBuilder.RenameIndex(
                name: "IX_OrderedProducts_OrderId",
                table: "OrderedProduct",
                newName: "IX_OrderedProduct_OrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderedProduct",
                table: "OrderedProduct",
                column: "OrderedProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderedProduct_Orders_OrderId",
                table: "OrderedProduct",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
