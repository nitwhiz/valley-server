using Newtonsoft.Json;
using StardewValley;

namespace ValleyServer.server.data
{
    [JsonObject(MemberSerialization.OptIn)]
    public class NpcLocationData
    {
        [JsonProperty("npcName")] public readonly string NpcName;
        
        [JsonProperty("npcHasBirthday")] public readonly bool NpcHasBirthday;

        [JsonProperty("npcFriendshipPoints")] public readonly int NpcFriendshipPoints;
        
        [JsonProperty("npcGiftsThisWeek")] public readonly int NpcGiftsThisWeek;

        [JsonProperty("locationName")] public readonly string LocationName;
        
        [JsonProperty("tileX")] public readonly int TileX;

        [JsonProperty("tileY")] public readonly int TileY;

        public NpcLocationData(GameLocation location, NPC npc)
        {
            NpcName = npc.displayName;
            NpcHasBirthday = npc.isBirthday(Game1.currentSeason, Game1.dayOfMonth);

            if (Game1.MasterPlayer.friendshipData.TryGetValue(npc.Name, out var friendship))
            {
                NpcFriendshipPoints = Game1.MasterPlayer.getFriendshipHeartLevelForNPC(npc.Name);
                NpcGiftsThisWeek = friendship.GiftsThisWeek;
            }
            else
            {
                NpcFriendshipPoints = 0;
                NpcGiftsThisWeek = 0;
            }

            LocationName = location.NameOrUniqueName;

            TileX = npc.getTileX();
            TileY = npc.getTileY();
        }
    }
}