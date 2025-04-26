namespace project_server.Dtos
{
    public class EntryDTO
    {
        public int ID { get; set; }
        public string Origin { get; set; } = string.Empty;
        public List<ValuesDTO> SubmittedValues { get; set; } = new List<ValuesDTO>();
    }
}
