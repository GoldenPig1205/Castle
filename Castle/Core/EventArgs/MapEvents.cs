using Decals;
using Exiled.Events.EventArgs.Map;
using MEC;

namespace Castle.Core.EventArgs
{
    public class MapEvents
    {
        public static void OnPickupAdded(PickupAddedEventArgs ev)
        {
            if (ev.Pickup.Base.name.Contains($"[P]") || ev.Pickup.Base.name.Contains($"[O]"))
                return;

            Timing.CallDelayed(180, () =>
            {
                if (ev.Pickup != null)
                    ev.Pickup.Destroy();
            });
        }
    }
}
