using AutoMapper;
using SafeScribe.Domain.DTOs.Request;
using SafeScribe.Domain.DTOs.Response;
using SafeScribe.Domain.Interfaces;
using SafeScribe.Domain.Models;

namespace SafeScribe.Application.Services;

public class NoteService
{
    private readonly INoteRepository _repository;
    private readonly IMapper _mapper;

    public NoteService(INoteRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<NoteResponseDTO?> GetByIdAsync(Guid id, Guid userId, string role)
    {
        var note = await _repository.GetByIdAsync(id);
        if (note is null) return null;
        if (role != "Admin" && note.UserId != userId) return null;

        return _mapper.Map<NoteResponseDTO>(note);
    }

    public async Task<NoteResponseDTO> CreateAsync(NoteCreateDTO dto, Guid userId)
    {
        var note = _mapper.Map<Note>(dto);
        note.UserId = userId;

        await _repository.AddAsync(note);
        await _repository.SaveChangesAsync();

        return _mapper.Map<NoteResponseDTO>(note);
    }

    public async Task<bool> UpdateAsync(Guid id, NoteCreateDTO dto, Guid userId, string role)
    {
        var note = await _repository.GetByIdAsync(id);
        if (note is null) return false;
        if (role != "Admin" && note.UserId != userId) return false;

        _mapper.Map(dto, note);
        _repository.Update(note);
        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var note = await _repository.GetByIdAsync(id);
        if (note is null) return false;

        _repository.Delete(note);
        await _repository.SaveChangesAsync();
        return true;
    }
}