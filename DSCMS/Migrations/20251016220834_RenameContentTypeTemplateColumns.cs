using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSCMS.Migrations
{
    /// <inheritdoc />
    public partial class RenameContentTypeTemplateColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentTypes_Templates_DefaultTemplateForContent",
                table: "ContentTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ContentTypes_Templates_TemplateId",
                table: "ContentTypes");

            migrationBuilder.RenameColumn(
                name: "TemplateId",
                table: "ContentTypes",
                newName: "MultipleContentsTemplateId");

            migrationBuilder.RenameColumn(
                name: "DefaultTemplateForContent",
                table: "ContentTypes",
                newName: "DefaultSingleContentTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_ContentTypes_TemplateId",
                table: "ContentTypes",
                newName: "IX_ContentTypes_MultipleContentsTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_ContentTypes_DefaultTemplateForContent",
                table: "ContentTypes",
                newName: "IX_ContentTypes_DefaultSingleContentTemplateId");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultContentType",
                table: "ContentTypes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentTypes_Templates_DefaultSingleContentTemplateId",
                table: "ContentTypes",
                column: "DefaultSingleContentTemplateId",
                principalTable: "Templates",
                principalColumn: "TemplateId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentTypes_Templates_MultipleContentsTemplateId",
                table: "ContentTypes",
                column: "MultipleContentsTemplateId",
                principalTable: "Templates",
                principalColumn: "TemplateId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentTypes_Templates_DefaultSingleContentTemplateId",
                table: "ContentTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ContentTypes_Templates_MultipleContentsTemplateId",
                table: "ContentTypes");

            migrationBuilder.DropColumn(
                name: "IsDefaultContentType",
                table: "ContentTypes");

            migrationBuilder.RenameColumn(
                name: "MultipleContentsTemplateId",
                table: "ContentTypes",
                newName: "TemplateId");

            migrationBuilder.RenameColumn(
                name: "DefaultSingleContentTemplateId",
                table: "ContentTypes",
                newName: "DefaultTemplateForContent");

            migrationBuilder.RenameIndex(
                name: "IX_ContentTypes_MultipleContentsTemplateId",
                table: "ContentTypes",
                newName: "IX_ContentTypes_TemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_ContentTypes_DefaultSingleContentTemplateId",
                table: "ContentTypes",
                newName: "IX_ContentTypes_DefaultTemplateForContent");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentTypes_Templates_DefaultTemplateForContent",
                table: "ContentTypes",
                column: "DefaultTemplateForContent",
                principalTable: "Templates",
                principalColumn: "TemplateId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentTypes_Templates_TemplateId",
                table: "ContentTypes",
                column: "TemplateId",
                principalTable: "Templates",
                principalColumn: "TemplateId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
