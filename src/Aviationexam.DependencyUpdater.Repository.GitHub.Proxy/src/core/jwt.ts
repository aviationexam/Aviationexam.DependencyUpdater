import type { GitHubAppConfig } from "../types.ts";

const JWT_EXPIRATION_SECONDS = 600; // 10 minutes (GitHub max)

export async function generateJwt(config: GitHubAppConfig): Promise<string> {
  const now = Math.floor(Date.now() / 1000);
  const payload = {
    iat: now - 60, // Issued 60 seconds ago to account for clock drift
    exp: now + JWT_EXPIRATION_SECONDS,
    iss: config.appId,
  };

  const privateKey = await importPrivateKey(config.privateKey);
  return await signJwt(payload, privateKey);
}

async function importPrivateKey(pem: string): Promise<CryptoKey> {
  const pemContents = pem
    .replace(/-----BEGIN RSA PRIVATE KEY-----/, "")
    .replace(/-----END RSA PRIVATE KEY-----/, "")
    .replace(/-----BEGIN PRIVATE KEY-----/, "")
    .replace(/-----END PRIVATE KEY-----/, "")
    .replace(/\s/g, "");

  const binaryDer = Uint8Array.from(atob(pemContents), (c) => c.charCodeAt(0));
  const keyBuffer = binaryDer.buffer as ArrayBuffer;

  try {
    return await crypto.subtle.importKey(
      "pkcs8",
      keyBuffer,
      { name: "RSASSA-PKCS1-v1_5", hash: "SHA-256" },
      false,
      ["sign"]
    );
  } catch {
    const pkcs1Key = convertPkcs1ToPkcs8(binaryDer);
    const pkcs8Buffer = pkcs1Key.buffer as ArrayBuffer;
    return await crypto.subtle.importKey(
      "pkcs8",
      pkcs8Buffer,
      { name: "RSASSA-PKCS1-v1_5", hash: "SHA-256" },
      false,
      ["sign"]
    );
  }
}

function convertPkcs1ToPkcs8(pkcs1: Uint8Array): Uint8Array {
  const pkcs8Header = new Uint8Array([
    0x30, 0x82, 0x00, 0x00, 0x02, 0x01, 0x00, 0x30, 0x0d, 0x06, 0x09, 0x2a,
    0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01, 0x05, 0x00, 0x04, 0x82,
    0x00, 0x00,
  ]);

  const pkcs1Length = pkcs1.length;
  const totalLength = pkcs8Header.length + pkcs1Length;

  pkcs8Header[2] = ((totalLength - 4) >> 8) & 0xff;
  pkcs8Header[3] = (totalLength - 4) & 0xff;
  pkcs8Header[24] = (pkcs1Length >> 8) & 0xff;
  pkcs8Header[25] = pkcs1Length & 0xff;

  const result = new Uint8Array(totalLength);
  result.set(pkcs8Header, 0);
  result.set(pkcs1, pkcs8Header.length);

  return result;
}

async function signJwt(
  payload: Record<string, unknown>,
  privateKey: CryptoKey
): Promise<string> {
  const header = { alg: "RS256", typ: "JWT" };

  const encodedHeader = base64UrlEncode(JSON.stringify(header));
  const encodedPayload = base64UrlEncode(JSON.stringify(payload));
  const signingInput = `${encodedHeader}.${encodedPayload}`;

  const signature = await crypto.subtle.sign(
    "RSASSA-PKCS1-v1_5",
    privateKey,
    new TextEncoder().encode(signingInput)
  );

  const encodedSignature = base64UrlEncode(signature);
  return `${signingInput}.${encodedSignature}`;
}

function base64UrlEncode(input: string | ArrayBuffer): string {
  let base64: string;

  if (typeof input === "string") {
    base64 = btoa(input);
  } else {
    const bytes = new Uint8Array(input);
    let binary = "";
    for (const byte of bytes) {
      binary += String.fromCharCode(byte);
    }
    base64 = btoa(binary);
  }

  return base64.replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
}

export function decodeJwtPayload(jwt: string): Record<string, unknown> {
  const parts = jwt.split(".");
  if (parts.length !== 3) {
    throw new Error("Invalid JWT format");
  }
  const payload = parts[1];
  if (!payload) {
    throw new Error("Invalid JWT: missing payload");
  }
  const decoded = atob(payload.replace(/-/g, "+").replace(/_/g, "/"));
  return JSON.parse(decoded) as Record<string, unknown>;
}
