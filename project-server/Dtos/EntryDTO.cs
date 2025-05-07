namespace project_server.Dtos
{
    public class EntryDTO
    {
        //public int ID { get; set; }
        public string OriginID { get; set; } = string.Empty;
        public List<GroupedDataDTO> SubmittedValues { get; set; } = new List<GroupedDataDTO>();
    }
}
