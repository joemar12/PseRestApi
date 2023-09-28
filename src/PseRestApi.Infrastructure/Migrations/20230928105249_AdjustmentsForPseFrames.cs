using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PseRestApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdjustmentsForPseFrames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistoricalTradingData_SecurityInfo_SecurityId",
                table: "HistoricalTradingData");

            migrationBuilder.DropIndex(
                name: "IX_HistoricalTradingData_SecurityId",
                table: "HistoricalTradingData");

            migrationBuilder.AlterColumn<string>(
                name: "SecurityStatus",
                table: "SecurityInfo",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Symbol",
                table: "HistoricalTradingData",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_SecurityInfo_Symbol",
                table: "SecurityInfo",
                column: "Symbol");

            migrationBuilder.AddForeignKey(
                name: "FK_HistoricalTradingData_SecurityInfo_Symbol",
                table: "HistoricalTradingData",
                column: "Symbol",
                principalTable: "SecurityInfo",
                principalColumn: "Symbol",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistoricalTradingData_SecurityInfo_Symbol",
                table: "HistoricalTradingData");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_SecurityInfo_Symbol",
                table: "SecurityInfo");

            migrationBuilder.AlterColumn<string>(
                name: "SecurityStatus",
                table: "SecurityInfo",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Symbol",
                table: "HistoricalTradingData",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalTradingData_SecurityId",
                table: "HistoricalTradingData",
                column: "SecurityId");

            migrationBuilder.AddForeignKey(
                name: "FK_HistoricalTradingData_SecurityInfo_SecurityId",
                table: "HistoricalTradingData",
                column: "SecurityId",
                principalTable: "SecurityInfo",
                principalColumn: "SecurityId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
