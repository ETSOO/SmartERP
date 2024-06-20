import { IApiPayload, OrgApi as OrgApiBase } from '@etsoo/appscript';
import { OrgViewSet } from './dto/org/OrgViewSet';

/**
 * Organization API
 */
export class OrgApi extends OrgApiBase {
  /**
   * Upload avatar
   * @param id Organization id
   * @param data Avatar form data
   * @param payload Payload
   * @returns Result
   */
  uploadAvatar(id: number, data: FormData, payload?: IApiPayload<string>) {
    return this.api.put(`${this.flag}/UploadAvatar/${id}`, data, payload);
  }

  /**
   * Read
   * @param id Id
   * @param payload Payload
   * @returns Result
   */
  readUI(id: number, payload?: IApiPayload<OrgViewSet>) {
    return this.api.get(`${this.flag}/ReadUI/${id}`, undefined, payload);
  }
}
