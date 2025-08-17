using System;

using PhotoController;

namespace PhotoMain;
public class Photo
{
    public static void Main(string[] args)
    {
        if (args.Length <= 1)
        {
            Controller controller = new Controller(args);
            controller.Run();
        }
        else
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine("Usage: photo [db-file-name]");
        }
    }
}
