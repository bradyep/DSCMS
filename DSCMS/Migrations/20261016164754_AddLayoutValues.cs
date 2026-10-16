using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSCMS.Migrations
{
    /// <inheritdoc />
    public partial class AddLayoutValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LayoutValues",
                columns: table => new
                {
                    LayoutValueId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    LayoutId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LayoutValues", x => x.LayoutValueId);
                    table.ForeignKey(
                        name: "FK_LayoutValues_Layouts_LayoutId",
                        column: x => x.LayoutId,
                        principalTable: "Layouts",
                        principalColumn: "LayoutId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LayoutValues_LayoutId",
                table: "LayoutValues",
                column: "LayoutId");

            migrationBuilder.Sql(@"
INSERT INTO LayoutValues (Key, Value, LayoutId)
SELECT 'blog-title', 'bradyep.com', LayoutId FROM Layouts WHERE LayoutSource = '/Views/DSCMS/Layouts/_BootstrapBlog.cshtml'
UNION ALL
SELECT 'blog-description', 'A Vanity Website', LayoutId FROM Layouts WHERE LayoutSource = '/Views/DSCMS/Layouts/_BootstrapBlog.cshtml'
UNION ALL
SELECT 'blog-title', 'Syntonic Studios', LayoutId FROM Layouts WHERE LayoutSource = '/Views/DSCMS/Layouts/_BootstrapWide.cshtml'
UNION ALL
SELECT 'blog-description', 'Harmonious Engineering', LayoutId FROM Layouts WHERE LayoutSource = '/Views/DSCMS/Layouts/_BootstrapWide.cshtml';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LayoutValues");
        }
    }
}
