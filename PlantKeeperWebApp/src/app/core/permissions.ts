/**
 * The permission strings the API authorizes against, mirrored from
 * `PlantKeeperAPI/src/Authorization/Permissions.cs`.
 *
 * These hide controls the current keeper cannot use. They are a courtesy, not a control:
 * every one of them is enforced again on the server, and the server is the authority.
 */
export const Permission = {
  plantsRead: 'plants.read',
  plantsWrite: 'plants.write',
  almanacRead: 'almanac.read',
  almanacPropose: 'almanac.propose',
  almanacApprove: 'almanac.approve',
  keepersManage: 'keepers.manage',
  rolesManage: 'roles.manage',
} as const;

export type PermissionName = (typeof Permission)[keyof typeof Permission];
