using AutoMapper;
using Core.Models.User;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Mappers;

public class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<RegisterRequest, UserEntity>()
            .ForMember(x => x.PasswordHash, opt => opt.Ignore());

        CreateMap<UserEntity, UserModel>();

    }
}
