using Cove.Server.Plugins;
using Cove.Server;

// Change the namespace and class name!
namespace MyCovePlugin
{
    public class MyCovePlugin : CovePlugin
    {
        public MyCovePlugin(CoveServer server) : base(server) { }

        public override void onInit()
        {
            base.onInit();

            Log("Hello world!");
        }

    }
}