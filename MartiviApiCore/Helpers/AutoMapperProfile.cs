using AutoMapper;
using MartiviApi.Models.Users;
using MartiviApi.Models;

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