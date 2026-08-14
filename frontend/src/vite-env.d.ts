/// <reference types="vite/client" />

/** A build-időben behelyettesített env-változók típusai (lásd .env.development). */
interface ImportMetaEnv {
  readonly VITE_API_BASE?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
