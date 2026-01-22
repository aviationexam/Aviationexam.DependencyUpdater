import { parseArgs } from "util";
import { getInstallationAccessToken } from "./core/token-service.ts";
import { decodeBase64Key } from "./core/utils.ts";

interface CliArgs {
  appId: string;
  appKey: string;
  githubToken: string;
  owner: string;
  repository: string;
}

function parseCliArgs(): CliArgs {
  const { values } = parseArgs({
    args: Bun.argv.slice(2),
    options: {
      "app-id": { type: "string" },
      "app-key": { type: "string" },
      "github-token": { type: "string" },
      owner: { type: "string" },
      repository: { type: "string" },
    },
    strict: true,
    allowPositionals: false,
  });

  const appId = values["app-id"] ?? process.env["GITHUB_APP_ID"];
  const appKey = resolveAppKey(
    values["app-key"],
    process.env["GITHUB_APP_KEY"],
    process.env["GITHUB_APP_KEY_BASE64"]
  );
  const githubToken = values["github-token"] ?? process.env["GITHUB_TOKEN"];
  const owner = values["owner"];
  const repository = values["repository"];

  if (!appId) {
    console.error("Error: --app-id or GITHUB_APP_ID is required");
    process.exit(1);
  }
  if (!appKey) {
    console.error(
      "Error: --app-key, GITHUB_APP_KEY, or GITHUB_APP_KEY_BASE64 is required"
    );
    process.exit(1);
  }
  if (!githubToken) {
    console.error("Error: --github-token or GITHUB_TOKEN is required");
    process.exit(1);
  }
  if (!owner) {
    console.error("Error: --owner is required");
    process.exit(1);
  }
  if (!repository) {
    console.error("Error: --repository is required");
    process.exit(1);
  }

  return { appId, appKey, githubToken, owner, repository };
}

async function main(): Promise<void> {
  const args = parseCliArgs();

  const result = await getInstallationAccessToken(
    { appId: args.appId, privateKey: args.appKey },
    args.githubToken,
    { owner: args.owner, repository: args.repository }
  );

  if (!result.success) {
    console.error(`Error: ${result.error.message}`);
    process.exit(1);
  }

  console.log(result.data.token);
  process.exit(0);
}

function resolveAppKey(
  argValue: string | undefined,
  envKey: string | undefined,
  envKeyBase64: string | undefined
): string | undefined {
  if (argValue) return argValue;
  if (envKey) return envKey;
  if (envKeyBase64) return decodeBase64Key(envKeyBase64);
  return undefined;
}

await main();
