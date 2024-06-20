import { DataTypes } from '@etsoo/shared';
import { ApiServiceDto } from '../../../api/dto/apiservice/ApiServiceEditDto';

export type ApiServiceProps = {
  data: DataTypes.AddAndEditType<ApiServiceDto, 'id'>;
  handleChange: (e: React.ChangeEvent<any>) => void;
};
