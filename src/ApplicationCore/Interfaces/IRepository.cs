namespace ApplicationCore.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    void Add(T entity);

    void Delete(T entity); // tương tự như update vậy
    void Update(T entity); // void vì khi thao tác chưa thay đổi còn thêm bước save nữa mới thay đổi thật sự

    Task<int> SaveChangesAsync(); // Vì SaveChangesAsync() của EF Core trả về số lượng bản ghi/state entry bị ảnh hưởng sau khi save. Nên là kiểu int
}