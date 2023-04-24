using MingweiSamuel.Camille.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using EzAspDotNet.Models;

namespace LolCrawler.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ChampionInfo
    {
        public int Difficulty { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Magic { get; set; }
    }
}
