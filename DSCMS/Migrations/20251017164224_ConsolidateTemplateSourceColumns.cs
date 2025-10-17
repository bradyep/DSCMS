using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSCMS.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateTemplateSourceColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add new columns (TemplateSource and SourceTypeId)
            migrationBuilder.AddColumn<string>(
                name: "TemplateSource",
                table: "Templates",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceTypeId",
                table: "Templates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1); // Default to RazorFile

            // Step 2: Migrate data - if FileLocation has data, copy to TemplateSource and set SourceTypeId=1
            migrationBuilder.Sql(@"
                UPDATE Templates 
                SET TemplateSource = FileLocation, 
                    SourceTypeId = 1 
                WHERE FileLocation IS NOT NULL AND FileLocation != '';
            ");

            // Step 3: Migrate data - if FileContents has data (and FileLocation is null/empty), copy to TemplateSource and set SourceTypeId=2
            migrationBuilder.Sql(@"
                UPDATE Templates 
                SET TemplateSource = FileContents, 
                    SourceTypeId = 2 
                WHERE (FileLocation IS NULL OR FileLocation = '') 
                  AND FileContents IS NOT NULL AND FileContents != '';
            ");

            // Step 4: Drop old columns
            migrationBuilder.DropColumn(
                name: "FileContents",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "FileLocation",
                table: "Templates");

            // Step 5: Add foreign key and index
            migrationBuilder.CreateIndex(
                name: "IX_Templates_SourceTypeId",
                table: "Templates",
                column: "SourceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Templates_SourceTypes_SourceTypeId",
                table: "Templates",
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
                name: "FK_Templates_SourceTypes_SourceTypeId",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_SourceTypeId",
                table: "Templates");

            // Step 2: Add back old columns
            migrationBuilder.AddColumn<string>(
                name: "FileLocation",
                table: "Templates",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileContents",
                table: "Templates",
                type: "TEXT",
                nullable: true);

            // Step 3: Restore data - if SourceTypeId=1 (RazorFile), copy TemplateSource back to FileLocation
            migrationBuilder.Sql(@"
                UPDATE Templates 
                SET FileLocation = TemplateSource 
                WHERE SourceTypeId = 1;
            ");

            // Step 4: Restore data - if SourceTypeId=2 (InlineRazor), copy TemplateSource back to FileContents
            migrationBuilder.Sql(@"
                UPDATE Templates 
                SET FileContents = TemplateSource 
                WHERE SourceTypeId = 2;
            ");

            // Step 5: Drop new columns
            migrationBuilder.DropColumn(
                name: "SourceTypeId",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "TemplateSource",
                table: "Templates");
        }
    }
}
