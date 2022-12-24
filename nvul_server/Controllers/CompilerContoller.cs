using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvul_compiler.Models.Configuration;
using nvul_server.Models;
using nvul_server.Services;
using System.ComponentModel.DataAnnotations;

namespace nvul_server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
[RequestSizeLimit(1 * 1024 * 1024 * 20)]
[AllowAnonymous]
public class CompilerContoller : ControllerBase
{
	private readonly ILogger<CompilerContoller>? _logger;
	private readonly NvulConfigurationProvider _nvulConfigurationProvider;
	public CompilerContoller(
		NvulConfigurationProvider nvulConfigurationProvider,
		ILogger<CompilerContoller>? logger)
	{
		_nvulConfigurationProvider = nvulConfigurationProvider;
		_logger = logger;
	}

	[HttpGet]
	[Route("default-configuration")]
	public IActionResult GetDefaultConfiguration()
	{
		return Ok(_nvulConfigurationProvider.DefaultConfiguration);
	}

	[HttpGet]
	[Route("russian-configuration")]
	public IActionResult GetRussianConfiguration()
	{
		return Ok(_nvulConfigurationProvider.RussianConfiguration);
	}

	[HttpPost]
	public async Task<IActionResult> CompileCode([FromBody][Required] CompilationRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.NvulCode))
		{
			return BadRequest("You provided no code for translation.");
		}
		if (request.NvulConfiguration is null)
		{
			request.NvulConfiguration = _nvulConfigurationProvider.DefaultConfiguration;
		}

		string? result = null;
		string? parsed = null;
		string? error = null;
		try
		{
			if (request.ParsingResultRequired ?? false)
				result = await Task.Run(() => nvul_compiler.Services.Compiler.Compile(request.NvulCode, request.NvulConfiguration, out parsed));
			else
				result = await Task.Run(() => nvul_compiler.Services.Compiler.Compile(request.NvulCode, request.NvulConfiguration));
		}
		catch (Exception ex)
		{
			error = ex.Message;
		}

		return Ok(new CompilationResponse(result, parsed, error));
	}
}
