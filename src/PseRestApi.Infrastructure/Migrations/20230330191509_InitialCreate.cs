using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PseRestApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SecurityInfo",
                columns: table => new
                {
                    SecurityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SecurityStatus = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    SecurityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityInfo", x => x.SecurityId);
                });

            migrationBuilder.CreateTable(
                name: "HistoricalTradingData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SecurityId = table.Column<int>(type: "int", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SqPrevious = table.Column<double>(type: "float", nullable: true),
                    SqOpen = table.Column<double>(type: "float", nullable: true),
                    SqHigh = table.Column<double>(type: "float", nullable: true),
                    SqLow = table.Column<double>(type: "float", nullable: true),
                    FiftyTwoWeekHigh = table.Column<double>(type: "float", nullable: true),
                    FiftyTwoWeekLow = table.Column<double>(type: "float", nullable: true),
                    ChangeClose = table.Column<double>(type: "float", nullable: true),
                    PercChangeClose = table.Column<double>(type: "float", nullable: true),
                    ChangeClosePercChangeClose = table.Column<double>(type: "float", nullable: true),
                    AvgPrice = table.Column<double>(type: "float", nullable: true),
                    LastTradePrice = table.Column<double>(type: "float", nullable: true),
                    LastTradedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentPe = table.Column<double>(type: "float", nullable: true),
                    TotalValue = table.Column<double>(type: "float", nullable: true),
                    TotalVolume = table.Column<double>(type: "float", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricalTradingData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricalTradingData_SecurityInfo_SecurityId",
                        column: x => x.SecurityId,
                        principalTable: "SecurityInfo",
                        principalColumn: "SecurityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalTradingData_SecurityId",
                table: "HistoricalTradingData",
                column: "SecurityId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalTradingData_Symbol",
                table: "HistoricalTradingData",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityInfo_Symbol",
                table: "SecurityInfo",
                column: "Symbol",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricalTradingData");

            migrationBuilder.DropTable(
                name: "SecurityInfo");
        }
    }
}
