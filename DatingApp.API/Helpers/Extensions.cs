using System;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace DatingApp.API.Helpers
{
  public static class Extensions
  {
    public static void AddApplicationError(this HttpResponse response, string message)
    {
      response.Headers.Add("Application-Error", message);
      response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
      response.Headers.Add("Access-Control-Allow-Origin", "*");
    }

    public static int CalculateAge(this DateTime dateTimeVar)
    {
      var age = DateTime.Today.Year - dateTimeVar.Year;
      if (dateTimeVar.AddYears(age) > DateTime.Today) age--;
      return age;
    }

    // public static IMappingExpression AddPhotoUrl(this IMappingExpression mapping, Expression lambda)
    // {
    //   return (
    //     mapping.ForMember(
    //       dest => dest.PhotoUrl,
    //       opt => opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url))
    //   );
    // }
  }
}