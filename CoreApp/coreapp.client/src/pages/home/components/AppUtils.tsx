import { AppQueryData, OrgQueryDto } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import { DomUtils, IActionResult } from "@etsoo/shared";
import { NavigateFunction } from "react-router-dom";
import { BuyApp, BuyKind } from "./BuyApp";
import { VBox } from "@etsoo/materialui";
import { Button, InputAdornment, TextField } from "@mui/material";
import React from "react";
import { InviteMember } from "./InviteMember";

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
          organizationId: "number",
          name: "string",
          pin: "string"
        });

        let result: IActionResult | undefined;

        if (formData.kind === 1) {
          // Check organization
          if (formData.organizationId == null) {
            DomUtils.setFocus("organizationIdInput", form);
            return false;
          }

          result = await app.core.appApi.buy(
            { id: data.id, organizationId: formData.organizationId },
            {
              showLoading: false
            }
          );
        } else {
          // Check name
          if (formData.name == null) {
            DomUtils.setFocus("name", form);
            return false;
          }

          result = await app.core.appApi.buyNew(
            {
              id: data.id,
              orgName: formData.name,
              orgPin: formData.pin,
              region: app.region
            },
            { showLoading: false }
          );
        }

        if (result == null) return false;

        if (result.ok) {
          // Refresh token in silence
          // app.refreshToken({ showLoading: false });

          // Succeed
          app.notifier.succeed(labels.operationSucceeded, undefined, () => {
            // New organization created
            const url = formData.kind === 1 ? "./../my" : "./../../org/my";
            navigate(url);
          });

          return;
        } else if (result.type === "ItemExists") {
          result.title = app.get("purchaseExists");
        }

        app.alertResult(result);

        return false;
      },
      inputs: <BuyApp kind={kind} />,
      fullScreen: app.smDown
    });
  }

  export function renewApp(
    data: { id: number; name: string },
    callback: () => void
  ) {
    // Labels
    const labels = app.getLabels("monthsUnit", "renew", "renewLength", "qty");
    const years = app.get<string[]>("years5");
    const inputRef = React.createRef<HTMLInputElement>();
    const setMonths = (index: number) => {
      if (inputRef.current == null) return;
      inputRef.current.value = `${(index + 1) * 12}`;
    };

    app.showInputDialog({
      title: labels.renew,
      message: data.name,
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Form data
        const { months } = DomUtils.dataAs(new FormData(form), {
          months: "number"
        });
        if (months == null || months < 1) {
          DomUtils.setFocus("months", form);
          return false;
        }

        const result = await app.core.appApi.renew(
          { id: data.id, months },
          { showLoading: false }
        );

        if (result == null) return;

        if (result.ok) {
          callback();
          return;
        }

        app.alertResult(result);
        return false;
      },
      inputs: (
        <VBox gap={1} width="100%" paddingTop={1}>
          <TextField
            name="months"
            margin="dense"
            variant="standard"
            label={labels.renewLength}
            defaultValue={12}
            required
            type="number"
            inputRef={inputRef}
            helperText={years?.map((y, index) => (
              <Button key={index} onClick={() => setMonths(index)}>
                {y}
              </Button>
            ))}
            slotProps={{
              input: {
                endAdornment: (
                  <InputAdornment position="end">
                    {labels.monthsUnit}
                  </InputAdornment>
                )
              },
              htmlInput: { max: 120, min: 1, step: 1, inputMode: "numeric" }
            }}
          />
        </VBox>
      ),
      fullScreen: app.smDown
    });
  }

  export function inviteMember(callback: () => void) {
    // Labels
    const labels = app.getLabels("inviteMember", "inviteResult");

    // Show input dialog
    app.showInputDialog({
      title: labels.inviteMember,
      message: "",
      fullScreen: app.smDown,
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Validate form
        if (!form.reportValidity()) {
          return false;
        }

        // Form data
        const { userRole, emails, message } = DomUtils.dataAs(
          new FormData(form),
          {
            userRole: "number",
            emails: "string[]",
            message: "string"
          }
        );

        if (userRole == null) {
          // No role selected
          DomUtils.setFocus("userRole", form);
          return false;
        }

        if (emails == null || emails.length === 0) {
          // No valid email provided
          DomUtils.setFocus("emails", form);
          return false;
        }

        // Submit
        const result = await app.core.memberApi.invite(
          {
            userRole,
            emails: emails.toUnique(),
            message
          },
          { showLoading: false }
        );
        if (result == null) return;

        if (result.ok) {
          app.notifier.succeed(
            labels.inviteResult.format(result.data?.msg ?? ""),
            undefined,
            () => callback()
          );
          return;
        }

        app.alertResult(result);
        return false;
      },
      inputs: <InviteMember />
    });
  }

  /**
   * Switch organization
   * @param data Data
   */
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
