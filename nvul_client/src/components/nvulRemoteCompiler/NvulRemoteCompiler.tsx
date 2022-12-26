import { useEffect, useState } from "react";
import Collapsible from "react-collapsible";
import { NvulApiHelper } from "../../apiHelpers/nvulApiHelper";
import CompilationRequest from "../../models/CompilationRequest";
import styles from "./NvulRemoteCompiler.module.css";

export const NvulRemoteCompiler: React.FC = ({ ...props }) => {
    const [configuration, setConfiguration] = useState<string>("");
    const [codeInput, setCodeInput] = useState<string>("");
    const [errorMessage, setErrorMessage] = useState<string>("Error message will be here!");
    const [parsingResult, setParsingResult] = useState<string>("");
    const [compilationResult, setCompilationResult] = useState<string>("");
    const [isNetworkLocked, setIsNetworkLocked] = useState<boolean>(false);

    useEffect(() => {
        try {
            loadConfig("en");
        } catch { }
        console.log("Compiler loaded.");
    }, []);

    const loadConfig = async (lang: string) => {
        console.log("Requesting configuration...")
        setIsNetworkLocked(true);
        setTimeout(() => { setIsNetworkLocked(false); }, 5000);

        try {
            if (lang.toLocaleLowerCase() == "ru")
                var response = await NvulApiHelper.GetRussianConfiguration();
            else
                var response = await NvulApiHelper.GetDefaultConfiguration();

            if (response.status != 200)
                setConfiguration(`Error: HTTP ${response.status}`);
            else
                setConfiguration(JSON.stringify(response.data, null, 1));

            console.log("Configuration received from server.")
        } catch { }

        setIsNetworkLocked(false);
    }

    const requestCompilation = async () => {
        console.log("Requesting compilation...");
        setIsNetworkLocked(true);
        setTimeout(() => { setIsNetworkLocked(false); }, 5000);

        try {
            const request = new CompilationRequest(
                codeInput,
                configuration,
                true);

            const response = await NvulApiHelper.RequestCodeCompilation(request);

            if (response.status != 200) {
                setParsingResult(`Error: HTTP ${response.status}`);
                setCompilationResult(`Error: HTTP ${response.status}`);
                setErrorMessage(`Network error: HTTP ${response.status}`);
            } else {
                const result = response.data;
                try {
                    setParsingResult(JSON.stringify(JSON.parse(result.parsingResult ?? ""), null, 2));
                } catch (error) {
                    console.log(`Unable to pretty print parsing result: ${error}`);
                    try{
                        setParsingResult(result.parsingResult ?? "");
                    } catch { }
                }
                setCompilationResult(result.compilationResult ?? "");
                setErrorMessage(result.errorMessage ?? "");
            }

            console.log("Compilation result received from server.");
        }
        catch { }

        setIsNetworkLocked(false);
    }

    return (
        <span>
            <Collapsible trigger="Configuration" className={styles.collapsibleBlock} openedClassName={styles.collapsibleBlock} transitionTime={100}>
                <div className={styles.buttonsMenu}>
                    <button className={styles.actionButton} onClick={() => loadConfig("en")} disabled={isNetworkLocked}>Load default</button>
                    <button className={styles.actionButton} onClick={() => loadConfig("ru")} disabled={isNetworkLocked}>Load russian</button>
                </div>
                <textarea value={configuration} onChange={e => setConfiguration(e.target.value)} rows={15} className={styles.codeTextArea}>

                </textarea>
            </Collapsible>

            <Collapsible trigger="Input code" className={styles.collapsibleBlock} openedClassName={styles.collapsibleBlock} transitionTime={100} open={true}>
                <textarea value={codeInput} onChange={e => setCodeInput(e.target.value)} rows={15} className={styles.codeTextArea}>

                </textarea>
                <button className={styles.actionButton} onClick={() => requestCompilation()} disabled={isNetworkLocked}>Compile!</button>
                <div id="errorMessageDiv" style={{ display: errorMessage ? "block" : "none" }} className={styles.errorMessage}>
                    {errorMessage}
                </div>
            </Collapsible>

            <Collapsible trigger="Parsing result" className={styles.collapsibleBlock} openedClassName={styles.collapsibleBlock} transitionTime={100}>
                <textarea value={parsingResult} onChange={e => setParsingResult(e.target.value)} rows={15} className={styles.codeTextArea}>

                </textarea>
            </Collapsible>

            <Collapsible trigger="Compilation result" className={styles.collapsibleBlock} openedClassName={styles.collapsibleBlock} transitionTime={100} open={true}>
                <textarea value={compilationResult} onChange={e => setCompilationResult(e.target.value)} rows={15} className={styles.codeTextArea}>

                </textarea>
            </Collapsible>
        </span>
    )
}