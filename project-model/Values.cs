﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using project_model;

namespace project_model;

[Table("Values")]
public partial class Values
{
    [Key]
    [Column("submission_id")]
    public int SubmissionID { get; set; }

    //FORMULA VARIABLES
    //Star Formation Rate
    [Column("r_s",TypeName = "decimal(3,2)")]
    public decimal RateStars {  get; set; }

    //% that planets will form
    [Column("f_p", TypeName = "decimal(3,2)")]
    public decimal FrequencyPlanets { get; set; }

    //% of planets that support life
    [Column("n_e", TypeName = "decimal(3,2)")]
    public short NearEarth { get; set; }

    //% planets that develop life
    [Column("f_l", TypeName = "decimal(3,2)")]
    public decimal FractionLife { get; set; }

    //% planets that develop intelligent life
    [Column("f_i", TypeName = "decimal(3,2)")]
    public decimal FractionIntelligence { get; set; }

    //% of planets that develop detectable technology
    [Column("f_c", TypeName = "decimal(3,2)")]
    public decimal FractionCommunication { get; set;}

    //length of time they release detectable information
    [Column("l")]
    public long Length { get; set; }

    //FOREIGN KEY
    [ForeignKey("EntryOrigin")]
    [InverseProperty("SubmittedValues")]
    public virtual Entry Entry { get; set; } = null;
}