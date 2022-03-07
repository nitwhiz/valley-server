using StardewModdingAPI;
using ValleyServer.server;
using ValleyServer.server.resource;

namespace ValleyServer
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            var server = new HttpServer(Monitor);

            server.AddResource("/locations/npcs", NpcLocations.GetLocations);

            server.Start();

            Monitor.Log("server started", LogLevel.Info);
        }
    }
}