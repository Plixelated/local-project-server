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
    public int Id { get; set; }
}
