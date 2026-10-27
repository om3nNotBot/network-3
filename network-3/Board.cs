using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace TicToeToe;

static class Board
{
    public const int Empty = -1, X = 1, O = 0, Draw = 2, None = -2;

    public static int[,] New()
    {
        var b = new int[3, 3];
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++) b[i, j] = Empty;
        return b;
    }

    public static int Count(int[,] b, int mark)
    {
        int n = 0;
        foreach (int v in b) if (v == mark) n++;
        return n;
    }

    public static int Result(int[,] b)
    {
        for (int i = 0; i < 3; i++)
        {
            if (b[i, 0] != Empty && b[i, 0] == b[i, 1] && b[i, 1] == b[i, 2]) return b[i, 0];
            if (b[0, i] != Empty && b[0, i] == b[1, i] && b[1, i] == b[2, i]) return b[0, i];
        }
        if (b[1, 1] != Empty)
        {
            if (b[0, 0] == b[1, 1] && b[1, 1] == b[2, 2]) return b[1, 1];
            if (b[0, 2] == b[1, 1] && b[1, 1] == b[2, 0]) return b[1, 1];
        }
        return Count(b, Empty) == 0 ? Draw : None;
    }

    public static string WinnerName(int[,] b, string g1, string g2)
    {
        int r = Result(b);
        return r == X ? g1 : r == O ? g2 : r == Draw ? "Draw" : "";
    }

    public static string TurnFromBoard(int[,] b, string g1, string g2) =>
        Count(b, X) == Count(b, O) ? g1 : g2;

    public static void Print(int[,] b)
    {
        Console.WriteLine("\n    1   2   3");
        for (int i = 0; i < 3; i++)
        {
            Console.Write($" {i + 1}  ");
            for (int j = 0; j < 3; j++)
            {
                char ch = b[i, j] == X ? 'X' : b[i, j] == O ? 'O' : ' ';
                Console.Write(ch + (j < 2 ? " | " : ""));
            }
            Console.WriteLine();
            if (i < 2) Console.WriteLine("    ---------");
        }
        Console.WriteLine();
    }

    public static int[][] ToJagged(int[,] b)
    {
        var r = new int[3][];
        for (int i = 0; i < 3; i++)
        {
            r[i] = new int[3];
            for (int j = 0; j < 3; j++) r[i][j] = b[i, j];
        }
        return r;
    }

    public static int[,] ToArray(int[][] g)
    {
        var b = new int[3, 3];
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++) b[i, j] = g[i][j];
        return b;
    }
    public static GameData Make(string type, string g1, string g2, int[,] b, string turn, string winner = "") =>
        new GameData
        {
            MessageType = type,
            Gamer1 = g1,
            Gamer2 = g2,
            Game = ToJagged(b),
            CurrentTurn = turn,
            Winner = winner
        };

    public static string ToJson(GameData d) => JsonSerializer.Serialize(d);

    public static GameData? FromJson(string json)
    {
        try { return JsonSerializer.Deserialize<GameData>(json); }
        catch { return null; }
    }

    public static bool IsValid(GameData d) =>
        d.Game != null && d.Game.Length == 3 && d.Game.All(r => r != null && r.Length == 3);


    public static string FileName(string g1, string g2)
    {
        string Clean(string s) => string.Concat(s.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));
        return $"{Clean(g1)}-{Clean(g2)}.json";
    }

    public static void Save(string g1, string g2, int[,] b)
    {
        try
        {
            string json = JsonSerializer.Serialize(new { Gamer1 = g1, Gamer2 = g2, Game = ToJagged(b) });
            File.WriteAllText(FileName(g1, g2), json);
        }
        catch { }
    }

    public static int[,]? Load(string file)
    {
        try
        {
            var d = FromJson(File.ReadAllText(file));
            return d != null && IsValid(d) ? ToArray(d.Game) : null;
        }
        catch { return null; }
    }
}