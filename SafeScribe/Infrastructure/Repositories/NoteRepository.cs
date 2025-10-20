using Microsoft.EntityFrameworkCore;
using SafeScribe.Domain.Interfaces;
using SafeScribe.Domain.Models;
using SafeScribe.Infrastructure.Data;

namespace SafeScribe.Infrastructure.Repositories;

public class NoteRepository : INoteRepository
{
    private readonly SafeScribeContext _context;

    public NoteRepository(SafeScribeContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Note>> GetAllAsync()
    {
        return await _context.Notes.ToListAsync();
    }

    public async Task<Note?> GetByIdAsync(Guid id)
    {
        return await _context.Notes.FindAsync(id);
    }

    public async Task AddAsync(Note note)
    {
        await _context.Notes.AddAsync(note);
    }

    public void Update(Note note)
    {
        _context.Notes.Update(note);
    }

    public void Delete(Note note)
    {
        _context.Notes.Remove(note);
    }

    public async Task<IEnumerable<Note>> GetNotesByUserIdAsync(Guid userId)
    {
        return await _context.Notes
            .Where(n => n.UserId == userId)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}