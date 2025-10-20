using AutoMapper;
using SafeScribe.Domain.DTOs.Request;
using SafeScribe.Domain.DTOs.Response;
using SafeScribe.Domain.Models;

namespace SafeScribe.Application.Mappings;

public class NoteProfile : Profile
{
    public NoteProfile()
    {
        CreateMap<NoteCreateDTO, Note>()
            .ForMember(dest => dest.CreateAt, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore());

        CreateMap<Note, NoteResponseDTO>();
    }
}