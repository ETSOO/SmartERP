import { DomUtils } from "@etsoo/shared";
import { HBox, InputField, VBox } from "@etsoo/materialui";
import React from "react";
import { app } from "../app/MyApp";
import { UserTiplist } from "./UserTiplist";
import TextField from "@mui/material/TextField";
import FormControlLabel from "@mui/material/FormControlLabel";
import Checkbox from "@mui/material/Checkbox";
import Button from "@mui/material/Button";
import InputAdornment from "@mui/material/InputAdornment";

function RenewAppUI({ orgId }: { orgId: number }) {
  // Labels
  const labels = app.getLabels(
    "applicant",
    "approver",
    "description",
    "monthsUnit",
    "renewLength",
    "revoke"
  );

  const years = app.get<string[]>("years5");

  const inputRef = React.useRef<HTMLInputElement>(null);
  const revokeRef = React.useRef<HTMLInputElement>(null);

  const setMonths = (index: number) => {
    if (inputRef.current == null) return;
    inputRef.current.value = `${
      (revokeRef.current?.checked ? -1 : 1) * (index + 1) * 12
    }`;
  };

  return (
    <VBox spacing={2} sx={{ width: "100%", paddingTop: 1 }}>
      <HBox spacing={1}>
        <FormControlLabel
          control={
            <Checkbox
              slotProps={{ input: { ref: revokeRef } }}
              onChange={() => {
                setMonths(0);
              }}
            />
          }
          label={labels.revoke}
          sx={{ wordBreak: "keep-all" }}
        />
        <TextField
          name="months"
          margin="dense"
          variant="standard"
          label={labels.renewLength}
          defaultValue={12}
          required
          fullWidth
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
            htmlInput: {
              max: 120,
              min: -120,
              step: 1,
              inputMode: "numeric"
            }
          }}
        />
      </HBox>
      <UserTiplist
        name="requester"
        search={false}
        label={labels.applicant}
        rq={{ orgId, excludeSelf: true }}
      />
      <UserTiplist
        name="approver"
        search={false}
        label={labels.approver}
        rq={{
          orgId: app.userData?.organization,
          excludeSelf: true
        }}
      />
      <InputField
        name="comment"
        label={labels.description}
        multiline
        rows={2}
        slotProps={{ htmlInput: { maxLength: 255 } }}
      />
    </VBox>
  );
}

/**
 * App utilities
 */
export namespace AppUtils {
  export function adminSupport(data: { id: number; name: string }) {
    // Labels
    const labels = app.getLabels(
      "adminSupport",
      "applicant",
      "approver",
      "description"
    );

    const title = `${labels.adminSupport}`;

    app.showInputDialog({
      title,
      message: data.name,
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Form data
        const { requester, approver, comment } = DomUtils.dataAs(
          new FormData(form),
          {
            requester: "number",
            approver: "number",
            comment: "string"
          }
        );

        if (requester == null || requester < 1) {
          DomUtils.setFocus("requesterInput", form);
          return false;
        }

        if (approver == null || approver < 1 || approver === requester) {
          DomUtils.setFocus("approverInput", form);
          return false;
        }

        if (comment == null || comment.length < 1) {
          DomUtils.setFocus("comment", form);
          return false;
        }

        const result = await app.core.authApi.adminSupport(
          { orgId: data.id, requester, approver, comment },
          { showLoading: false }
        );

        if (result == null) return;

        if (result.ok && result.data?.uri) {
          app.loadUrlEx(result.data.uri);
          return;
        }

        app.alertResult(result);
        return false;
      },
      inputs: (
        <VBox spacing={2} sx={{ paddingTop: 2 }}>
          <UserTiplist
            name="requester"
            search={false}
            label={labels.applicant}
            rq={{ orgId: data.id, excludeSelf: true }}
          />
          <UserTiplist
            name="approver"
            search={false}
            label={labels.approver}
            rq={{
              orgId: app.userData?.organization,
              excludeSelf: true
            }}
          />
          <InputField
            name="comment"
            label={labels.description}
            multiline
            rows={2}
            slotProps={{ htmlInput: { maxLength: 255 } }}
          />
        </VBox>
      ),
      fullScreen: app.smDown
    });
  }

  export function renewApp(
    data: { id: number; name: string; orgId: number },
    callback: () => void
  ) {
    // Labels
    const labels = app.getLabels("adminRenew");

    const title = `${labels.adminRenew}`;

    app.showInputDialog({
      title,
      message: data.name,
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Form data
        const { months, requester, approver, comment } = DomUtils.dataAs(
          new FormData(form),
          {
            months: "number",
            requester: "number",
            approver: "number",
            comment: "string"
          }
        );
        if (months == null || months === 0) {
          DomUtils.setFocus("months", form);
          return false;
        }

        if (requester == null || requester < 1) {
          DomUtils.setFocus("requesterInput", form);
          return false;
        }

        if (approver == null || approver < 1 || approver === requester) {
          DomUtils.setFocus("approverInput", form);
          return false;
        }

        if (comment == null || comment.length < 1) {
          DomUtils.setFocus("comment", form);
          return false;
        }

        const result = await app.adminApi.appRenew(
          { id: data.id, months, requester, approver, comment },
          { showLoading: false }
        );

        if (result == null) return;

        if (result.ok) {
          app.notifier.succeed(title, undefined, callback);
          return;
        }

        app.alertResult(result);
        return false;
      },
      inputs: <RenewAppUI orgId={data.orgId} />,
      fullScreen: app.smDown
    });
  }
}
