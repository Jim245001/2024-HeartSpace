namespace HeartSpace.Models.EFModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string CategoryName { get; set; }

        public int DisplayOrder { get; set; }

		//public virtual ICollection<Event> Events { get; set; }
	}
}
