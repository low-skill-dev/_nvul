export default class CompilationRequest {
    nvulConfiguration: string | null;
    parsingResultRequired: boolean | null;
    nvulCode: string;

    constructor(nvulCode: string, nvulConfiguration: string | null, parsingResultRequired: boolean | null) {
        this.nvulCode = nvulCode;
        this.nvulConfiguration = nvulConfiguration;
        this.parsingResultRequired = parsingResultRequired;
    }
}