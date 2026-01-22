export function decodeBase64Key(base64Key: string): string {
  return atob(base64Key);
}
