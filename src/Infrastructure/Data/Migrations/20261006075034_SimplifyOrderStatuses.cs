using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyOrderStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE [Orders]
                SET [Status] = CASE [Status]
                    WHEN 3 THEN 2
                    WHEN 4 THEN 2
                    WHEN 5 THEN 2
                    WHEN 6 THEN 3
                    WHEN 7 THEN 4
                    WHEN 8 THEN 5
                    ELSE [Status]
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE [Orders]
                SET [Status] = CASE [Status]
                    WHEN 3 THEN 6
                    WHEN 4 THEN 7
                    WHEN 5 THEN 8
                    ELSE [Status]
                END;
                """);
        }
    }
}
