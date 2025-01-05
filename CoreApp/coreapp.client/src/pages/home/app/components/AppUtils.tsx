import { AppQueryData, OrgQueryDto } from "@etsoo/smarterp-core";
import { app } from "../../../../app/MyApp";
import { DomUtils, IActionResult } from "@etsoo/shared";
import { NavigateFunction } from "react-router-dom";
import { BuyApp, BuyKind } from "./BuyApp";

/**
 * App utilities
 */
export namespace AppUtils {
  /**
   * Buy app
   * @param data Data
   */
  export function buyApp(
    data: AppQueryData,
    kind: BuyKind,
    navigate: NavigateFunction
  ) {
    // Labels
    const labels = app.getLabels("buy", "operationSucceeded");

    app.showInputDialog({
      title: labels.buy,
      message: data.name,
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Form data
        const formData = DomUtils.dataAs(new FormData(form), {
          kind: "number",
          organization: "number",
          name: "string",
          pin: "string"
        });

        let result: IActionResult | undefined;

        if (formData.kind === 1) {
          // Check organization
          if (formData.organization == null) {
            DomUtils.setFocus("organizationInput", form);
            return false;
          }

          /*
          result = await app.productApi.buy(data.id, formData.organization, {
            showLoading: false
          });
          */
        } else {
          // Check name
          if (formData.name == null) {
            DomUtils.setFocus("name", form);
            return false;
          }

          /*
          result = await app.productApi.buyNew(
            {
              id: data.id,
              region: app.region,
              name: formData.name,
              identifier: formData.identifier,
              deviceId: app.deviceId
            },
            { showLoading: false }
          );
          */
        }

        if (result == null) return false;

        if (result.ok) {
          // Refresh token in silence
          app.refreshToken({ showLoading: false });

          // Succeed
          app.notifier.succeed(labels.operationSucceeded, undefined, () => {
            // New organization created
            const url =
              formData.kind === 1 ? "./../my" : "./../../organization/all";
            navigate(url);
          });

          return;
        }

        app.alertResult(result);
        return false;
      },
      inputs: <BuyApp kind={kind} />,
      fullScreen: app.smDown
    });
  }

  export function switchOrg(data: OrgQueryDto) {
    // Labels
    const labels = app.getLabels("confirmAction", "switchOrg", "unknownError");

    // Message
    const message =
      labels.confirmAction.format(labels.switchOrg) + ` => ${data.name}`;

    app.notifier.confirm(message, undefined, async (confirmed) => {
      if (confirmed) {
        const result = await app.switchOrg(data.id);
        if (result == null) return;

        if (!result.ok) {
          app.alertResult(result);
          return;
        }
      }
    });
  }
}
