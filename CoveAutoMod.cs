using Cove.Server.Plugins;
using Cove.Server;

// Change the namespace and class name!
namespace CoveAutoMod
{
    public class CoveAutoMod : CovePlugin
    {
        public CoveAutoMod(CoveServer server) : base(server) { }

        public override void onInit()
        {
            base.onInit();

            Log("Hello world!");
        }

    }
}