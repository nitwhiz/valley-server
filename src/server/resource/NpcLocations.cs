using System.Collections.Generic;
using Newtonsoft.Json;
using StardewValley;
using ValleyServer.server.data;

namespace ValleyServer.server.resource
{
    public class NpcLocations
    {
        public static string GetLocations()
        {
            var locationData = new List<NpcLocationData>();

            foreach (var location in Game1.locations)
            {
                foreach (var npc in location.characters)
                {
                    locationData.Add(new NpcLocationData(location, npc));
                }
            }

            return JsonConvert.SerializeObject(locationData, Formatting.Indented);
        }
    }
}