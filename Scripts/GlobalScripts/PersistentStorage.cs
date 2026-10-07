// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2026 Brainbowbash
using Godot;
using System;
using System.Collections.Generic;

namespace Config;

public partial class PersistentStorage : Node
{
	public interface IConfigProperty
	{
		public string Key { get; }
		public string Section { get; }
		public Variant VariantValue { get; }
	}

	public class ConfigProperty<[MustBeVariantAttribute] T> : IConfigProperty
	{
		private readonly string _section;
		private readonly string _key;
		private T _value;
		private readonly Variant.Type _type;
		private readonly ConfigFile _config;

		private readonly Func<T, bool> _valueCondition;

		public T Value
		{
			get
			{
				_value = GetConfigProperty();
				return _value;
			}
			set
			{
				_value = value;
				SetConfigProperty(true);
			}
		}

		public string Key
		{
			get { return _key; }
		}

		public string Section
		{
			get { return _section; }
		}

		public Variant VariantValue
		{
			get { return Variant.From(Value); }
		}

		public bool CheckValidity()
		{
			try
			{
				if (!_config.HasSectionKey(_section, _key))
				{
					return false;
				}

				T configValue = _config.GetValue(_section, _key).As<T>();

				if (typeof(T).IsEnum && !Enum.IsDefined(typeof(T), configValue))
				{
					return false;
				}

				if (_valueCondition is not null && !_valueCondition(configValue))
				{
					return false;
				}

				return _config.GetValue(_section, _key).VariantType == _type;
			}
			catch
			{
				return false;
			}
		}

		private void SetConfigProperty(bool save)
		{
			_config.SetValue(_section, _key, Variant.From(_value));

			if (save)
			{
				_config.Save(configPath);
			}
		}

		private T GetConfigProperty()
		{
			_value = _config.GetValue(_section, _key).As<T>();
			return _value;
		}

		public ConfigProperty(string section, string key, T defaultValue, ConfigFile config, List<IConfigProperty> propertyList, Func<T, bool> valueCondition)
		{
			_valueCondition = valueCondition;

			_section = section;
			_key = key;
			_value = defaultValue;
			_type = Variant.From(defaultValue).VariantType;
			_config = config;

			if (CheckValidity())
			{
				_value = GetConfigProperty();
			}
			else
			{
				SetConfigProperty(false);
			}
			propertyList.Add(this);
		}

		public ConfigProperty(string section, string key, T defaultValue, ConfigFile config, List<IConfigProperty> propertyList) : this(section, key, defaultValue, config, propertyList, null)
		{
		}
	}


	public static List<IConfigProperty> ConfigProperties;

	ConfigFile configuration;

	public static ConfigProperty<bool> DisplayCentralShiftIndicator;
	public static ConfigProperty<bool> DisplayRevGaugeShiftIndicator;
	public static ConfigProperty<bool> DisplayRevGaugePowerbandZone;
	public static ConfigProperty<Enums.PowerCurveOverlayType> DisplayRevGaugeEnginePowerInfo;
	public static ConfigProperty<Enums.PowerUnitType> PowerUnit;
	public static ConfigProperty<Enums.TorqueUnitType> TorqueUnit;

	public static ConfigProperty<string> DataOutIPAddress;
	public static ConfigProperty<int> DataOutIPPort;

	public static ConfigProperty<double> HUDSafeFrameHorizontal;
	public static ConfigProperty<double> HUDSafeFrameVertical;

	private readonly RegEx completeIPAddressRegex = RegEx.CreateFromString(@"^((25[0-5]|2[0-4]\d|1\d\d|[1-9]\d|\d)\.){3}(25[0-5]|2[0-4]\d|1\d\d|[1-9]\d|\d)$");
	private const string configPath = @"user://OverlayConfig.ini";

	public override void _Ready()
	{
		configuration = new ConfigFile();
		ConfigProperties = new List<IConfigProperty>();

		var outcome = configuration.Load(configPath);

		DisplayCentralShiftIndicator = new ConfigProperty<bool>("Overlay Display", "DisplayCentralShiftIndicator", false, configuration, ConfigProperties);
		DisplayRevGaugeShiftIndicator = new ConfigProperty<bool>("Overlay Display", "DisplayRevGaugeShiftIndicator", true, configuration, ConfigProperties);
		DisplayRevGaugePowerbandZone = new ConfigProperty<bool>("Overlay Display", "DisplayRevGaugePowerbandZone", true, configuration, ConfigProperties);

		DisplayRevGaugeEnginePowerInfo = new ConfigProperty<Enums.PowerCurveOverlayType>("Overlay Display", "DisplayRevGaugeEnginePowerInfo", Enums.PowerCurveOverlayType.Disabled, configuration, ConfigProperties);

		PowerUnit = new ConfigProperty<Enums.PowerUnitType>("Overlay Display", "PowerUnit", Enums.PowerUnitType.ImperialHP, configuration, ConfigProperties);
		TorqueUnit = new ConfigProperty<Enums.TorqueUnitType>("Overlay Display", "TorqueUnit", Enums.TorqueUnitType.NewtonMeters, configuration, ConfigProperties);

		DataOutIPAddress = new ConfigProperty<string>("Data Out", "DataOutIPAddress", "127.0.0.1", configuration, ConfigProperties, (string address) => (completeIPAddressRegex.Search(address) is not null));
		DataOutIPPort = new ConfigProperty<int>("Data Out", "DataOutIPPort", 8000, configuration, ConfigProperties, (int port) => (port >= 0 && port < 65536));

		HUDSafeFrameHorizontal = new ConfigProperty<double>("HUD Safe Frame", "HUDSafeFrameHorizontal", 5.2, configuration, ConfigProperties, (double value) => (value >= 0.0 && value <= 70.0));
		HUDSafeFrameVertical = new ConfigProperty<double>("HUD Safe Frame", "HUDSafeFrameVertical", 9.3, configuration, ConfigProperties, (double value) => (value >= 0.0 && value <= 25.0));


		configuration.Save(configPath);
	}
}
