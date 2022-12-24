export default interface CompilationResponse {
    compilationResult: string | null;
    parsingResult: string | null;
    errorMessage: string | null;
}