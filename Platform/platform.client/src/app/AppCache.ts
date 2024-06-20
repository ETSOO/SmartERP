/**
 * App cache namespace
 */
export namespace AppCache {
  export const MemberCache = 'search-member-cache';
  export const OrgCache = 'search-organization-cache';
  export const ServiceCache = 'search-service-cache';
  export const MyServiceCache = 'search-my-service-cache';

  export function switchOrg() {
    removeMemberCache();
    removeOrgCache();
    removeServiceCache();
    removeMyServiceCache();
  }

  export function removeMemberCache() {
    sessionStorage.removeItem(AppCache.MemberCache);
  }

  export function removeOrgCache() {
    sessionStorage.removeItem(AppCache.OrgCache);
  }

  export function removeServiceCache() {
    sessionStorage.removeItem(AppCache.ServiceCache);
  }

  export function removeMyServiceCache() {
    sessionStorage.removeItem(AppCache.MyServiceCache);
  }
}
