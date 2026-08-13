using Hazel;

namespace SkidMenu.anticheat.rpc
{
    internal class SetLevel : RpcCheck
    {
        public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
        {
            uint level = reader.ReadPackedUInt32();
            uint maxLevel = (uint)SkidMenu.maxPlayerLevel;

            if (level > maxLevel)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} sent SetLevel RPC with level too high ({level} > {maxLevel}).");
                blockRpc = true;
                player.SetLevel(maxLevel);
            }

            if (ShipStatus.Instance)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} sent SetLevel RPC while a game is already in progress.");
            }
        }

        public override RpcCalls GetRpcCall() => RpcCalls.SetLevel;
    }
}
