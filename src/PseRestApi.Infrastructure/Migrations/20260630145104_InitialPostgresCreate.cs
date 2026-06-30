using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PseRestApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgresCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SecurityInfo",
                columns: table => new
                {
                    SecurityId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SecurityStatus = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    SecurityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityInfo", x => x.SecurityId);
                    table.UniqueConstraint("AK_SecurityInfo_Symbol", x => x.Symbol);
                });

            migrationBuilder.CreateTable(
                name: "SyncBatchData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncBatchData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricalTradingData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SecurityId = table.Column<int>(type: "integer", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: true),
                    Change = table.Column<double>(type: "double precision", nullable: true),
                    PercentChange = table.Column<double>(type: "double precision", nullable: true),
                    Price = table.Column<double>(type: "double precision", nullable: true),
                    TradeDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Value = table.Column<double>(type: "double precision", nullable: true),
                    Volume = table.Column<double>(type: "double precision", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricalTradingData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricalTradingData_SecurityInfo_Symbol",
                        column: x => x.Symbol,
                        principalTable: "SecurityInfo",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "SyncBatchData");

            migrationBuilder.DropTable(
                name: "SecurityInfo");
        }
    }
}
