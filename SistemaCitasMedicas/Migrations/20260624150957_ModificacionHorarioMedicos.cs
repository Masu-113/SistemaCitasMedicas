using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaCitasMedicas.Migrations
{
    /// <inheritdoc />
    public partial class ModificacionHorarioMedicos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HorariosMedico_Medicos_MedicoIdMedico",
                table: "HorariosMedico");

            migrationBuilder.DropIndex(
                name: "IX_HorariosMedico_MedicoIdMedico",
                table: "HorariosMedico");

            migrationBuilder.DropColumn(
                name: "MedicoIdMedico",
                table: "HorariosMedico");

            migrationBuilder.AddForeignKey(
                name: "FK_HorariosMedico_Medicos_IdMedico",
                table: "HorariosMedico",
                column: "IdMedico",
                principalTable: "Medicos",
                principalColumn: "IdMedico",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HorariosMedico_Medicos_IdMedico",
                table: "HorariosMedico");

            migrationBuilder.AddColumn<int>(
                name: "MedicoIdMedico",
                table: "HorariosMedico",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_HorariosMedico_MedicoIdMedico",
                table: "HorariosMedico",
                column: "MedicoIdMedico");

            migrationBuilder.AddForeignKey(
                name: "FK_HorariosMedico_Medicos_MedicoIdMedico",
                table: "HorariosMedico",
                column: "MedicoIdMedico",
                principalTable: "Medicos",
                principalColumn: "IdMedico",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
