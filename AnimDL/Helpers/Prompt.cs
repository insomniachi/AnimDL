namespace AnimDL.Helpers
{
    public class Prompt
    {
        public static int GetUserInput(string message)
        {
            Console.Write(message);
            int value;
            while (!int.TryParse(Console.ReadLine(), out value)) ;
            return value;
        }
    }
}
