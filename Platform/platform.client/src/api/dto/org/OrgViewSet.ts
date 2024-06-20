import { OrgViewDto } from '@etsoo/appscript';

export type OrgViewSetData = OrgViewDto & {
  parentName?: string;
};

export type OrgApiViewDto = {
  id: number;
  title: string;
  appId: string;
  enabled: boolean;
  creation: string | Date;
};

export type OrgViewSet = {
  data: OrgViewSetData;
  apis: OrgApiViewDto[];
};
