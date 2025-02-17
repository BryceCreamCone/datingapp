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
  [Route("api/users/{userId}/[controller]")]
  [ApiController]
  public class MessagesController : ControllerBase
  {
    private readonly IDatingRepository _repo;
    private readonly IMapper _mapper;

    public MessagesController(IDatingRepository repo, IMapper mapper)
    {
      _repo = repo;
      _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetMessagesForUser(int userId, [FromQuery]MessageParams messageParams)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        return Unauthorized();

      messageParams.UserId = userId;

      var messsagesFromRepo = await _repo.GetMessagesForUser(messageParams);
      var messages = _mapper.Map<IEnumerable<MessageForReturnDto>>(messsagesFromRepo);

      Response.AddPagination(
        messsagesFromRepo.CurrentPage,
        messsagesFromRepo.PageSize,
        messsagesFromRepo.TotalCount,
        messsagesFromRepo.TotalPages
      );

      return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
    {
      var sender = await _repo.GetUser(userId);
      if (sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        return Unauthorized();

      messageForCreationDto.SenderId = sender.Id;
      var recipient = await _repo.GetUser(messageForCreationDto.RecipientId);
      if (recipient == null) return BadRequest("User Not Found");

      var message = _mapper.Map<Message>(messageForCreationDto);
      _repo.Add(message);
      if (await _repo.SaveAll())
      {
        var messageForReturn = _mapper.Map<MessageForReturnDto>(message);
        return CreatedAtRoute("GetMessage", new { userId, id = message.Id }, messageForReturn);
      }

      throw new System.Exception("Failed To Save Message At This Time");
    }

    [HttpPost("{id}")]
    public async Task<IActionResult> DeleteMessage(int userId, int id)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        return Unauthorized();

      var messageFromRepo = await _repo.GetMessage(id);
      if (messageFromRepo.SenderId == userId) messageFromRepo.SenderDeleted = true;
      if (messageFromRepo.RecipientId == userId) messageFromRepo.RecipientDeleted = true;
      if (messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted) _repo.Delete(messageFromRepo);

      if (await _repo.SaveAll()) return NoContent();

      throw new System.Exception("Could Not Delete Message At This Time.");
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkMessageAsRead(int userId, int id)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        return Unauthorized();

      var message = await _repo.GetMessage(id);

      if (message.RecipientId != userId) return Unauthorized();

      message.IsRead = true;
      message.DateRead = System.DateTime.Now;
      await _repo.SaveAll();
      return NoContent();
    }

    [HttpGet("{id}", Name = "GetMessage")]
    public async Task<IActionResult> GetMessage(int userId, int id)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        return Unauthorized();

      var messageFromRepo = await _repo.GetMessage(id);
      if (messageFromRepo == null) return NotFound();
      return Ok(messageFromRepo);
    }

    [HttpGet("thread/{recipientId}")]
    public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        return Unauthorized();

      var messagesFromRepo = await _repo.GetMessageThread(userId, recipientId);
      var messageThread = _mapper.Map<IEnumerable<MessageForReturnDto>>(messagesFromRepo);

      foreach (var message in messagesFromRepo)
      {
        if (message.IsRead == false)
        {
          message.IsRead = true;
          message.DateRead = System.DateTime.Now;
        }
      }
      await _repo.SaveAll();
      return Ok(messageThread);
    }
  }
}