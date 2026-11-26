using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarjumusteRPG.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "RewardDate",
                table: "Users",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RewardsToday",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "RewardDate", "RewardsToday" },
                values: new object[] { null, 0 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "RewardDate", "RewardsToday" },
                values: new object[] { null, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RewardDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RewardsToday",
                table: "Users");
        }
    }
}
