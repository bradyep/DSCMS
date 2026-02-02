using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSCMS.Migrations
{
    /// <inheritdoc />
    public partial class RenameContentTypeItemsToContentTypeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentItems_ContentTypeItems_ContentTypeItemId",
                table: "ContentItems");

            migrationBuilder.DropTable(
                name: "ContentTypeItems");

            migrationBuilder.RenameColumn(
                name: "ContentTypeItemId",
                table: "ContentItems",
                newName: "ContentTypeFieldId");

            migrationBuilder.RenameIndex(
                name: "IX_ContentItems_ContentTypeItemId",
                table: "ContentItems",
                newName: "IX_ContentItems_ContentTypeFieldId");

            migrationBuilder.AlterColumn<string>(
                name: "FileContents",
                table: "Layouts",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "ContentTypeFields",
                columns: table => new
                {
                    ContentTypeFieldId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Required = table.Column<bool>(type: "INTEGER", nullable: false),
                    ContentTypeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentTypeFields", x => x.ContentTypeFieldId);
                    table.ForeignKey(
                        name: "FK_ContentTypeFields_ContentTypes_ContentTypeId",
                        column: x => x.ContentTypeId,
                        principalTable: "ContentTypes",
                        principalColumn: "ContentTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentTypeFields_ContentTypeId",
                table: "ContentTypeFields",
                column: "ContentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentItems_ContentTypeFields_ContentTypeFieldId",
                table: "ContentItems",
                column: "ContentTypeFieldId",
                principalTable: "ContentTypeFields",
                principalColumn: "ContentTypeFieldId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentItems_ContentTypeFields_ContentTypeFieldId",
                table: "ContentItems");

            migrationBuilder.DropTable(
                name: "ContentTypeFields");

            migrationBuilder.RenameColumn(
                name: "ContentTypeFieldId",
                table: "ContentItems",
                newName: "ContentTypeItemId");

            migrationBuilder.RenameIndex(
                name: "IX_ContentItems_ContentTypeFieldId",
                table: "ContentItems",
                newName: "IX_ContentItems_ContentTypeItemId");

            migrationBuilder.AlterColumn<string>(
                name: "FileContents",
                table: "Layouts",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ContentTypeItems",
                columns: table => new
                {
                    ContentTypeItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContentTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentTypeItems", x => x.ContentTypeItemId);
                    table.ForeignKey(
                        name: "FK_ContentTypeItems_ContentTypes_ContentTypeId",
                        column: x => x.ContentTypeId,
                        principalTable: "ContentTypes",
                        principalColumn: "ContentTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentTypeItems_ContentTypeId",
                table: "ContentTypeItems",
                column: "ContentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentItems_ContentTypeItems_ContentTypeItemId",
                table: "ContentItems",
                column: "ContentTypeItemId",
                principalTable: "ContentTypeItems",
                principalColumn: "ContentTypeItemId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
