using Hazel;
using UnityEngine;

namespace SkidMenu.routines
{
    public class PetPlayerRoutine : IRoutine
    {
        public PetPlayerRoutine()
        {
            RoutineName = "PetPlayer";
        }

        public bool _enabled = false;
        private PlayerControl target;

        public override bool Enabled
        {
            get => AmPetting(PlayersSection.selectedPlayer);
            set
            {
                if (value && (_enabled != value || !AmPetting(PlayersSection.selectedPlayer)))
                {
                    target = PlayersSection.selectedPlayer;
                    _enabled = true;
                    SkidMenu.notifications.Send("Pet Player", $"Petting {target.Data.PlayerName}", 2f);
                }
                else if (!value && AmPetting(PlayersSection.selectedPlayer))
                {
                    Disable();
                }
            }
        }

        public bool AmPetting(PlayerControl player)
        {
            return _enabled && target != null && player != null && target.PlayerId == player.PlayerId;
        }

        private float _timer;

        public override void Run()
        {
            if (PlayerControl.LocalPlayer == null || target == null)
            {
                Disable();
                return;
            }

            _timer += Time.deltaTime;
            if (_timer < 0.45f) return;
            _timer = 0f;

            var pet = PlayerControl.LocalPlayer.cosmetics?.CurrentPet;
            if (pet == null) return;

            Vector2 petPosition = target.transform.position;
            petPosition.y += 0.12f;

            pet.SetGettingPet(true, petPosition);
            PlayerControl.LocalPlayer.cosmetics.PettingHand.StartPet(pet);

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.MyPhysics.NetId,
                (byte)RpcCalls.Pet,
                SendOption.Reliable,
                -1
            );
            NetHelpers.WriteVector2(PlayerControl.LocalPlayer.GetTruePosition(), writer);
            NetHelpers.WriteVector2(petPosition, writer);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        private void Disable()
        {
            _enabled = false;
            target = null;
            if (PlayerControl.LocalPlayer != null)
                PlayerControl.LocalPlayer.MyPhysics.RpcCancelPet();
        }

        public void OnDisconnect()
        {
            if (_enabled)
                SkidMenu.notifications.Send("Pet Player", "Pet Player was disabled as you left the game.", 5f);
            Disable();
        }
    }
}
