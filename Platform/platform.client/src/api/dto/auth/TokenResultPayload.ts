import { IApiPayload } from "@etsoo/appscript";
import { IActionResult } from "@etsoo/shared";

export type TokenResultPayload = IApiPayload<
  IActionResult<{
    simpleRegistration: boolean;
    token: string;
  }>
>;
