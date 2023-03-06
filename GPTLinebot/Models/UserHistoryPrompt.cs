namespace GPTLinebot.Models
{
    public class UserHistoryPrompt
    {
        public List<string> HistoryPrompt { get; set; }

        public UserHistoryPrompt()
        {
            HistoryPrompt=new List<string>();
        }
    }
}
