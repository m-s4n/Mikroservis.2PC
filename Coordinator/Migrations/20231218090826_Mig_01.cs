using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Coordinator.Migrations
{
    /// <inheritdoc />
    public partial class Mig_01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nodes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nodes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nodes_state",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    node_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_ready = table.Column<int>(type: "integer", nullable: false),
                    transaction_state = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nodes_state", x => x.id);
                    table.ForeignKey(
                        name: "fk_nodes_state_nodes_node_id",
                        column: x => x.node_id,
                        principalTable: "nodes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "nodes",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { new Guid("ad8cd0dd-20de-4a50-8963-e51b1f110edc"), "Order.API" },
                    { new Guid("c95b9428-1c85-45e5-8474-2751b0219d7b"), "Payment.API" },
                    { new Guid("f39acc9d-2299-4ee8-9a76-f6809fabda9d"), "Stock.API" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_nodes_state_node_id",
                table: "nodes_state",
                column: "node_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nodes_state");

            migrationBuilder.DropTable(
                name: "nodes");
        }
    }
}
