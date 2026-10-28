using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSCMS.Migrations
{
    /// <inheritdoc />
    public partial class AddStaticUploads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StaticUploads",
                columns: table => new
                {
                    StaticUploadId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FolderPath = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    IsArchive = table.Column<bool>(type: "INTEGER", nullable: false),
                    EntryPoint = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TotalSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    FileCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastReplacedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UploadedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaticUploads", x => x.StaticUploadId);
                    table.ForeignKey(
                        name: "FK_StaticUploads_AspNetUsers_UploadedBy",
                        column: x => x.UploadedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StaticUploadFiles",
                columns: table => new
                {
                    StaticUploadFileId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RelativePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    Sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    StaticUploadId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaticUploadFiles", x => x.StaticUploadFileId);
                    table.ForeignKey(
                        name: "FK_StaticUploadFiles_StaticUploads_StaticUploadId",
                        column: x => x.StaticUploadId,
                        principalTable: "StaticUploads",
                        principalColumn: "StaticUploadId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StaticUploadFiles_StaticUploadId",
                table: "StaticUploadFiles",
                column: "StaticUploadId");

            migrationBuilder.CreateIndex(
                name: "IX_StaticUploads_FolderPath",
                table: "StaticUploads",
                column: "FolderPath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaticUploads_UploadedBy",
                table: "StaticUploads",
                column: "UploadedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaticUploadFiles");

            migrationBuilder.DropTable(
                name: "StaticUploads");
        }
    }
}
