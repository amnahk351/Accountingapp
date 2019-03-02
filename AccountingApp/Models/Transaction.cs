namespace AccountingApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Transaction")]
    public partial class Transaction
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public double AccountNumber { get; set; }

        [Column(TypeName = "money")]
        public decimal Debit { get; set; }

        [Column(TypeName = "money")]
        public decimal Credit { get; set; }

        [Required]
        [StringLength(10)]
        public string Status { get; set; }

        public DateTime DateSubmitted { get; set; }

        public int EntryID { get; set; }
    }
}
