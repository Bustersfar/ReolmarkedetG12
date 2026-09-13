namespace ReolmarkedetG12.Core.Repositories;

public interface IRepository<T>
{
    void Add(T item); // Create
    T? GetById(int id); // Read
    void Update(T item); // Update
    void Delete(int id); // Delete
    IEnumerable<T> GetAll();
}
