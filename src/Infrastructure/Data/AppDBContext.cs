using ApplicationCore.Entities;
using ApplicationCore.Entities.Shopping;
using ApplicationCore.Entities.Catalog;
using ApplicationCore.Entities.Orders;
using ApplicationCore.Entities.Payments;
using ApplicationCore.Entities.Warehousing;
using ApplicationCore.Entities.Fulfillment;
using ApplicationCore.Entities.Shipping;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext // nghĩa là class của bạn kế thừa từ DbContext của EF Core. Bạn có thể hiểu: DbContext = cầu nối giữa code C# và database Nó chịu trách nhiệm những việc như: kết nối database, theo dõi entity, query dữ liệu, add/update/delete, save xuống db, mapping entity với bảng
// "Tôi tạo một DbContext riêng cho application của tôi."
{
    public AppDbContext(DbContextOptions<AppDbContext> options) // DbContextOptions<AppDbContext> chứa cấu hình cho database. Ví dụ sau này trong Program.cs bạn sẽ có kiểu:builder.Services.AddDbContext<AppDbContext>(options =>options.UseSqlServer(connectionString)); Thì: UseSqlServer(...) connection string provider SQL Server các config EF được đóng gói thành: DbContextOptions<AppDbContext> rồi DI truyền vào constructor: public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) // nghĩa là: gửi options này lên constructor của class cha DbContext. Vì DbContext mới là thằng thực sự cần config này để biết: kết nối database nào, dùng provider nào, connection string nào
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<RefreshToken> RefreshTokens => Set < RefreshToken > ();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<InventoryReservation> InventoryReservations => Set<InventoryReservation>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentItem> ShipmentItems => Set<ShipmentItem>();
    public DbSet<ShippingProfile> ShippingProfiles => Set<ShippingProfile>();
    /*
     Đoạn này:

        public DbSet<Product> Products => Set<Product>();

        cực kỳ quan trọng.

        DbSet<Product> có thể hiểu là:

        tập hợp các Product mà EF Core quản lý.

        Ví dụ bạn có database:

        Products
        ----------------
        Id
        Name

        thì trong C#:

        DbSet<Product> Products

        gần như đại diện cho bảng đó.

        Bạn có thể dùng:

        context.Products

        để query:

        var products = await context.Products.ToListAsync();

        Hoặc:

        context.Products.Add(product);

        Hoặc:

        context.Products.Remove(product);

        Nó giống như một "cửa vào" để thao tác với Product.

        Bạn có thể viết cách khác:

        public DbSet<Product> Products { get; set; }

        nhưng cách:

        public DbSet<Product> Products => Set<Product>();

        gọn hơn và tránh nullable warning trong nhiều trường hợp.
        
        Hai cách về ý nghĩa gần tương tự.
     */

    protected override void OnModelCreating(ModelBuilder modelBuilder) // Đây là method EF Core gọi khi nó đang xây dựng model database. Bạn có thể hiểu: "Khi EF chuẩn bị hiểu các entity map ra bảng như thế nào, hãy chạy code ở đây." Ví dụ EF cần biết: Product map vào bảng nào? Id có phải primary key không? Name dài tối đa bao nhiêu? Quan hệ Order - OrderItem thế nào?
    {
        base.OnModelCreating(modelBuilder); // Những config đó được apply trong OnModelCreating. Dòng này: base.OnModelCreating(modelBuilder); nghĩa là gọi logic mặc định của class cha DbContext. Bạn có thể hiểu: EF Core xử lý config mặc định trước-> sau đó mình thêm config riêng

        modelBuilder.ApplyConfigurationsFromAssembly( 
            typeof(AppDbContext).Assembly
        );
        // Đoạn này dùng để EF Core tự tìm các class 
        /*
            Ví dụ bạn có:

            Infrastructure/
            └── Data/
                ├── AppDbContext.cs
                └── Config/
                    ├── ProductConfiguration.cs
                    ├── OrderConfiguration.cs
                    └── UserConfiguration.cs
        Trong đó:
        public class ProductConfiguration
            : IEntityTypeConfiguration<Product>
        và:
        public class OrderConfiguration
            : IEntityTypeConfiguration<Order>
        Thì:
        ApplyConfigurationsFromAssembly(...)
        sẽ quét assembly hiện tại và tự apply tất cả các class kiểu: IEntityTypeConfiguration<T> Tức là bạn không cần viết:
        modelBuilder.ApplyConfiguration(
            new ProductConfiguration()
        );

        modelBuilder.ApplyConfiguration(
            new OrderConfiguration()
        );
        mỗi lần thêm entity mới.
        Còn:
        typeof(AppDbContext).Assembly
        nghĩa là:
        lấy assembly chứa class AppDbContext.
        Trong trường hợp của bạn, nó chính là project:
        Infrastructure
        nên EF sẽ quét project Infrastructure để tìm:
        ProductConfiguration
        OrderConfiguration
        ...
        Đây là lý do ProductConfiguration.cs nên nằm trong Infrastructure.
        Toàn bộ flow sẽ là:
        Product.cs
        ApplicationCore
           ↓
        AppDbContext biết Product tồn tại
           ↓
        ProductConfiguration
        Infrastructure
           ↓
        EF Core đọc mapping
           ↓
        Migration
           ↓
        SQL Server
           ↓
        Products table
        Ví dụ:
        public DbSet<Product> Products => Set<Product>();
        nói với EF:

        "Tôi có entity Product."
        Còn:
        ProductConfiguration
        nói:
        "Product phải map thành bảng thế nào."
        Bạn có thể nhớ cực ngắn:
        Product
        = dữ liệu/nghiệp vụ

        ProductConfiguration
        = luật map Product → SQL

        AppDbContext
        = trung tâm EF Core quản lý entity + database
         */
    }
}
