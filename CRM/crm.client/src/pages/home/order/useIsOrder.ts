import { useLocation } from "react-router-dom";

export function useIsOrder() {
  const location = useLocation();
  return location.pathname.includes("/order/");
}
