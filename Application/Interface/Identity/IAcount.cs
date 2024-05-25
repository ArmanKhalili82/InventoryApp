using Application.DTO.Request.Identity;
using Application.DTO.Response;
using Application.DTO.Response.Identity;

namespace Application.Interface.Identity
{
    public interface IAcount
    {
        Task<ServiceResponse> LoginAsync(LoginUserRequestDTO model);
        Task<ServiceResponse> CreateUserAsync(CreateUserRequestDTO model);
        Task<IEnumerable<GetUserWithClaimResponseDTO>> GetUserWithClaimsAsync();
        Task SetUpAsync();
        Task<ServiceResponse> UpdateUserAsync(ChangeUserClaimRequestDTO model);
        //Task SaveActivityAsync(ActivityTrackerRequestDTO model);
        //Task<IEnumerable<ActivityTrackerResponseDTO>> GetActivitiesAsync();
    }
}
