namespace Enums;

public enum PowerUnitType
{
	ImperialHP = 0,
	Kilowatts = 1,
	MetricHP = 2
}

public enum TorqueUnitType
{
	NewtonMeters = 0,
	FootPounds = 1
}

public enum PowerCurveOverlayType
{
	Disabled = 0,
	Simple = 1,
	Full = 2
}


static public class UnitConversion
{
	public static readonly float[] Power = [0.00134102f, 0.001f, 0.00135962f];
	public static readonly float[] Torque = [1.0f, 0.7375621493f];
}

static public class UnitNames
{
	public static readonly string[] Power = ["hp", "KW", "PS"];
	public static readonly string[] Torque = ["Nm", "ft-lb"];
}