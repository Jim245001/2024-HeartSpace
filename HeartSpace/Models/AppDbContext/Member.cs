namespace HeartSpace.Models.EFModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Member
    {
        public int Id { get; set; }

        [Required]
        [StringLength(25)]
        public string Account { get; set; }

        [Required]
        [StringLength(25)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(50)]
        public string Email { get; set; }

        [Column(TypeName = "image")]
        public byte[] img { get; set; }

        [StringLength(50)]
        public string ConfirmCode { get; set; }

        [Required]
        [StringLength(25)]
        public string NickName { get; set; }

        public bool? IsConfirmed { get; set; }
    }
}
