using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Airport.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightCityRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationDate",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "FlightNumber",
                table: "Flight",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "ArrivalCityId",
                table: "Flight",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartureCityId",
                table: "Flight",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManufacturerId",
                table: "Aircraft",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Aircraft",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TotalSeats",
                table: "Aircraft",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Manufacturer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manufacturer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "City",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_City", x => x.Id);
                    table.ForeignKey(
                        name: "FK_City_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Country",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Россия" },
                    { 2, "США" },
                    { 3, "Германия" }
                });

            migrationBuilder.InsertData(
                table: "Manufacturer",
                columns: new[] { "Id", "Country", "Name" },
                values: new object[] { 1, "Unknown", "Default Manufacturer" });

            migrationBuilder.InsertData(
                table: "City",
                columns: new[] { "Id", "CountryId", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Москва" },
                    { 2, 1, "Санкт-Петербург" },
                    { 3, 2, "Нью-Йорк" },
                    { 4, 2, "Лос-Анджелес" },
                    { 5, 3, "Берлин" },
                    { 6, 3, "Мюнхен" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flight_ArrivalCityId",
                table: "Flight",
                column: "ArrivalCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Flight_DepartureCityId",
                table: "Flight",
                column: "DepartureCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Aircraft_ManufacturerId",
                table: "Aircraft",
                column: "ManufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_City_CountryId",
                table: "City",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Aircraft_Manufacturer_ManufacturerId",
                table: "Aircraft",
                column: "ManufacturerId",
                principalTable: "Manufacturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Flight_City_ArrivalCityId",
                table: "Flight",
                column: "ArrivalCityId",
                principalTable: "City",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Flight_City_DepartureCityId",
                table: "Flight",
                column: "DepartureCityId",
                principalTable: "City",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Aircraft_Manufacturer_ManufacturerId",
                table: "Aircraft");

            migrationBuilder.DropForeignKey(
                name: "FK_Flight_City_ArrivalCityId",
                table: "Flight");

            migrationBuilder.DropForeignKey(
                name: "FK_Flight_City_DepartureCityId",
                table: "Flight");

            migrationBuilder.DropTable(
                name: "City");

            migrationBuilder.DropTable(
                name: "Manufacturer");

            migrationBuilder.DropTable(
                name: "Country");

            migrationBuilder.DropIndex(
                name: "IX_Flight_ArrivalCityId",
                table: "Flight");

            migrationBuilder.DropIndex(
                name: "IX_Flight_DepartureCityId",
                table: "Flight");

            migrationBuilder.DropIndex(
                name: "IX_Aircraft_ManufacturerId",
                table: "Aircraft");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RegistrationDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ArrivalCityId",
                table: "Flight");

            migrationBuilder.DropColumn(
                name: "DepartureCityId",
                table: "Flight");

            migrationBuilder.DropColumn(
                name: "ManufacturerId",
                table: "Aircraft");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Aircraft");

            migrationBuilder.DropColumn(
                name: "TotalSeats",
                table: "Aircraft");

            migrationBuilder.AlterColumn<string>(
                name: "FlightNumber",
                table: "Flight",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
