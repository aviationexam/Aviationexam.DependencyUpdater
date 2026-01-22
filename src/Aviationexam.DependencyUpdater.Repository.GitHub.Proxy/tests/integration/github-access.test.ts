import { describe, expect, it } from "bun:test";
import { validateCallerHasRepoAccess } from "../../src/core/github-client.ts";

const GITHUB_TOKEN = process.env["GITHUB_TOKEN"];

describe.skipIf(!GITHUB_TOKEN)("GitHub Access Validation", () => {
  describe("Positive flow - authorized access", () => {
    it("validates access to aviationexam/Aviationexam.DependencyUpdater", async () => {
      const result = await validateCallerHasRepoAccess(
        GITHUB_TOKEN!,
        "aviationexam",
        "Aviationexam.DependencyUpdater"
      );

      expect(result.success).toBe(true);
      if (result.success) {
        expect(result.data.full_name).toBe(
          "aviationexam/Aviationexam.DependencyUpdater"
        );
        expect(result.data.owner.login).toBe("aviationexam");
      }
    });

    it("validates access to current repository from GITHUB_REPOSITORY env", async () => {
      const repoFullName = process.env["GITHUB_REPOSITORY"];
      if (!repoFullName) {
        console.log("GITHUB_REPOSITORY not set, skipping test");
        return;
      }

      const [owner, repo] = repoFullName.split("/");
      if (!owner || !repo) {
        console.log("Invalid GITHUB_REPOSITORY format, skipping test");
        return;
      }

      const result = await validateCallerHasRepoAccess(
        GITHUB_TOKEN!,
        owner,
        repo
      );

      expect(result.success).toBe(true);
      if (result.success) {
        expect(result.data.full_name.toLowerCase()).toBe(
          repoFullName.toLowerCase()
        );
      }
    });
  });

  describe("Negative flow - unauthorized access", () => {
    it("rejects access to microsoft/vscode (no write access)", async () => {
      const result = await validateCallerHasRepoAccess(
        GITHUB_TOKEN!,
        "microsoft",
        "vscode"
      );

      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.status).toBe(403);
        expect(result.error.message).toContain("write access");
      }
    });

    it("rejects access to non-existent repository", async () => {
      const result = await validateCallerHasRepoAccess(
        GITHUB_TOKEN!,
        "aviationexam",
        "definitely-does-not-exist-12345"
      );

      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.status).toBe(404);
      }
    });

    it("rejects access with invalid token", async () => {
      const result = await validateCallerHasRepoAccess(
        "invalid-token-12345",
        "aviationexam",
        "Aviationexam.DependencyUpdater"
      );

      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.status).toBe(401);
      }
    });
  });
});
