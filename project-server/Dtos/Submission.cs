using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace project_server.Dtos
{
    public class Submission
    {
        public int Id { get; set; }
        public decimal RateStars {  get; set; }
        public decimal FrequencyPlanets { get; set; }
        public short NearEarth { get; set; }
        public decimal FractionLife { get; set; }
        public decimal FractionIntelligence { get; set; }
        public decimal FractionCommunication { get; set; }
        public long Length { get; set; }
    }
}
