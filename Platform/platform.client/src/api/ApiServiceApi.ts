import { EntityApi, IApiPayload, IdResultPayload } from '@etsoo/appscript';
import { ReactAppType } from '@etsoo/materialui';
import { DataTypes } from '@etsoo/shared';
import { ApiServiceDto } from './dto/apiservice/ApiServiceEditDto';

/**
 * Api service API
 */
export class ApiServiceApi extends EntityApi {
  /**
   * Constructor
   * @param app Application
   */
  constructor(app: ReactAppType) {
    super('ApiService', app);
  }

  /**
   * Create
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  create(
    rq: DataTypes.AddOrEditType<ApiServiceDto, false>,
    payload?: IdResultPayload
  ) {
    return this.createBase(rq, payload);
  }

  /**
   * Update
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  update(
    rq: DataTypes.AddOrEditType<ApiServiceDto, true>,
    payload?: IdResultPayload
  ) {
    return this.updateBase(rq, payload);
  }

  /**
   * Read for update
   * @param id Id
   * @param payload Payload
   * @returns Result
   */
  updateRead(id: number, payload?: IApiPayload<ApiServiceDto>) {
    return this.updateReadBase(id, payload);
  }
}
