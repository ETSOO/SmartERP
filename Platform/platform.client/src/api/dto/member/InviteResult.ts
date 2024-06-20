import { IActionResult } from '@etsoo/appscript';

/**
 * Invite member result
 */
export type InviteResult = IActionResult<{ emails: string[] }>;
