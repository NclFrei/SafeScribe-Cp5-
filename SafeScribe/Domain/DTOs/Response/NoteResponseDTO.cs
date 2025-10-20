namespace SafeScribe.Domain.DTOs.Response;

public record NoteResponseDTO (Guid Id, string Title, string Content, DateTime CreatedAt, Guid UserId);

