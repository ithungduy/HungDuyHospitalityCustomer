using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.HCAEntity;

public partial class ServiceCall
{
    public Guid Id { get; set; }

    public Guid OutletId { get; set; }

    public Guid TableId { get; set; }

    public string? TableCode { get; set; }

    public string? Note { get; set; }

    public byte Status { get; set; }

    public DateTime RequestedAt { get; set; }

    public DateTime? AcknowledgedAt { get; set; }

    public DateTime? DoneAt { get; set; }

    public string? StaffAck { get; set; }

    public string? CreatedIp { get; set; }

    public DateTime? DateResponse { get; set; }

    public Guid? UserResponse { get; set; }
}
