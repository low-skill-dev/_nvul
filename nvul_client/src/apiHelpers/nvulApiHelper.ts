import axios from "axios";
import { urlJoin } from 'url-join-ts';
import apiPaths from "../configuration/apiPaths.json"
import CompilationRequest from "../models/CompilationRequest";
import CompilationResponse from "../models/CompilationResponse";

export class NvulApiHelper {
    private static getHostUrl = () =>
        window.location.protocol + '//' + window.location.host;

    private static getCompilerUrl = () =>
        urlJoin(
            this.getHostUrl(),
            apiPaths.apiBasePath,
            apiPaths.compilerPath);

    private static getDefaultConfigurationUrl = () =>
        urlJoin(
            this.getCompilerUrl(),
            apiPaths.compilerDefaultConfigurationPath);

    private static getRussianConfigurationUrl = () =>
        urlJoin(
            this.getCompilerUrl(),
            apiPaths.compilerRussianConfigurationPath);

    public static GetDefaultConfiguration = async () =>
        await axios.get<string>(this.getDefaultConfigurationUrl());

    public static GetRussianConfiguration = async () =>
        await axios.get<string>(this.getRussianConfigurationUrl());

    public static RequestCodeCompilation = async (request: CompilationRequest) =>
        await axios.post<CompilationResponse>(this.getCompilerUrl(), request);
}