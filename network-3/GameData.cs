using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TicToeToe
{
    public class GameData
    {
        [JsonPropertyName("Gamer1")]
        public string Gamer1 { get; set; } = "";

        [JsonPropertyName("Gamer2")]
        public string Gamer2 { get; set; } = "";

        [JsonPropertyName("Game")]
        public int[][] Game { get; set; } = new int[3][]
        {
        new int[3],
        new int[3],
        new int[3]
        };

        public string MessageType { get; set; } = "MOVE";
        public string CurrentTurn { get; set; } = "";
        public string Winner { get; set; } = "";
    }
}