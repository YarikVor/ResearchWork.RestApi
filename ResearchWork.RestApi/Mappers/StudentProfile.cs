using AutoMapper;
using ResearchWork.Entities;
using ResearchWork.RestApi.Dtos;

namespace ResearchWork.RestApi.Mappers;

public class StudentProfile : Profile
{
    public StudentProfile()
    {
        CreateProjection<Student, StudentShortItem>();
        CreateProjection<Student, StudentItem>()
            .ForMember(s => s.GroupName,
                f => f.MapFrom(p => p.Group.Name));

        CreateProjection<Student, StudentInfo>()
            .ForMember(s => s.GroupName,
                f => f.MapFrom(p => p.Group.Name));
    }
}