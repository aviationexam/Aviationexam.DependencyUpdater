import { describe, expect, it } from "bun:test";
import { readFileSync } from "fs";
import { join } from "path";
import { generateJwt } from "../../src/core/jwt.ts";

function decodeJwtPayloadWithoutVerification(jwt: string): Record<string, unknown> {
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

const TEST_KEY_PATH = join(import.meta.dir, "../fixtures/test-key.pem");
const TEST_PRIVATE_KEY = readFileSync(TEST_KEY_PATH, "utf-8");
const TEST_APP_ID = "12345";

describe("JWT Generation", () => {
  it("generates a valid JWT with correct structure", async () => {
    const jwt = await generateJwt({
      appId: TEST_APP_ID,
      privateKey: TEST_PRIVATE_KEY,
    });

    const parts = jwt.split(".");
    expect(parts.length).toBe(3);
  });

  it("includes correct header with RS256 algorithm", async () => {
    const jwt = await generateJwt({
      appId: TEST_APP_ID,
      privateKey: TEST_PRIVATE_KEY,
    });

    const [headerPart] = jwt.split(".");
    const header = JSON.parse(
      atob(headerPart!.replace(/-/g, "+").replace(/_/g, "/"))
    );

    expect(header.alg).toBe("RS256");
    expect(header.typ).toBe("JWT");
  });

  it("includes correct payload with iss, iat, and exp claims", async () => {
    const beforeGeneration = Math.floor(Date.now() / 1000);

    const jwt = await generateJwt({
      appId: TEST_APP_ID,
      privateKey: TEST_PRIVATE_KEY,
    });

    const afterGeneration = Math.floor(Date.now() / 1000);
    const payload = decodeJwtPayloadWithoutVerification(jwt);

    expect(payload["iss"]).toBe(TEST_APP_ID);
    expect(typeof payload["iat"]).toBe("number");
    expect(typeof payload["exp"]).toBe("number");

    const iat = payload["iat"] as number;
    const exp = payload["exp"] as number;

    expect(iat).toBeGreaterThanOrEqual(beforeGeneration - 61);
    expect(iat).toBeLessThanOrEqual(afterGeneration - 59);

    expect(exp - iat).toBe(660);
  });

  it("generates different JWTs for different app IDs", async () => {
    const jwt1 = await generateJwt({
      appId: "11111",
      privateKey: TEST_PRIVATE_KEY,
    });

    const jwt2 = await generateJwt({
      appId: "22222",
      privateKey: TEST_PRIVATE_KEY,
    });

    expect(jwt1).not.toBe(jwt2);

    const payload1 = decodeJwtPayloadWithoutVerification(jwt1);
    const payload2 = decodeJwtPayloadWithoutVerification(jwt2);

    expect(payload1["iss"]).toBe("11111");
    expect(payload2["iss"]).toBe("22222");
  });

  it("throws error for invalid private key", async () => {
    await expect(
      generateJwt({
        appId: TEST_APP_ID,
        privateKey: "invalid-key-content",
      })
    ).rejects.toThrow();
  });
});

describe("decodeJwtPayloadWithoutVerification", () => {
  it("decodes valid JWT payload", () => {
    const testPayload = { iss: "12345", iat: 1234567890, exp: 1234568490 };
    const encodedPayload = btoa(JSON.stringify(testPayload))
      .replace(/\+/g, "-")
      .replace(/\//g, "_")
      .replace(/=+$/, "");

    const fakeJwt = `header.${encodedPayload}.signature`;
    const decoded = decodeJwtPayloadWithoutVerification(fakeJwt);

    expect(decoded["iss"]).toBe("12345");
    expect(decoded["iat"]).toBe(1234567890);
    expect(decoded["exp"]).toBe(1234568490);
  });

  it("throws error for invalid JWT format", () => {
    expect(() => decodeJwtPayloadWithoutVerification("invalid")).toThrow("Invalid JWT format");
    expect(() => decodeJwtPayloadWithoutVerification("only.two")).toThrow("Invalid JWT format");
  });
});
