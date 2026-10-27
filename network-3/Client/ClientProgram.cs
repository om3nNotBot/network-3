using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TicToeToe;

 
class ClientProgram
{
    static UdpClient udp = null!;
    static IPEndPoint remote = null!;
    static readonly BlockingCollection<string> inbox = new();
    static int[,] board = Board.New();
    static string name1 = "", name2 = "", currentTurn = "";

    public static void Run()
    {
        Console.WriteLine("=== UDP Tic-Tac-Toe: CLIENT (Oyunçu 2 - O) ===");
        Console.Write("Adınızı daxil edin: ");
        name2 = (Console.ReadLine() ?? "").Trim();
        if (name2 == "") name2 = "Oyunçu2";

        Console.Write("Game IP (default 127.0.0.1): ");
        string ip = (Console.ReadLine() ?? "").Trim();
        if (ip == "") ip = "127.0.0.1";
        Console.Write("Game port (default 5000): ");
        if (!int.TryParse(Console.ReadLine(), out int port)) port = 5000;

        udp = new UdpClient(0);
        remote = new IPEndPoint(IPAddress.Parse(ip), port);
        Send(Board.Make("JOIN", "", name2, board, ""));
        Console.WriteLine("Birinci oyunçudan cavab gözlənilir...");

     
        while (true)
        {
            var ep = new IPEndPoint(IPAddress.Any, 0);
            var data = Board.FromJson(Encoding.UTF8.GetString(udp.Receive(ref ep)));
            if (data != null && data.MessageType == "START" && Board.IsValid(data))
            {
                name1 = data.Gamer1;
                name2 = data.Gamer2;  
                board = Board.ToArray(data.Game);
                currentTurn = data.CurrentTurn;
                break;
            }
        }
        Board.Save(name1, name2, board);

        Console.WriteLine($"\n{name1} (X)  vs  {name2} (O)");
        Task.Run(ReceiveLoop);
        Play();

        udp.Close();
        Console.WriteLine("Oyun bitdi. Çıxmaq üçün Enter basın.");
        Console.ReadLine();
    }

    static void Send(GameData data)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(Board.ToJson(data));
        udp.Send(bytes, bytes.Length, remote);
    }

    static void ReceiveLoop()
    {
        while (true)
        {
            try
            {
                var ep = new IPEndPoint(IPAddress.Any, 0);
                inbox.Add(Encoding.UTF8.GetString(udp.Receive(ref ep)));
            }
            catch (ObjectDisposedException) { break; }
            catch (SocketException) { }
        }
    }

    static void Play()
    {
        const int myMark = Board.O;
        while (true)
        {
            Board.Print(board);
            int result = Board.Result(board);
            if (result != Board.None)
            {
                if (result == Board.Draw) Console.WriteLine("Heç-heçə!");
                else Console.WriteLine(result == myMark ? "Təbriklər, siz qalib gəldiniz!" : "Rəqib qalib gəldi.");
                return;
            }

            if (currentTurn == name2)
            {
                while (true)
                {
                    Console.Write("Sizin gediş (sətir sütun, 1-3; çıxış üçün q): ");
                    string input = (Console.ReadLine() ?? "").Trim();
                    if (input.ToLower() == "q")
                    {
                        Send(Board.Make("BYE", name1, name2, board, currentTurn));
                        return;
                    }
                    var p = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (p.Length == 2 && int.TryParse(p[0], out int r) && int.TryParse(p[1], out int c)
                        && r >= 1 && r <= 3 && c >= 1 && c <= 3 && board[r - 1, c - 1] == Board.Empty)
                    {
                        board[r - 1, c - 1] = myMark;
                        currentTurn = name1;
                        Send(Board.Make("MOVE", name1, name2, board, currentTurn, Board.WinnerName(board, name1, name2)));
                        Board.Save(name1, name2, board);
                        break;
                    }
                    Console.WriteLine("Yanlış gediş! Xana boş olmalıdır.");
                }
            }
            else
            {
                Console.WriteLine("Rəqibin gedişi gözlənilir...");
                var data = Board.FromJson(inbox.Take());
                if (data == null) continue;
                if (data.MessageType == "BYE") { Console.WriteLine("Rəqib oyunu tərk etdi."); return; }
                if (data.MessageType == "MOVE" && Board.IsValid(data))
                {
                    board = Board.ToArray(data.Game);
                    currentTurn = data.CurrentTurn;
                    Board.Save(name1, name2, board);
                }
            }
        }
    }
}