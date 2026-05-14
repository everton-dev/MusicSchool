export type SupportedLocale = 'en-US' | 'pt-PT' | 'pt-BR' | 'es-ES';

export interface LanguageOption {
  readonly locale: SupportedLocale;
  readonly code: 'US' | 'PT' | 'BR' | 'ES';
  readonly label: string;
}
