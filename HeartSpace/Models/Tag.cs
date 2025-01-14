namespace HeartSpace.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Tag
    {
        public int Id { get; set; }

        [Required]
        [StringLength(15)]
        public string TagName { get; set; }
    }
}
