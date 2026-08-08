using UnityEngine;

namespace SkidMenu.routines
{
	public class RoutineManager : MonoBehaviour
	{
		public AutoTriggerSporesRoutine autoTriggerSpores = new AutoTriggerSporesRoutine();
		public DiscoHostRoutine discoHost = new DiscoHostRoutine();
		public DoorTrollerRoutine doorTroller = new DoorTrollerRoutine();
		public PlayerFollowerRoutine playerFollower = new PlayerFollowerRoutine();
		public ReportBodySpam reportBodySpam = new ReportBodySpam();
		public FullyRandomizeRoutine fullyRandomize = new FullyRandomizeRoutine();
		public TeleportFlooderRoutine teleportFlooder = new TeleportFlooderRoutine();

		public void Update()
		{
			if(autoTriggerSpores.Enabled) autoTriggerSpores.Run();
			if(discoHost.Enabled) discoHost.Run();
			if(doorTroller.Enabled) doorTroller.Run();
			if(playerFollower._enabled) playerFollower.Run();
			if(reportBodySpam.Enabled) reportBodySpam.Run();
			if(fullyRandomize.Enabled) fullyRandomize.Run();
			if(teleportFlooder.Enabled) teleportFlooder.Run();
		}
	}
}
