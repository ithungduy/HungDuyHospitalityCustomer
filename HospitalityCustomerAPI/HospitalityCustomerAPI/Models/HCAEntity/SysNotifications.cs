using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.HCAEntity;

public partial class SysNotifications
{
    public Guid Ma { get; set; }

    public string? UserPhone { get; set; }

    public string? Title { get; set; }

    public string? Message { get; set; }

    public string? NotificationType { get; set; }

    public string? Data { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public string? FcmToken { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? MessageId { get; set; }
}
