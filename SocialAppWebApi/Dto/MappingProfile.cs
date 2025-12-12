using AutoMapper;
using SocialAppWebApi.Data;

namespace SocialAppWebApi.Dto;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Post, PostDto>().ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author.Username));
    }
}