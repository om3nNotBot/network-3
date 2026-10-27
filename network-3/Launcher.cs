using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;


class Launcher
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Write("1) Game (Oyunçu 1 - X)   2) Client (Oyunçu 2 - O): ");
        if (Console.ReadLine()?.Trim() == "1")
            TicToeToe.GameProgram.Run();
        else
            TicToeToe.ClientProgram.Run();
    }
}