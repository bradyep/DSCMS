using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSCMS.Migrations
{
    /// <inheritdoc />
    public partial class RenameIsForContentTypeToIsForMultipleContents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsForContentType",
                table: "Templates",
                newName: "IsForMultipleContents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsForMultipleContents",
                table: "Templates",
                newName: "IsForContentType");
        }
    }
}
