using System;

namespace DatingApp.API.Helpers
{
  public class MessageForReturnDto
  {
    public int Id { get; set; }
    public int SenderId { get; set; }
    public string SenderKnownAs { get; set; }
    public string SenderPhotoUrl { get; set; }
    public int RecipientId { get; set; }
    public string RecipientKnownAs { get; set; }
    public string RecipientPhotoUrl { get; set; }
    public string Content { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset? DateRead { get; set; }
    public DateTimeOffset MessageSent { get; set; }
  }
}