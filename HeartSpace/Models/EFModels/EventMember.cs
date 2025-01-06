namespace HeartSpace.Models.EFModels
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

        public virtual Event Event { get; set; }

        public virtual Member Member { get; set; }
    }
}
