using nvul_compiler.Models.Configuration;
using System.ComponentModel.DataAnnotations;

namespace nvul_server.Models
{
	public class CompilationRequest
	{
		public NvulConfiguration? NvulConfiguration { get; set; }
		public bool? ParsingResultRequired { get; set; }
		
		[Required]
		public string NvulCode { get; set; }

		public CompilationRequest(string nvulCode, NvulConfiguration? nvulConfiguration)
		{
			this.NvulConfiguration = nvulConfiguration;
			this.NvulCode = nvulCode;
		}
	}
}
