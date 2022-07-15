using DiscordRPC;

namespace AnimDL.Api
{
    public static class DiscordRpc
    {
        private static readonly DiscordRpcClient client = new DiscordRpcClient("997177919052984622");

        public static void Initialize()
        {
            client.Initialize();
        }

        public static void SetPresense(RichPresence presence)
        {
            if(!client.IsInitialized)
            {
                return;
            }

            client.SetPresence(presence);
        }
    }
}
