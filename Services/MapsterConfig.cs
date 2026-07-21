using Mapster;
using Domain.Entities;
using Services.DTOs.Auth;

namespace Services;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Map User to UserResponse
        config.NewConfig<User, UserResponse>()
            .Map(dest => dest.RoleName, src => src.Role != null ? src.Role.Name : string.Empty)
            .Map(dest => dest.Status, src => src.Status.ToString());

    }
}