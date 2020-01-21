using AutoMapper;
using MartiviApi.Models.Users;
using MartiviApiCore.Models;
using MartiviApiCore.Models.Users;

namespace MartiviApiCore.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserModel>();
            CreateMap<RegisterModel, User>();
            CreateMap<UpdateModel, User>();
        }
    }
}