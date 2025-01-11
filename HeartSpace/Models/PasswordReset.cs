using HeartSpace.Models.EFModels;
using System;

public class PasswordReset
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string ResetToken { get; set; }
    public DateTime ExpiryDate { get; set; }
    public virtual Member Member { get; set; }
}
