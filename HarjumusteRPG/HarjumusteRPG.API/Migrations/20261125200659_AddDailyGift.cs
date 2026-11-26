using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarjumusteRPG.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyGift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "LastGiftDate",
                table: "Users",
                type: "date",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastGiftDate",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastGiftDate",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastGiftDate",
                table: "Users");
        }
    }
}
