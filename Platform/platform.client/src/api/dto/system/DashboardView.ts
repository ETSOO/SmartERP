import { DeviceDto } from './DeviceDto';
import { SystemOrganizationDto } from './SystemOrganizationDto';
import { SystemServiceDto } from './SystemServiceDto';

/**
 * Dashboard view
 */
export type DashboardView = {
  /**
   * Organization
   */
  organization?: SystemOrganizationDto;

  /**
   * Services
   */
  services: SystemServiceDto[];

  /**
   * Devices
   */
  devices: DeviceDto[];
};
