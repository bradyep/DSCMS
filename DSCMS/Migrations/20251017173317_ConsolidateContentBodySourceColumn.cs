using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSCMS.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateContentBodySourceColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Rename Body to BodySource
            migrationBuilder.RenameColumn(
                name: "Body",
                table: "Contents",
                newName: "BodySource");

            // Step 2: Add BodySourceTypeId column with default value 4 (HTML)
            migrationBuilder.AddColumn<int>(
                name: "BodySourceTypeId",
                table: "Contents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 4); // All existing content body sources are HTML

            // Step 3: Add DisplayOrder column (nullable for future functionality)
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Contents",
                type: "INTEGER",
                nullable: true);

            // Step 4: Add foreign key and index
            migrationBuilder.CreateIndex(
                name: "IX_Contents_BodySourceTypeId",
                table: "Contents",
                column: "BodySourceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contents_SourceTypes_BodySourceTypeId",
                table: "Contents",
                column: "BodySourceTypeId",
                principalTable: "SourceTypes",
                principalColumn: "SourceTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contents_SourceTypes_BodySourceTypeId",
                table: "Contents");

            migrationBuilder.DropIndex(
                name: "IX_Contents_BodySourceTypeId",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "BodySourceTypeId",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Contents");

            migrationBuilder.RenameColumn(
                name: "BodySource",
                table: "Contents",
                newName: "Body");
        }
    }
}
