using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
  [ServiceFilter(typeof(LogUserActivity))]
  [Authorize]
  [Route("/api/[controller]")]
  [ApiController]
  public class UsersController : ControllerBase
  {
    private readonly IDatingRepository _repo;
    private readonly IMapper _mapper;

    public UsersController(IDatingRepository repo, IMapper mapper)
    {
      _repo = repo;
      _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
    {
      var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
      var currentUser = await _repo.GetUser(currentUserId);
      userParams.UserId = currentUserId;
      if (string.IsNullOrEmpty(userParams.Gender))
      {
        userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
      }
      var users = await _repo.GetUsers(userParams);
      var usersToReturn = _mapper.Map<IEnumerable<UsersForListDto>>(users);

      Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
      return Ok(usersToReturn);
    }

    [HttpGet("{id}", Name = "GetUser")]
    public async Task<IActionResult> GetUser(int id)
    {
      var user = await _repo.GetUser(id);
      var userToReturn = _mapper.Map<UserForDetailedDto>(user);
      return Ok(userToReturn);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
    {
      if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        return Unauthorized();

      var userFromRepo = await _repo.GetUser(id);
      _mapper.Map(userForUpdateDto, userFromRepo);

      if (await _repo.SaveAll()) return NoContent();

      throw new System.Exception($"Updating user {id} failed on save");
    }

    [HttpPost("{likerId}/like/{likeeId}")]
    public async Task<IActionResult> LikeUser(int likerId, int likeeId)
    {
      if (likerId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        return Unauthorized();

      var like = await _repo.GetLike(likerId, likeeId);
      if (like != null) return BadRequest("You have already liked this User.");
      if (await _repo.GetUser(likeeId) == null) return NotFound();
      like = new Like
      {
        LikerId = likerId,
        LikeeId = likeeId
      };
      _repo.Add<Like>(like);
      if (await _repo.SaveAll()) return Ok();
      return BadRequest("Could not like this User at this time.");
    }
  }
}