namespace HeartSpace.Models.EFModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class EventMember
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public int MemberId { get; set; }
    }
}
