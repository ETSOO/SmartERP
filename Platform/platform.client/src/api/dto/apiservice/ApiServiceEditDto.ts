import { ApiService } from '@etsoo/appscript';

export type ApiServiceDto = {
  id: number;
  organizationId: number;
  serviceId?: ApiService;
  title: string;
  appId: string;
  appSecret: string;
  settings?: Record<string, any>;
  enabled: boolean;
};
