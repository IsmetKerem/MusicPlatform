using MusicPlatform.Entity.Enums;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Playlist;

namespace MusicPlatform.Business.Services.Abstract;

public interface IPlaylistService
{
    Task<ApiResponse<List<PlaylistListDto>>> GetMyPlaylistsAsync(int userId);
    Task<ApiResponse<PlaylistDetailDto>> GetByIdAsync(int playlistId, int userId, PackageLevel userPackage);
    Task<ApiResponse<PlaylistListDto>> CreateAsync(int userId, CreatePlaylistDto dto);
    Task<ApiResponse> UpdateAsync(int playlistId, int userId, CreatePlaylistDto dto);
    Task<ApiResponse> DeleteAsync(int playlistId, int userId);
    Task<ApiResponse> AddSongAsync(int playlistId, int userId, int songId);
    Task<ApiResponse> RemoveSongAsync(int playlistId, int userId, int songId);
}