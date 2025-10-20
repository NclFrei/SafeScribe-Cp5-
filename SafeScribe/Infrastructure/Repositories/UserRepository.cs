using Microsoft.EntityFrameworkCore;
using SafeScribe.Domain.Interfaces;
using SafeScribe.Domain.Models;
using SafeScribe.Infrastructure.Data;

namespace SafeScribe.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SafeScribeContext _context;

    public UserRepository(SafeScribeContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _context.Users.AnyAsync(u => u.Username == username);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}