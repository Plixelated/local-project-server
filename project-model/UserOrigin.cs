using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using project_model;

namespace project_model;

//This table links a userID to an OriginID.

//The ensures that if a user already has an entry
//and submitted values in the datbase, and they
//register for an account afterwards, that their
//user account is linked to their existing OriginID

[Table("UserOrigin")]
public partial class UserOrigin
{
    //FK userID
    [ForeignKey(nameof(User))]
    public string UserId { get; set; }
    //Link for FK
    public virtual ProjectUser User { get; set; }

    //FK OriginID
    [ForeignKey(nameof(Entry))]
    public string EntryOrigin { get; set; }
    //Link for FK
    public virtual Entry Entry { get; set; }
}
