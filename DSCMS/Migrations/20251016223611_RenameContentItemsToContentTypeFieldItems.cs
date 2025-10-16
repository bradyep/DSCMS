using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSCMS.Migrations
{
    /// <inheritdoc />
    public partial class RenameContentItemsToContentTypeFieldItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentItems");

            migrationBuilder.CreateTable(
                name: "ContentTypeFieldItems",
                columns: table => new
                {
                    ContentTypeFieldItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    ContentTypeFieldId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContentId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentTypeFieldItems", x => x.ContentTypeFieldItemId);
                    table.ForeignKey(
                        name: "FK_ContentTypeFieldItems_ContentTypeFields_ContentTypeFieldId",
                        column: x => x.ContentTypeFieldId,
                        principalTable: "ContentTypeFields",
                        principalColumn: "ContentTypeFieldId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentTypeFieldItems_Contents_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Contents",
                        principalColumn: "ContentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentTypeFieldItems_ContentId",
                table: "ContentTypeFieldItems",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentTypeFieldItems_ContentTypeFieldId",
                table: "ContentTypeFieldItems",
                column: "ContentTypeFieldId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentTypeFieldItems");

            migrationBuilder.CreateTable(
                name: "ContentItems",
                columns: table => new
                {
                    ContentItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContentTypeFieldId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentItems", x => x.ContentItemId);
                    table.ForeignKey(
                        name: "FK_ContentItems_ContentTypeFields_ContentTypeFieldId",
                        column: x => x.ContentTypeFieldId,
                        principalTable: "ContentTypeFields",
                        principalColumn: "ContentTypeFieldId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentItems_Contents_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Contents",
                        principalColumn: "ContentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentItems_ContentId",
                table: "ContentItems",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentItems_ContentTypeFieldId",
                table: "ContentItems",
                column: "ContentTypeFieldId");
        }
    }
}
