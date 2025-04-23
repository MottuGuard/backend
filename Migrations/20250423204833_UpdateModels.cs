using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PositionRecords",
                columns: table => new
                {
                    PositionRecordId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    MotoId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    X = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    Y = table.Column<double>(type: "BINARY_DOUBLE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionRecords", x => x.PositionRecordId);
                    table.ForeignKey(
                        name: "FK_PositionRecords_Motos_MotoId",
                        column: x => x.MotoId,
                        principalTable: "Motos",
                        principalColumn: "MotoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UwbAnchors",
                columns: table => new
                {
                    UwbAnchorId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    X = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    Y = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    Z = table.Column<double>(type: "BINARY_DOUBLE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UwbAnchors", x => x.UwbAnchorId);
                });

            migrationBuilder.CreateTable(
                name: "UwbTags",
                columns: table => new
                {
                    UwbTagId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Eui64 = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MotoId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UwbTags", x => x.UwbTagId);
                    table.ForeignKey(
                        name: "FK_UwbTags_Motos_MotoId",
                        column: x => x.MotoId,
                        principalTable: "Motos",
                        principalColumn: "MotoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UwbMeasurements",
                columns: table => new
                {
                    UwbMeasurementId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    UwbTagId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    UwbAnchorId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Distance = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    Rssi = table.Column<double>(type: "BINARY_DOUBLE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UwbMeasurements", x => x.UwbMeasurementId);
                    table.ForeignKey(
                        name: "FK_UwbMeasurements_UwbAnchors_UwbAnchorId",
                        column: x => x.UwbAnchorId,
                        principalTable: "UwbAnchors",
                        principalColumn: "UwbAnchorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UwbMeasurements_UwbTags_UwbTagId",
                        column: x => x.UwbTagId,
                        principalTable: "UwbTags",
                        principalColumn: "UwbTagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PositionRecords_MotoId",
                table: "PositionRecords",
                column: "MotoId");

            migrationBuilder.CreateIndex(
                name: "IX_UwbMeasurements_UwbAnchorId",
                table: "UwbMeasurements",
                column: "UwbAnchorId");

            migrationBuilder.CreateIndex(
                name: "IX_UwbMeasurements_UwbTagId",
                table: "UwbMeasurements",
                column: "UwbTagId");

            migrationBuilder.CreateIndex(
                name: "IX_UwbTags_MotoId",
                table: "UwbTags",
                column: "MotoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PositionRecords");

            migrationBuilder.DropTable(
                name: "UwbMeasurements");

            migrationBuilder.DropTable(
                name: "UwbAnchors");

            migrationBuilder.DropTable(
                name: "UwbTags");
        }
    }
}
