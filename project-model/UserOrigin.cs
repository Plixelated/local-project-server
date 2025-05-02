using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using project_model;

namespace project_model;

[Table("UserOrigin")]
public partial class UserOrigin
{
    //Set up later 5/1
/*    [ForeignKey(nameof(User))]
    public string UserId { get; set; }
    public virtual ProjectUser User { get; set; }

    [ForeignKey(nameof(Entry))]
    public string EntryOrigin { get; set; }
    public virtual Entry Entry { get; set; }*/
}
