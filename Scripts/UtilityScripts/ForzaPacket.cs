using System.Runtime.InteropServices;
namespace Utility;
public struct ForzaPacket
{
	public int IsRaceOn = 0;

	public uint TimeStampMS = 0;

	public float EngineMaxRpm = 0f;
	public float EngineIdleRpm = 0f;
	public float CurrentEngineRpm = 0f;

	public float AccelerationX = 0f;
	public float AccelerationY = 0f;
	public float AccelerationZ = 0f;

	public float VelocityX = 0f;
	public float VelocityY = 0f;
	public float VelocityZ = 0f;

	public float AngularVelocityX = 0f;
	public float AngularVelocityY = 0f;
	public float AngularVelocityZ = 0f;

	public float Yaw = 0f;
	public float Pitch = 0f;
	public float Roll = 0f;

	public float NormalizedSuspensionTravelFrontLeft = 0f;
	public float NormalizedSuspensionTravelFrontRight = 0f;
	public float NormalizedSuspensionTravelRearLeft = 0f;
	public float NormalizedSuspensionTravelRearRight = 0f;

	public float TyreSlipRatioFrontLeft = 0f;
	public float TyreSlipRatioFrontRight = 0f;
	public float TyreSlipRatioRearLeft = 0f;
	public float TyreSlipRatioRearRight = 0f;

	public float WheelRotationSpeedFrontLeft = 0f;
	public float WheelRotationSpeedFrontRight = 0f;
	public float WheelRotationSpeedRearLeft = 0f;
	public float WheelRotationSpeedRearRight = 0f;

	public int WheelOnRumbleStripFrontLeft = 0;
	public int WheelOnRumbleStripFrontRight = 0;
	public int WheelOnRumbleStripRearLeft = 0;
	public int WheelOnRumbleStripRearRight = 0;

	public int WheelInPuddleFrontLeft = 0;
	public int WheelInPuddleFrontRight = 0;
	public int WheelInPuddleRearLeft = 0;
	public int WheelInPuddleRearRight = 0;

	public float SurfaceRumbleFrontLeft = 0f;
	public float SurfaceRumbleFrontRight = 0f;
	public float SurfaceRumbleRearLeft = 0f;
	public float SurfaceRumbleRearRight = 0f;

	public float TireSlipAngleFrontLeft = 0f;
	public float TireSlipAngleFrontRight = 0f;
	public float TireSlipAngleRearLeft = 0f;
	public float TireSlipAngleRearRight = 0f;

	public float TireCombinedSlipFrontLeft = 0f;
	public float TireCombinedSlipFrontRight = 0f;
	public float TireCombinedSlipRearLeft = 0f;
	public float TireCombinedSlipRearRight = 0f;

	public float SuspensionTravelMetersFrontLeft = 0f;
	public float SuspensionTravelMetersFrontRight = 0f;
	public float SuspensionTravelMetersRearLeft = 0f;
	public float SuspensionTravelMetersRearRight = 0f;

	public int CarOrdinal = 0;
	public int CarClass = 0;
	public int CarPerformanceIndex = 0;

	public int DrivetrainType = 0;

	public int NumCylinders = 0;

	public uint CarGroup = 0;

	public float SmashableVelDiff = 0f;

	public float SmashableMass = 0f;

	public float PositionX = 0f;
	public float PositionY = 0f;
	public float PositionZ = 0f;

	public float Speed = 0f;

	public float Power = 0f;

	public float Torque = 0f;

	public float TireTempFrontLeft = 0f;
	public float TireTempFrontRight = 0f;
	public float TireTempRearLeft = 0f;
	public float TireTempRearRight = 0f;

	public float Boost = 0f;

	public float Fuel = 0f;

	public float DistanceTraveled = 0f;

	public float BestLap = 0f;
	public float LastLap = 0f;
	public float CurrentLap = 0f;

	public float CurrentRaceTime = 0f;

	public ushort LapNumber = 0;

	public byte RacePosition = 0;

	public byte Accel = 0;
	public byte Brake = 0;
	public byte Clutch = 0;
	public byte HandBrake = 0;

	public byte Gear = 0;

	public sbyte Steer = 0;
	public sbyte NormalisedDrivingLine = 0;
	public sbyte NormalisedAIBrakeDifference = 0;

	public ForzaPacket(byte[] rawData)
	{
		this = MemoryMarshal.AsRef<ForzaPacket>(rawData);
	}
}

