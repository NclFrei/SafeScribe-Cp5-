using SafeScribe.Domain.Models;

namespace SafeScribe.Domain.Interfaces;

public interface INoteRepository
{
    Task<IEnumerable<Note>> GetAllAsync();
    Task<Note?> GetByIdAsync(Guid id);
    Task AddAsync(Note note);
    void Update(Note note);
    void Delete(Note note);
    Task<IEnumerable<Note>> GetNotesByUserIdAsync(Guid userId);
    Task SaveChangesAsync();
}