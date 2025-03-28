using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using project_model;

namespace project_model;

[Table("Entry")]
public partial class Entry
{
    [Key]
    [Column("id")]
    public int ID { get; set; }

    [Column("origin")]
    [StringLength(255)]
    [Unicode(false)]
    [Required]
    public string Origin { get; set; } = string.Empty;

    [InverseProperty("Entry")]
    public virtual ICollection<Values> SubmittedValues { get; set; } = new List<Values>();

}
