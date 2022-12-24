namespace nvul_server.Models
{
	public class CompilationResponse
	{
		public string? CompilationResult { get; set; }
		public string? ParsingResult { get; set; }
		public string? ErrorMessage { get; set; }
		
		public CompilationResponse(string? CompilationResult, string? parsingResult, string? ErrorMessage)
		{
			this.CompilationResult = CompilationResult;
			this.ParsingResult = parsingResult;
			this.ErrorMessage = ErrorMessage;
		}
	}
}
