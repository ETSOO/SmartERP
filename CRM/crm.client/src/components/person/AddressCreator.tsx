import { HBox } from "@etsoo/materialui";
import {
  AddressTiplist,
  AddressTiplistProps
} from "@etsoo/smarterp-core/components";

export type AddressCreatorProps = AddressTiplistProps & {};

export function AddressCreator(props: AddressCreatorProps) {
  // Destruct
  const { sx, ...rest } = props;

  return (
    <HBox>
      <AddressTiplist {...rest} sx={{ flex: 2, ...sx }} />
    </HBox>
  );
}
