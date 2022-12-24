import React from 'react';
import { NvulRemoteCompiler } from './components/nvulRemoteCompiler/NvulRemoteCompiler';
import styles from "./App.module.css";

function App() {
  return (
    <div style={{justifyContent:"center",display:"flex"}}>
      <div className={styles.app} style={{ width: "100%", maxWidth: "800px" }}>
        <div className={styles.appHeader}>nVUL</div>
        <div className={styles.appSubheader}>not VERY USEFUL LANGUAGE</div>
        <div className={styles.compilerHolder}>
          <NvulRemoteCompiler />
        </div>
      </div>
    </div>
  );
}

export default App;
