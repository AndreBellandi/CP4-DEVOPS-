using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClyvoVetApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TUTORES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Nome = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Telefone = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TUTORES", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PETS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Nome = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Especie = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Raca = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    DataNascimento = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    TutorId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PETS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PETS_TUTORES_TutorId",
                        column: x => x.TutorId,
                        principalTable: "TUTORES",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CONSULTAS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Data = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Descricao = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Veterinario = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Observacoes = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PetId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONSULTAS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CONSULTAS_PETS_PetId",
                        column: x => x.PetId,
                        principalTable: "PETS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VACINAS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Nome = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    DataAplicacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ProximaDose = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    Aplicada = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    PetId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VACINAS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VACINAS_PETS_PetId",
                        column: x => x.PetId,
                        principalTable: "PETS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTAS_PetId",
                table: "CONSULTAS",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_PETS_TutorId",
                table: "PETS",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_VACINAS_PetId",
                table: "VACINAS",
                column: "PetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CONSULTAS");

            migrationBuilder.DropTable(
                name: "VACINAS");

            migrationBuilder.DropTable(
                name: "PETS");

            migrationBuilder.DropTable(
                name: "TUTORES");
        }
    }
}
