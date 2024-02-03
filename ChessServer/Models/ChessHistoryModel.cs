namespace ChessServer.Models
{
    public class ChessMoveModel
    {
        public string After { get; set; }
        public string Before { get; set; }
        public string Color { get; set; }
        public string Flags { get; set; }        
        public string From { get; set; }
        public string Lan { get; set; }
        public string Piece { get; set; }
        public string San { get; set; }
        public string To { get; set; }

    }
}
