using MingweiSamuel.Camille.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using WebUtil.Models;

namespace LolCrawler.Models
{
    public class ChampionInfo
    {
        public int Difficulty { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Magic { get; set; }
    }
}
