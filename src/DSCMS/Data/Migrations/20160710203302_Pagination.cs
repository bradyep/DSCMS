using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DSCMS.Data.Migrations
{
    public partial class Pagination : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsForContentType",
                table: "Templates",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ItemsPerPage",
                table: "ContentTypes",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsForContentType",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "ItemsPerPage",
                table: "ContentTypes");
        }
    }
}
