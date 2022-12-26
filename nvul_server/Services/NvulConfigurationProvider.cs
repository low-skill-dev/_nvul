using nvul_compiler.Models.Configuration;
using nvul_server.Controllers;

namespace nvul_server.Services
{
	public sealed class NvulConfigurationProvider
	{
		private IWebHostEnvironment _environment;
		private ILogger<CompilerContoller>? _logger;

		private DateTime _configLastUpdateUtc;
		private DateTime _configVersionUtc;
		private TimeSpan _configLifespan;
		private NvulConfiguration _defaultConfiguration;
		private NvulConfiguration _russianConfiguration;

		public NvulConfiguration DefaultConfiguration
		{
			get
			{
				UpdateConfigIfNeeded();
				return _defaultConfiguration;
			}
		}
		public NvulConfiguration RussianConfiguration
		{
			get
			{
				UpdateConfigIfNeeded();
				return _russianConfiguration;
			}
		}

#pragma warning disable CS8618
		public NvulConfigurationProvider(IWebHostEnvironment environment, ILogger<CompilerContoller>? logger)
		{
			_environment = environment;
			_logger = logger;

			UpdateDefaultConfig();
			UpdateRussianConfig();

			_configLifespan = TimeSpan.FromMinutes(1);
			_configLastUpdateUtc = _configVersionUtc = DateTime.UtcNow;

			_logger?.LogInformation($"{nameof(NvulConfigurationProvider)} service was created.");
		}

		private void UpdateConfigIfNeeded()
		{
			var webPath = this._environment.WebRootPath;

			var paths = new string[]
			{
				Path.Join(webPath, "/DefaultConfiguration/nvulKeywords.json"),
				Path.Join(webPath, "/DefaultConfiguration/nvulOperators.json"),
				Path.Join(webPath, "/DefaultConfiguration/nvulFunctions.json"),
				Path.Join(webPath, "/DefaultConfiguration/nvulImplicits.json"),

				Path.Join(webPath, "/RussianConfiguration/nvulKeywords.json"),
				Path.Join(webPath, "/RussianConfiguration/nvulOperators.json"),
				Path.Join(webPath, "/RussianConfiguration/nvulFunctions.json"),
				Path.Join(webPath, "/RussianConfiguration/nvulImplicits.json")
			};

			if (DateTime.UtcNow - _configLastUpdateUtc > _configLifespan)
			{
				if (paths.Any(x => File.GetLastWriteTimeUtc(x) > _configVersionUtc))
				{
					UpdateDefaultConfig();
					UpdateRussianConfig();
					_configLastUpdateUtc = _configVersionUtc = DateTime.UtcNow;
					_logger?.LogInformation($"Language configuration was updated from files during request.");
				}
			}
		}

		private void UpdateDefaultConfig()
		{
			var webPath = this._environment.WebRootPath;

			var nvulKeywords = Newtonsoft.Json.JsonConvert.DeserializeObject<NvulKeyword[]>(System.IO.File.ReadAllText(
				Path.Join(webPath, "/DefaultConfiguration/nvulKeywords.json")));
			var nvulOperators = Newtonsoft.Json.JsonConvert.DeserializeObject<NvulOperator[]>(System.IO.File.ReadAllText(
				Path.Join(webPath, "/DefaultConfiguration/nvulOperators.json")));
			var nvulFunctions = Newtonsoft.Json.JsonConvert.DeserializeObject<NvulFunction[]>(System.IO.File.ReadAllText(
				Path.Join(webPath, "/DefaultConfiguration/nvulFunctions.json")));
			var nvulImplicits = Newtonsoft.Json.JsonConvert.DeserializeObject<NvulImplicit[]>(System.IO.File.ReadAllText(
				Path.Join(webPath, "/DefaultConfiguration/nvulImplicits.json")));

			this._defaultConfiguration = new NvulConfiguration(nvulFunctions!, nvulImplicits!, nvulKeywords!, nvulOperators!);
		}

		private void UpdateRussianConfig()
		{
			var webPath = this._environment.WebRootPath;

			var nvulKeywords = Newtonsoft.Json.JsonConvert.DeserializeObject<NvulKeyword[]>(System.IO.File.ReadAllText(
				Path.Join(webPath, "/RussianConfiguration/nvulKeywords.json")));
			var nvulOperators = Newtonsoft.Json.JsonConvert.DeserializeObject<NvulOperator[]>(System.IO.File.ReadAllText(
				Path.Join(webPath, "/RussianConfiguration/nvulOperators.json")));
			var nvulFunctions = Newtonsoft.Json.JsonConvert.DeserializeObject<NvulFunction[]>(System.IO.File.ReadAllText(
				Path.Join(webPath, "/RussianConfiguration/nvulFunctions.json")));
			var nvulImplicits = Newtonsoft.Json.JsonConvert.DeserializeObject<NvulImplicit[]>(System.IO.File.ReadAllText(
				Path.Join(webPath, "/RussianConfiguration/nvulImplicits.json")));

			this._russianConfiguration = new NvulConfiguration(nvulFunctions!, nvulImplicits!, nvulKeywords!, nvulOperators!);
		}
	}
}
