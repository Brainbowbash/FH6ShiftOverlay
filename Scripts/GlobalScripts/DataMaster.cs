// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2026 Brainbowbash
using Godot;
using System;
namespace Data;
public partial class DataMaster : Node
{
	static public float MaxRPM { get { return _maxRPM; } }
	static public float GaugeMaxRPM { get { return _gaugeMaxRPM; } }
	static public float IdleRPM { get { return _idleRPM; } }
	static public float CurrentRPM { get { return _currentRPM; } }
	static public bool IsElectric { get { return CylinderCount == 0; } }

	static public (float RPMNormalised, float Power, float Torque)[] EngineCurveNormalised { get { return _engineCurveNormalised; } }

	static public (float lowerRPMNormalised, float upperRPMNormalised, bool upperPowerbandCertain) PowerbandNormalised { get { return (_shiftDownRPM / _gaugeMaxRPM, _shiftUpRPM / _gaugeMaxRPM, _upperPowerbandCertain); } }

	static public int ShiftDirection { get { return _shiftDirection; } }

	static public float HighestPowerReading
	{
		get { return _highestPowerReading; }
	}
	static public (float, float) HighestPower
	{
		get { return (_highestPower * Enums.UnitConversion.Power[(int)_powerUnit], _highestPowerRPM); }
	}
	static public (float, float) HighestTorque
	{
		get { return (_highestTorque * Enums.UnitConversion.Torque[(int)_torqueUnit], _highestTorqueRPM); }
	}
	static public float CurrentPower
	{
		get { return _currentPower * Enums.UnitConversion.Power[(int)_powerUnit]; }
	}
	static public float CurrentTorque
	{
		get { return _currentTorque * Enums.UnitConversion.Torque[(int)_torqueUnit]; }
	}

	static public bool InMenu
	{
		get { return _inMenu; }
	}

	static private PacketPeerUdp forzaUDPStream = new PacketPeerUdp();
	static private Utility.CatmullRomSpline engineSpline = new Utility.CatmullRomSpline([[0, 0]], [0]);

	static private bool valuesDirty = true;
	static private bool splineUpdated = false;
	static private bool gearRatioChanged = false;

	static private int _port;
	static private string _address;
	static private bool _udpBound;

	static private Enums.PowerUnitType _powerUnit;
	static private Enums.TorqueUnitType _torqueUnit;

	static private float _maxRPM;
	static private float _idleRPM;
	static private float _gaugeMaxRPM;

	static private int CarID;
	static private int CarPI;
	static private int CylinderCount;

	static private float _highestPowerReading;

	static private float _highestPower;
	static private float _highestPowerRPM;

	static private float _highestTorque;
	static private float _highestTorqueRPM;

	static private float _highestRPM;

	static private float _currentRPM;
	static private float _currentPower;
	static private float _currentTorque;

	static private float ratio;
	static private float wheelSpeed;
	static private int _gear;
	static private int drivetrainType;
	static private uint gearChangeTimeStamp;

	static private float _shiftDownRPM = 0;
	static private float _shiftUpRPM = 0;

	static private float[] gearRatios = new float[10];
	static private (float RPMNormalised, float Power, float Torque)[] _engineCurveNormalised = new (float, float, float)[64];

	static private bool gearChanged = false;
	static private int _shiftDirection = 0;
	static private bool _inMenu = false;
	static private bool _upperPowerbandCertain = false;
	

	static public void UpdateSettings(int port, string IPAddress, Enums.PowerUnitType powerUnit, Enums.TorqueUnitType torqueUnit)
	{
		_port = port;
		_address = IPAddress;

		_torqueUnit = torqueUnit;
		_powerUnit = powerUnit;
		valuesDirty = true;
	}

	static public string BindUDP()
	{
		if (_udpBound)
		{
			CloseUDP();
		}

		Error err = forzaUDPStream.Bind(_port, _address);

		if (err == Error.Ok)
		{
			_udpBound = true;
			return "";
		}
		else
		{
			_udpBound = false;
			return $"Binding failed, error: {err}";
		}
	}

	static public void CloseUDP()
	{
		_udpBound = false;
		forzaUDPStream.Close();
	}

	static private bool IsInMenu(Utility.ForzaPacket packet)
	{
		bool inMenu = false;

		inMenu |= packet.CarPerformanceIndex == 0;

		inMenu |= (packet.CurrentEngineRpm < packet.EngineIdleRpm * 2f && _currentRPM == packet.CurrentEngineRpm)
			&& packet.Power == _currentPower
			&& MathF.Abs(packet.Power) < 10f
			&& packet.Speed == 0f
			&& packet.Clutch == 255
			&& packet.Gear == 1
			&& packet.DistanceTraveled == 0;


		if (_inMenu != inMenu)
		{
			_inMenu = inMenu;
			MenuChange();
		}

		return inMenu;
	}


	static private bool HasCarChanged(Utility.ForzaPacket packet)
	{
		bool hasChanged = false;
		if (IsInMenu(packet)) return false;

		hasChanged |= drivetrainType != packet.DrivetrainType;
		drivetrainType = packet.DrivetrainType;

		hasChanged |= CarPI != packet.CarPerformanceIndex;
		CarPI = packet.CarPerformanceIndex;

		hasChanged |= (CarID != 0 && CarID != packet.CarOrdinal);
		CarID = packet.CarOrdinal;

		hasChanged |= (_idleRPM != 0 && _idleRPM != packet.EngineIdleRpm);

		_idleRPM = packet.EngineIdleRpm;

		hasChanged |= _maxRPM != packet.EngineMaxRpm;
		_maxRPM = packet.EngineMaxRpm;
		_gaugeMaxRPM = MathF.Ceiling(_maxRPM / 1000f) * 1000f;

		hasChanged |= CylinderCount != packet.NumCylinders;
		CylinderCount = packet.NumCylinders;

		return hasChanged;
	}

	static private void AssignData()
	{
		byte[] incomingData = forzaUDPStream.GetPacket();

		if (incomingData.Length != 324)
		{
			return;
		}
		Utility.ForzaPacket packet = new Utility.ForzaPacket(incomingData);

		if (valuesDirty == true || HasCarChanged(packet))
		{
			engineSpline.Clear();
			gearRatios = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
			_highestRPM = 0f;
			_highestPowerReading = 0f;
			gearChangeTimeStamp = 0;
			valuesDirty = false;
		}

		gearChanged = false;
		if (_gear != packet.Gear)
		{
			gearChangeTimeStamp = packet.TimeStampMS;
			gearChanged = true;

		}
		_gear = packet.Gear;


		if (packet.Gear == 11 || packet.Gear == 0 || packet.Speed < 1f || packet.Clutch > 0f || _currentRPM < _idleRPM * 1.5f)
		{
			gearChangeTimeStamp = packet.TimeStampMS;
		}

		if (packet.Gear != 11 && packet.Gear != 0 && gearRatios[_gear - 1] == 0)
		{
			gearChangeTimeStamp = 0; // forces gear ratio calculation immediately
		}

		_currentRPM = packet.CurrentEngineRpm;

		if (_currentPower > 0 && _highestRPM < _currentRPM)
		{
			_highestRPM = _currentRPM;
		}

		_currentPower = packet.Power;
		_currentTorque = packet.Torque;

		if (packet.Speed > 1f)
		{
			_highestPowerReading = MathF.Max(packet.Power, _highestPowerReading);
		}

		switch (drivetrainType)
		{
			case 0:
				wheelSpeed = (packet.WheelRotationSpeedFrontLeft + packet.WheelRotationSpeedFrontRight) * 0.5f;
				break;

			case 1:
				wheelSpeed = (packet.WheelRotationSpeedRearLeft + packet.WheelRotationSpeedRearRight) * 0.5f;
				break;

			case 2:
			default:
				wheelSpeed = (packet.WheelRotationSpeedFrontLeft + packet.WheelRotationSpeedFrontRight + packet.WheelRotationSpeedRearLeft + packet.WheelRotationSpeedRearRight) * 0.25f;
				break;
		}

		ratio = packet.CurrentEngineRpm / wheelSpeed;
		
		if ((packet.TimeStampMS - gearChangeTimeStamp) > 800 && _gear != 11 && _gear != 0 && _currentRPM > _idleRPM * 1.5f && packet.Brake == 0 && wheelSpeed > 1 && ratio > 0)
		{
			if (gearRatios[_gear - 1] == 0f)
			{
				gearRatioChanged = true;
				gearRatios[_gear - 1] = ratio;
			}
			else
			{
				float error = MathF.Abs(gearRatios[_gear - 1] - ratio) / gearRatios[_gear - 1];

				if (error > 0.01f)
				{
					gearRatioChanged = true;
					gearRatios[_gear - 1] = ratio;
				}
			}
		}
		else
		{
			gearRatioChanged = false;
		}

		if (packet.Accel == 255 && packet.Power > 0f && packet.Speed > 1f && packet.Clutch == 0 && packet.Brake == 0 && (packet.TimeStampMS - gearChangeTimeStamp) > 800)
		{
			splineUpdated = engineSpline.InsertPoint(packet.CurrentEngineRpm, [packet.Power, packet.Torque]);
		}
		else
		{
			splineUpdated = false;
		}
	}

	static public void ProcessPackets()
	{
		if (!_udpBound)
		{
			return;
		}

		while (forzaUDPStream.GetAvailablePacketCount() > 0)
		{
			AssignData();
			if (splineUpdated)
			{
				for (int i = 0; i < 64; i++)
				{
					float RPM = Mathf.Remap(i, 0, 63, engineSpline.minBounds, engineSpline.maxBounds);

					float[] values = engineSpline.Sample(RPM);

					_engineCurveNormalised[i] = (RPM / _gaugeMaxRPM, values[0] * Enums.UnitConversion.Power[(int)_powerUnit], values[1] * Enums.UnitConversion.Torque[(int)_torqueUnit]);
				}

				(_highestPower, _highestPowerRPM) = FindMaxPower();
				(_highestTorque, _highestTorqueRPM) = FindMaxTorque();

				EnginePeakValuesChange();
				EngineCurveChange();
			}

			if ((gearRatioChanged || gearChanged || splineUpdated) && _gear != 11)
			{
				_shiftDownRPM = OptimalShiftDown(_gear);
				_shiftUpRPM = OptimalShiftUp(_gear);

				PowerbandChange();
			}

			int shiftDirection = 0;

			if (_gear != 0 && _gear != 11)
			{
				if ((_highestRPM - _currentRPM < 200) && _gear != 10 && gearRatios[_gear] != 0f)
				{
					shiftDirection = 1;
				}
				else if (_currentRPM > _shiftUpRPM && _gear != 10)
				{
					shiftDirection = 1;
				}
				else if (_currentRPM < _shiftDownRPM && _gear != 1)
				{
					shiftDirection = -1;
				}
			}

			if (_shiftDirection != shiftDirection)
			{
				_shiftDirection = shiftDirection;
				ShiftValueChange();
			}

			splineUpdated = false;
			gearRatioChanged = false;

			CurrentPowerChange();
		}

	}

	static public event Action ShiftValueChange;
	static public event Action PowerbandChange;
	static public event Action EngineCurveChange;
	static public event Action EnginePeakValuesChange;
	static public event Action CurrentPowerChange;
	static public event Action MenuChange;

	static private (float, float) FindMaxPower()
	{
		float a = MathF.Max(_idleRPM, engineSpline.minBounds);
		float b = MathF.Min(MathF.Max(_highestRPM, _idleRPM), engineSpline.maxBounds);

		float PowerFunction(float RPM) => engineSpline.Sample(RPM)[0];

		float RPM = Optimise(a, b, PowerFunction, 40);

		return (PowerFunction(RPM), RPM);
	}

	static private (float, float) FindMaxTorque()
	{
		float a = MathF.Max(_idleRPM, engineSpline.minBounds);
		float b = MathF.Min(MathF.Max(_highestRPM, _idleRPM), engineSpline.maxBounds);

		float TorqueFunction(float RPM) => engineSpline.Sample(RPM)[1];

		float RPM = Optimise(a, b, TorqueFunction, 40);

		return (TorqueFunction(RPM), RPM);
	}

	static private float OptimalShiftDown(int currentGear)
	{
		if (currentGear == 11 || currentGear == 0 || currentGear == 1) return 0f;
		float dropFactor = gearRatios[currentGear - 1] / gearRatios[currentGear - 2];

		float PowerFunction(float RPM) // simpsons rule
		{
			const int pointCount = 7;
			const int n = pointCount - 1;

			float total = 0f;

			for (int i = 0; i < pointCount; i++)
			{
				float sample = engineSpline.Sample(RPM * Mathf.Remap(i, 0, n, dropFactor, 1f))[0];
				float weight = (i == 0 || i == n) ? 1f : ((i % 2 != 0) ? 4f : 2f);

				total += sample * weight;
			}

			return total;
		}

		float a = MathF.Min(MathF.Max(_idleRPM, engineSpline.minBounds) * 1.05f, engineSpline.maxBounds);
		float b = MathF.Min(MathF.Max(_highestRPM, _idleRPM), engineSpline.maxBounds);

		return Optimise(a, b, PowerFunction, 30) * dropFactor;
	}

	static private float OptimalShiftUp(int currentGear)
	{
		if (currentGear == 11 || currentGear == 0 || currentGear == 10 || gearRatios[currentGear] == 0) { _upperPowerbandCertain = false; return _highestRPM; }
		_upperPowerbandCertain = true;

		float dropFactor = gearRatios[currentGear] / gearRatios[currentGear - 1];

		float PowerFunction(float RPM) // simpsons rule
		{
			int pointCount = 7;
			int n = pointCount - 1;

			float total = 0f;

			for (int i = 0; i < pointCount; i++)
			{
				float sample = engineSpline.Sample(RPM * Mathf.Remap(i, 0, n, dropFactor, 1f))[0];
				float weight = (i == 0 || i == n) ? 1f : ((i % 2 != 0) ? 4f : 2f);

				total += sample * weight;
			}

			return total;
		}

		float a = MathF.Min(MathF.Max(_idleRPM, engineSpline.minBounds) * 1.05f, engineSpline.maxBounds);
		float b = MathF.Min(MathF.Max(_highestRPM, _idleRPM), engineSpline.maxBounds);

		return Optimise(a, b, PowerFunction, 30);
	}

	static private float Optimise(float lowerBound, float upperBound, Func<float,float> function, int iterations)
	{
		const float constant = 0.61803398875f;

		float x1 = upperBound - constant * (upperBound - lowerBound);
		float x2 = lowerBound + constant * (upperBound - lowerBound);

		float f1 = function(x1);
		float f2 = function(x2);

		for (int i = 0; i < iterations; i++)
		{
			if (f1 > f2)
			{
				upperBound = x2;
				x2 = x1;
				f2 = f1;
				x1 = upperBound - constant * (upperBound - lowerBound);
				f1 = function(x1);
			}
			else
			{
				lowerBound = x1;
				x1 = x2;
				f1 = f2;
				x2 = lowerBound + constant * (upperBound - lowerBound);
				f2 = function(x2);
			}
		}

		return (f1 > f2) ? x1 : x2;
	}
}
