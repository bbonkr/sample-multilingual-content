using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sample.MultilingualContent.Migrations
{
    public partial class Addidgen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Localization_Languages_LanguageId",
                table: "Localization");

            migrationBuilder.DropForeignKey(
                name: "FK_Localization_LocalizationSet_LocalizationSetId",
                table: "Localization");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Localization",
                table: "Localization");

            migrationBuilder.RenameTable(
                name: "Localization",
                newName: "Localizations");

            migrationBuilder.RenameIndex(
                name: "IX_Localization_LocalizationSetId",
                table: "Localizations",
                newName: "IX_Localizations_LocalizationSetId");

            migrationBuilder.RenameIndex(
                name: "IX_Localization_LanguageId",
                table: "Localizations",
                newName: "IX_Localizations_LanguageId");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Posts",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "LocalizationSet",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Languages",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Localizations",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Localizations",
                table: "Localizations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Localizations_Languages_LanguageId",
                table: "Localizations",
                column: "LanguageId",
                principalTable: "Languages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Localizations_LocalizationSet_LocalizationSetId",
                table: "Localizations",
                column: "LocalizationSetId",
                principalTable: "LocalizationSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Localizations_Languages_LanguageId",
                table: "Localizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Localizations_LocalizationSet_LocalizationSetId",
                table: "Localizations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Localizations",
                table: "Localizations");

            migrationBuilder.RenameTable(
                name: "Localizations",
                newName: "Localization");

            migrationBuilder.RenameIndex(
                name: "IX_Localizations_LocalizationSetId",
                table: "Localization",
                newName: "IX_Localization_LocalizationSetId");

            migrationBuilder.RenameIndex(
                name: "IX_Localizations_LanguageId",
                table: "Localization",
                newName: "IX_Localization_LanguageId");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Posts",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "LocalizationSet",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Languages",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Localization",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Localization",
                table: "Localization",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Localization_Languages_LanguageId",
                table: "Localization",
                column: "LanguageId",
                principalTable: "Languages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Localization_LocalizationSet_LocalizationSetId",
                table: "Localization",
                column: "LocalizationSetId",
                principalTable: "LocalizationSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
