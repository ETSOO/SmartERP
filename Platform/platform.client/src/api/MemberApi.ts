import { IApiPayload, IdResultPayload, ResultPayload } from '@etsoo/appscript';
import { DataTypes } from '@etsoo/shared';
import { InviteResult } from './dto/member/InviteResult';
import { MemberDto } from './dto/member/MemberDto';
import { MemberQueryDto } from './dto/member/MemberQueryDto';
import { MyInvitationCodeResult } from './dto/member/MyInvitationCodeResult';
import { InviteByCodeRQ } from './rq/member/InviteByCodeRQ';
import { InviteRQ } from './rq/member/InviteRQ';
import { MemberQueryRQ } from './rq/member/MemberQueryRQ';
import { MyInvitationCodeRQ } from './rq/member/MyInvitationCodeRQ';
import { MemberApi as MemberApiBase } from '@etsoo/appscript';

/**
 * Member API
 */
export class MemberApi extends MemberApiBase {
  /**
   * Accept invitation
   * @param id Id
   * @param localName Local name
   * @param payload Payload
   * @returns Result
   */
  acceptInvitation(id: string, localName: string, payload?: IdResultPayload) {
    return this.api.put(`Member/AcceptInvitation`, { id, localName }, payload);
  }

  /**
   * Delete
   * @param id Id
   * @param payload Payload
   * @returns Result
   */
  delete(id: string, payload?: ResultPayload) {
    return this.deleteBase(id, payload);
  }

  /**
   * Invite
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  invite(rq: InviteRQ, payload?: IApiPayload<InviteResult>) {
    return this.api.post<InviteResult>('Member/Invite', rq, payload);
  }

  /**
   * Invite by code
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  inviteByCode(rq: InviteByCodeRQ, payload?: IApiPayload<InviteResult>) {
    return this.api.post('Member/InviteByCode', rq, payload);
  }

  /**
   * My invitation code
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  myInvitationCode(
    rq: MyInvitationCodeRQ,
    payload?: IApiPayload<MyInvitationCodeResult>
  ) {
    return this.api.post('Member/MyInvitationCode', rq, payload);
  }

  /**
   * Query
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  query(rq: MemberQueryRQ, payload?: IApiPayload<MemberQueryDto[]>) {
    return this.queryBase(rq, payload);
  }

  /**
   * Update
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  update(
    rq: DataTypes.AddOrEditType<MemberDto, true>,
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
  updateRead(id: string, payload?: IApiPayload<MemberDto>) {
    return this.updateReadBase(id, payload);
  }
}
