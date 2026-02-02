using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSCMS.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateLayoutSourceColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Rename FileLocation to LayoutSource
            migrationBuilder.RenameColumn(
                name: "FileLocation",
                table: "Layouts",
                newName: "LayoutSource");

            // Step 2: Add SourceTypeId column with default value 1 (RazorFile)
            migrationBuilder.AddColumn<int>(
                name: "SourceTypeId",
                table: "Layouts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1); // All existing layouts are RazorFile

            // Step 3: Drop FileContents column (not needed anymore)
            migrationBuilder.DropColumn(
                name: "FileContents",
                table: "Layouts");

            // Step 4: Add foreign key and index
            migrationBuilder.CreateIndex(
                name: "IX_Layouts_SourceTypeId",
                table: "Layouts",
                column: "SourceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Layouts_SourceTypes_SourceTypeId",
                table: "Layouts",
                column: "SourceTypeId",
                principalTable: "SourceTypes",
                principalColumn: "SourceTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop foreign key and index
            migrationBuilder.DropForeignKey(
                name: "FK_Layouts_SourceTypes_SourceTypeId",
                table: "Layouts");

            migrationBuilder.DropIndex(
                name: "IX_Layouts_SourceTypeId",
                table: "Layouts");

            // Step 2: Add back FileContents column
            migrationBuilder.AddColumn<string>(
                name: "FileContents",
                table: "Layouts",
                type: "TEXT",
                nullable: true);

            // Step 3: Drop SourceTypeId column
            migrationBuilder.DropColumn(
                name: "SourceTypeId",
                table: "Layouts");

            // Step 4: Rename LayoutSource back to FileLocation
            migrationBuilder.RenameColumn(
                name: "LayoutSource",
                table: "Layouts",
                newName: "FileLocation");
        }
    }
}
