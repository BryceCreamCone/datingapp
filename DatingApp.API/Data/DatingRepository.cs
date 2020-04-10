using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
  public class DatingRepository : IDatingRepository
  {
    private readonly DataContext _context;

    public DatingRepository(DataContext context)
    {
      _context = context;
    }
    public void Add<T>(T entity) where T : class
    {
      _context.Add(entity);
    }

    public void Delete<T>(T entity) where T : class
    {
      _context.Remove(entity);
    }

    public async Task<Like> GetLike(int likerId, int likeeId)
    {
      return await _context.Likes.FirstOrDefaultAsync(u =>
        u.LikerId == likerId && u.LikeeId == likeeId);
    }

    public async Task<Photo> GetMainPhotoForUser(int userId)
    {
      return await _context.Photos.Where(u => u.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
    }

    public async Task<Photo> GetPhoto(int id)
    {
      var photo = await _context.Photos.FirstOrDefaultAsync(u => u.Id == id);
      return photo;
    }

    public async Task<User> GetUser(int id)
    {
      var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);
      return user;
    }

    public async Task<PagedList<User>> GetUsers(UserParams userParams)
    {
      var users = _context.Users.Include(p => p.Photos)
        .Where(u => u.Photos.Any())
        .Where(u => u.Id != userParams.UserId)
        .Where(u => u.Gender == userParams.Gender)
        .OrderByDescending(u => u.LastActive)
        .AsQueryable();

      if (userParams.Likers)
      {
        var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
        users = users.Where(u => userLikers.Contains(u.Id));
      }
      if (userParams.Likees)
      {
        var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
        users = users.Where(u => userLikees.Contains(u.Id));
      }

      if (userParams.MinAge != 18 || userParams.MaxAge != 120)
      {
        var minDob = System.DateTime.Today.AddYears(-userParams.MaxAge - 1);
        var maxDob = System.DateTime.Today.AddYears(-userParams.MinAge);

        users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
      }
      if (!string.IsNullOrEmpty(userParams.OrderBy))
      {
        switch (userParams.OrderBy)
        {
          case "created":
            users = users.OrderByDescending(u => u.Created);
            break;
          default:
            users = users.OrderByDescending(u => u.LastActive);
            break;
        }
      }
      return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
    }

    private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
    {
      var user = await _context.Users
        .Include(x => x.Likers)
        .Include(x => x.Likees)
        .FirstOrDefaultAsync(u => u.Id == id);

      if (likers) return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
      return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
    }

    public async Task<bool> SaveAll()
    {
      return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Message> GetMessage(int id)
    {
      return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
    {
      var messages = _context.Messages
        .Include(m => m.Sender).ThenInclude(u => u.Photos)
        .Include(m => m.Recipient).ThenInclude(u => u.Photos)
        .AsQueryable();

      switch (messageParams.MessageContainer)
      {
        case "Inbox":
          messages = messages.Where(m => m.RecipientId == messageParams.UserId);
          break;
        case "Outbox":
          messages = messages.Where(m => m.SenderId == messageParams.UserId);
          break;
        default:
          messages = messages.Where(m => m.SenderId == messageParams.UserId && m.IsRead == false);
          break;
      }

      messages = messages.OrderByDescending(m => m.MessageSent);
      return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
    }

    public Task<IEnumerable<Message>> GetMessageThread(int senderId, int recipientId)
    {
      throw new System.NotImplementedException();
    }
  }
}