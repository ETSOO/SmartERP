import { UserRole } from "@etsoo/appscript";
import { app } from "./MyApp";

/**
 * My utility functions
 */
export namespace MyUtils {
  const adminOrgs: Record<number, boolean> = {};

  /**
   * Check organization ownership
   * @param id Organization id
   * @returns result
   */
  export async function checkOrg(id: number) {
    const result = await app.core.orgApi.owns({
      id,
      minRole: UserRole.Executive
    });
    if (result == null) return;

    adminOrgs[id] = result;

    return result;
  }

  /**
   * Is admin for the organization
   * @param id Organization id
   * @returns Result
   */
  export function isAdmin(id: number) {
    if (adminOrgs[id] != null) return adminOrgs[id];
    return false;
  }
}
