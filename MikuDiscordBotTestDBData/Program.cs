namespace MikuDiscordBotTestDBData
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var songs = new Songs();
            //songs.InsertDumpSongData(55, 1111315944757796924, 3);
            songs.OnlyGetRangeOfSong(3, 1111315944757796924, 3);

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}