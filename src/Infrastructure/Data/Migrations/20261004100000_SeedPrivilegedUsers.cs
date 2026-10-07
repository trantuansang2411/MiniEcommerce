using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261004100000_SeedPrivilegedUsers")]
public partial class SeedPrivilegedUsers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            INSERT INTO [Users]
                ([Id], [RoleId], [Email], [PasswordHash], [GoogleId], [IsEmailVerified], [Status], [CreatedAt], [UpdatedAt])
            VALUES
                ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '22222222-2222-2222-2222-222222222222', 'admin@miniproject.local', 'AQAAAAIAAYagAAAAEKHTFflMqKy8+I7Ngg5vmvnfIuEuIlcAFHXChmYt66NDEdhaJQkC2iSnGoq3tOeewg==', NULL, 1, 2, '2026-10-04T00:00:00+00:00', '2026-10-04T00:00:00+00:00'),
                ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '44444444-4444-4444-4444-444444444444', 'manager@miniproject.local', 'AQAAAAIAAYagAAAAEKHTFflMqKy8+I7Ngg5vmvnfIuEuIlcAFHXChmYt66NDEdhaJQkC2iSnGoq3tOeewg==', NULL, 1, 2, '2026-10-04T00:00:00+00:00', '2026-10-04T00:00:00+00:00'),
                ('cccccccc-cccc-cccc-cccc-cccccccccccc', '33333333-3333-3333-3333-333333333333', 'staff@miniproject.local', 'AQAAAAIAAYagAAAAEKHTFflMqKy8+I7Ngg5vmvnfIuEuIlcAFHXChmYt66NDEdhaJQkC2iSnGoq3tOeewg==', NULL, 1, 2, '2026-10-04T00:00:00+00:00', '2026-10-04T00:00:00+00:00');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM [Users]
            WHERE [Id] IN (
                'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
                'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
                'cccccccc-cccc-cccc-cccc-cccccccccccc');
            """);
    }
}
