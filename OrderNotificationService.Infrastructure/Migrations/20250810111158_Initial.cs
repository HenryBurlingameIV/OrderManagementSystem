using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OrderNotificationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TemplateText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "NotificationTemplates",
                columns: new[] { "Id", "Name", "TemplateText" },
                values: new object[,]
                {
                    { 1, "Создан", "Ваш заказ {OrderId} создан." },
                    { 2, "Отменен", "Ваш заказ {OrderId} отменен." },
                    { 3, "В обработке", "Ваш заказ {OrderId} находится в обработке." },
                    { 4, "Готов к доставке", "Обработка вашего заказа {OrderId} завершена. Заказ готов к доставке." },
                    { 5, "В доставке", "Ваш заказ {OrderId} находится в процессе доставки." },
                    { 6, "Доставлен", "Ваш заказ {OrderId} доставлен по указанному адресу." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationTemplates");
        }
    }
}
