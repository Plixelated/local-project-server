using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using project_model;

namespace project_model;

//This table keeps track of entries submitted
//It stores the EntryID, OriginID and a list of
//Values submitted by the user. The helps to track
//multiple submissions be organizing them under
//a single entry.

[Table("Entry")]
public partial class Entry
{
    //TODO: Change to string for UUID
    [Key]
    [Column("id")]
    public int ID { get; set; }

    //UUID Sent via the Frontend
    [Column("origin")]
    [StringLength(255)]
    [Unicode(false)]
    [Required]
    public string Origin { get; set; } = string.Empty;

    //Link for FK
    [InverseProperty("Entry")]
    public virtual ICollection<Values> SubmittedValues { get; set; } = new List<Values>(); //Use ICollection for Lists
    public UserOrigin UserOrigin { get; set; } //Link for FK
}
