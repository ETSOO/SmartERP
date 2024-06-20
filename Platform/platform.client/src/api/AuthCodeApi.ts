import {
  BaseApi,
  ResultPayload,
  StringIdResultPayload
} from '@etsoo/appscript';
import { SendEmailRQ } from './rq/authcode/SendEmailRQ';
import { SendSMSRQ } from './rq/authcode/SendSMSRQ';
import { ValidateRQ } from './rq/authcode/ValidateRQ';

/**
 * Authentication code API
 */
export class AuthCodeApi extends BaseApi {
  /**
   * Send email
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  sendEmail(rq: SendEmailRQ, payload?: StringIdResultPayload) {
    return this.api.put('AuthCode/SendEmail', rq, payload);
  }

  /**
   * Send email
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  sendSMS(rq: SendSMSRQ, payload?: StringIdResultPayload) {
    return this.api.put('AuthCode/SendSMS', rq, payload);
  }

  /**
   * Validate code
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  validate(rq: ValidateRQ, payload?: ResultPayload) {
    return this.api.put('AuthCode/Validate', rq, payload);
  }
}
