/// <reference types="vite/client" />

// TypeScript Augmentation
interface ImportMetaEnv {
  /**
   * APP version
   */
  readonly VITE_APP_VERSION: string;

  /**
   * APP unsafe inlines for CSP
   */
  readonly VITE_APP_UNSAFE_INLINES: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
